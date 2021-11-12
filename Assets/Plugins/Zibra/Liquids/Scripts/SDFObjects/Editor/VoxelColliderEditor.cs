#if ZIBRA_LIQUID_PAID_VERSION

using com.zibra.liquid.DataStructures;
using com.zibra.liquid.SDFObjects;
using System;
using com.zibra.liquid.Utilities;
using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;

namespace com.zibra.liquid.Editor.SDFObjects
{
    [CustomEditor(typeof(VoxelCollider), true)]
    public class VoxelColliderEditor : UnityEditor.Editor
    {
        // Limits for representation generation web requests
        private const uint REQUEST_TRIANGLE_COUNT_LIMIT = 100000;
        private const uint REQUEST_SIZE_LIMIT = 3 << 20; // 3mb

        [HideInInspector]
        [SerializeField]
        private Vector3[] VertexCachedBuffer;
        private Bounds MeshBounds;
        private VoxelCollider VoxelCollider;
        private MeshFilter MeshFilter;
        private SerializedProperty ForceInteraction;
        private UnityWebRequest CurrentRequest;

        public event Action<VoxelRepresentation> OnGenerated;
        public bool IsGenerating => CurrentRequest != null;

        protected void Awake()
        {
            ZibraServerAuthenticationManager.GetInstance().Initialize();

            if (!EditorApplication.isPlaying && VoxelCollider != null)
            {
                var mesh = VoxelCollider.GetMesh();

                if (mesh != null)
                {
                    VertexCachedBuffer = new Vector3[mesh.vertices.Length];
                    Array.Copy(mesh.vertices, VertexCachedBuffer, mesh.vertices.Length);
                    EditorUtility.SetDirty(this);
                }
            }
        }

        protected void OnEnable()
        {
            VoxelCollider = serializedObject.targetObject as VoxelCollider;
            OnGenerated += FinishGeneration;
            EditorApplication.update += CheckMeshComponent;
            ForceInteraction = serializedObject.FindProperty("ForceInteraction");
        }
        public void CreateMeshBBCube()
        {
            Mesh mesh = VoxelCollider.GetComponent<MeshFilter>().sharedMesh;

            if (mesh == null)
            {
                return;
            }

            MeshBounds = mesh.bounds;

            VoxelCollider.BoundingBoxMax = MeshBounds.max;
            VoxelCollider.BoundingBoxMin = MeshBounds.min;

            Vector3 lengths = MeshBounds.size;
            float max_length = Math.Max(Math.Max(lengths.x, lengths.y), lengths.z);
            // for every direction (X,Y,Z)
            if (max_length != lengths.x)
            {
                float delta =
                    max_length - lengths.x; // compute difference between largest length and current (X,Y or Z) length
                VoxelCollider.BoundingBoxMin.x =
                    MeshBounds.min.x - (delta / 2.0f); // pad with half the difference before current min
                VoxelCollider.BoundingBoxMax.x =
                    MeshBounds.max.x + (delta / 2.0f); // pad with half the difference behind current max
            }

            if (max_length != lengths.y)
            {
                float delta =
                    max_length - lengths.y; // compute difference between largest length and current (X,Y or Z) length
                VoxelCollider.BoundingBoxMin.y =
                    MeshBounds.min.y - (delta / 2.0f); // pad with half the difference before current min
                VoxelCollider.BoundingBoxMax.y =
                    MeshBounds.max.y + (delta / 2.0f); // pad with half the difference behind current max
            }

            if (max_length != lengths.z)
            {
                float delta =
                    max_length - lengths.z; // compute difference between largest length and current (X,Y or Z) length
                VoxelCollider.BoundingBoxMin.z =
                    MeshBounds.min.z - (delta / 2.0f); // pad with half the difference before current min
                VoxelCollider.BoundingBoxMax.z =
                    MeshBounds.max.z + (delta / 2.0f); // pad with half the difference behind current max
            }

            // Next snippet adresses the problem reported here: https://github.com/Forceflow/cuda_voxelizer/issues/7
            // Suspected cause: If a triangle is axis-aligned and lies perfectly on a voxel edge, it sometimes gets
            // counted / not counted Probably due to a numerical instability (division by zero?) Ugly fix: we pad the
            // bounding box on all sides by 1/10001th of its total length, bringing all triangles ever so slightly
            // off-grid
            Vector3 epsilon = VoxelCollider.BoundingBoxMax - VoxelCollider.BoundingBoxMin;
            epsilon /= 10001.0f;
            VoxelCollider.BoundingBoxMin -= epsilon;
            VoxelCollider.BoundingBoxMax += epsilon;
        }

        private void CheckMeshComponent()
        {
            if (VoxelCollider == null)
                return;

            Renderer renderer = VoxelCollider.GetComponent<Renderer>();
            if (renderer == null)
            {
                EditorUtility.DisplayDialog(
                    "No Renderer attached",
                    "VoxelCollider requires MeshRenderer or SkinnedMeshRenderer " +
                        "attached to the same object to operate correctly. Make sure one of them is present before adding this component.",
                    "OK");
                DestroyImmediate(VoxelCollider);

                return;
            }

            if (renderer is MeshRenderer meshRenderer)
            {
                if (MeshFilter == null)
                {
                    MeshFilter = meshRenderer.GetComponent<MeshFilter>();

                    if (MeshFilter == null)
                    {
                        EditorUtility.DisplayDialog(
                            "MeshFilter not found",
                            "VoxelCollider requires MeshFilter component to be present " +
                                "on the object when used in combination with MeshRenderer. " +
                                "Ensure that MeshFilter is attached to the object you're adding this component to.",
                            "OK");
                        DestroyImmediate(VoxelCollider);

                        return;
                    }
                }
            }
            else if (!(renderer is SkinnedMeshRenderer))
            {
                EditorUtility.DisplayDialog(
                    "Unsupported Renderer type",
                    "VoxelCollider requires MeshRenderer or SkinnedMeshRenderer " +
                        "attached to the same object to operate correctly. Make sure one of them is present before adding this component.",
                    "OK");
                DestroyImmediate(VoxelCollider);

                return;
            }
        }

        private void FinishGeneration(VoxelRepresentation result)
        {
            EditorUtility.SetDirty(VoxelCollider);
            Repaint();
        }

        public override void OnInspectorGUI()
        {
            if (VoxelCollider == null)
            {
                return;
            }

            UpdateRequest();

            serializedObject.Update();
            if (ZibraServerAuthenticationManager.GetInstance().IsKeyRequestInProgress &&
                !ZibraServerAuthenticationManager.GetInstance().IsLicenseKeyValid)
            {
                GUILayout.Label("Licence key validation in progress");
                return;
            }

            if (IsGenerating)
            {
                GUILayout.Label("Generating collider representation. Please wait.");
            }
            else if (!VoxelCollider.HasRepresentation)
            {
                EditorGUILayout.HelpBox(
                    "Representation is absent. Generate it using the button below before entering play mode.",
                    MessageType.Warning);
            }
            else
            {
                GUILayout.Label("Representation is present. Collider can be used now.");
            }

            string generateText = "Generate Representation";
            if (VoxelCollider.HasRepresentation)
            {
                generateText = "Regenerate Representation";
            }
            if (IsGenerating)
            {
                generateText = "Restart Generation";
            }
            if (GUILayout.Button(generateText))
            {
                GenerateRepresentation();
            }

            if (IsGenerating && GUILayout.Button("Stop Generation"))
            {
                AbortGeneration();
            }

            GUILayout.Space(10);

            if (VoxelCollider.GetComponent<Collider>() == null && GUILayout.Button("Add Unity Collider"))
            {
                VoxelCollider.gameObject.AddComponent<MeshCollider>();
            }

            EditorGUILayout.PropertyField(ForceInteraction);

            if (VoxelCollider.ForceInteraction && VoxelCollider.GetComponent<Rigidbody>() == null &&
                GUILayout.Button("Add Unity Rigidbody"))
            {
                VoxelCollider.gameObject.AddComponent<Rigidbody>();
            }

            if (!IsGenerating && VoxelCollider.HasRepresentation)
            {
                GUILayout.Label(
                    $"Approximate VRAM footprint:{(float)VoxelCollider.GetMemoryFootrpint() / (1 << 20):N2}MB");
            }

            serializedObject.ApplyModifiedProperties();
        }

        protected void OnDisable()
        {
            OnGenerated -= FinishGeneration;
            EditorApplication.update -= CheckMeshComponent;
        }

        public void GenerateRepresentation()
        {
            var mesh = VoxelCollider.GetMesh();

            if (mesh == null)
            {
                return;
            }

            if (mesh.triangles.Length / 3 > REQUEST_TRIANGLE_COUNT_LIMIT)
            {
                string errorMessage =
                    $"Mesh is too large. Can't generate representation. Triangle count should not exceed {REQUEST_TRIANGLE_COUNT_LIMIT} triangles, but current mesh have {mesh.triangles.Length / 3} triangles";
                EditorUtility.DisplayDialog("ZibraLiquid Error.", errorMessage, "OK");
                Debug.LogError(errorMessage);
                return;
            }

            if (!EditorApplication.isPlaying)
            {
                VertexCachedBuffer = new Vector3[mesh.vertices.Length];
                Array.Copy(mesh.vertices, VertexCachedBuffer, mesh.vertices.Length);
                EditorUtility.SetDirty(this);
            }

            var meshRepresentation = new MeshRepresentation { vertices = mesh.vertices.Vector3ToString(),
                                                              faces = mesh.triangles.IntToString() };

            if (CurrentRequest != null)
            {
                CurrentRequest.Dispose();
                CurrentRequest = null;
            }

            var json = JsonUtility.ToJson(meshRepresentation);

            // 25 megabytes is current limit for representation generation request
            if (json.Length > REQUEST_SIZE_LIMIT)
            {
                string errorMessage =
                    $"Mesh is too large. Can't generate representation. Please decrease vertex/triangle count. Web request should not exceed {REQUEST_SIZE_LIMIT / (1 << 20):N2}mb, but for current mesh {(float)json.Length / (1 << 20):N2}mb is needed.";
                EditorUtility.DisplayDialog("ZibraLiquid Error.", errorMessage, "OK");
                Debug.LogError(errorMessage);
                return;
            }

            if (ZibraServerAuthenticationManager.GetInstance().IsLicenseKeyValid)
            {
                string requestURL = ZibraServerAuthenticationManager.GetInstance().GenerationURL;

                if (requestURL != "")
                {
                    CurrentRequest = UnityWebRequest.Post(requestURL, json);
                    CurrentRequest.SendWebRequest();
                }
            }
            else
            {
                EditorUtility.DisplayDialog("Zibra Liquid Error",
                                            ZibraServerAuthenticationManager.GetInstance().ErrorText, "Ok");
                Debug.LogError(ZibraServerAuthenticationManager.GetInstance().ErrorText);
            }
        }

        public void AbortGeneration()
        {
            if (CurrentRequest != null)
            {
                CurrentRequest.Dispose();
                CurrentRequest = null;
            }
        }

        private void UpdateRequest()
        {
            if (CurrentRequest != null && CurrentRequest.isDone)
            {
                VoxelRepresentation newRepresentation = null;

                if (CurrentRequest.isDone && !CurrentRequest.isHttpError && !CurrentRequest.isNetworkError)
                {
                    var json = CurrentRequest.downloadHandler.text;
                    newRepresentation = JsonUtility.FromJson<VoxelRepresentation>(json);

                    if (string.IsNullOrEmpty(newRepresentation.embeds) ||
                        string.IsNullOrEmpty(newRepresentation.vox_ids) || newRepresentation.shape <= 0)
                    {
                        EditorUtility.DisplayDialog("Zibra Liquid Server Error",
                                                    "Server returned empty result. Connect ZibraLiquid support", "Ok");
                        Debug.LogError("Server returned empty result. Connect ZibraLiquid support");
                    }
                }
                else
                {
                    EditorUtility.DisplayDialog("Zibra Liquid Server Error", CurrentRequest.downloadHandler.text, "Ok");
                    Debug.LogError(CurrentRequest.downloadHandler.text);
                }

                CurrentRequest.Dispose();
                CurrentRequest = null;

                VoxelCollider.currentRepresentation = newRepresentation;
                CreateMeshBBCube();
                VoxelCollider.CreateRepresentation();

                OnGenerated?.Invoke(newRepresentation);
            }
        }
    }
}
#endif
