#if ZIBRA_LIQUID_PAID_VERSION

using com.zibra.liquid.DataStructures;
using com.zibra.liquid.Solver;
using com.zibra.liquid.Utilities;
using com.zibra.liquid;
using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace com.zibra.liquid.SDFObjects
{
    [ExecuteInEditMode] // Careful! This makes script execute in edit mode.
    // Use "EditorApplication.isPlaying" for play mode only check.
    // Encase this check and "using UnityEditor" in "#if UNITY_EDITOR" preprocessor directive to prevent build errors
    [AddComponentMenu("Zibra/Zibra Voxel Collider")]
    public class VoxelCollider : SDFCollider
    {
        private const int EMBED_COORDINATES_COUNT = 3;
        private const int GRID_SIZE = 32 * 32 * 32;
        private const int EMBEDDING_SIZE = 29;

        private int VoxelCount;

        [SerializeField]
        public Vector3 BoundingBoxMin;
        [SerializeField]
        public Vector3 BoundingBoxMax;

        [SerializeField]
        public VoxelRepresentation currentRepresentation = null;

        public VoxelRepresentation CurrentRepresentation => currentRepresentation;
        [HideInInspector]
        public bool HasRepresentation;

        private ComputeBuffer[] VoxelIDGrid;
        private ComputeBuffer VoxelPositions;
        private ComputeBuffer VoxelEmbeddings;

        private VoxelEmbedding[] VoxelInfo;

        public void CreateRepresentation()
        {
            VoxelInfo = UnpackRepresentation();

            if (VoxelInfo != null)
            {
                HasRepresentation = true;
            }
        }

        public void Initialize(int InstanceID)
        {
            if (IsInitialized.Contains(InstanceID))
                return;

            if (IsInitialized.Count == 0) // if has not been initialized at all
            {
                ColliderIndex = AllColliders.IndexOf(this);
                HasRepresentation = false;
                CreateRepresentation();

#if UNITY_EDITOR
                if (!EditorApplication.isPlaying)
                {
                    return;
                }
#endif

                // initialize compute buffers
                VoxelIDGrid = new ComputeBuffer[2];
                VoxelIDGrid[0] = new ComputeBuffer(GRID_SIZE, 4 * sizeof(float));
                VoxelIDGrid[1] = new ComputeBuffer(GRID_SIZE, 4 * sizeof(float));
                VoxelPositions = new ComputeBuffer(VoxelCount, 4 * sizeof(int));
                VoxelEmbeddings = new ComputeBuffer(EMBEDDING_SIZE * VoxelCount, sizeof(float));

                float[] EmbeddingsArray = new float[] {};
                int[] VoxPositionArray = new int[] {};

                Array.Resize<float>(ref EmbeddingsArray, VoxelCount * EMBEDDING_SIZE);
                Array.Resize<int>(ref VoxPositionArray, VoxelCount * 4);

                for (var i = 0; i < VoxelCount; i++)
                {
                    VoxelEmbedding cur = VoxelInfo[i];

                    Array.Copy(cur.embedding, 0, EmbeddingsArray, i * EMBEDDING_SIZE, EMBEDDING_SIZE);
                    VoxPositionArray[i * 4 + 0] = cur.coords.x;
                    VoxPositionArray[i * 4 + 1] = cur.coords.y;
                    VoxPositionArray[i * 4 + 2] = cur.coords.z;
                }

                colliderParams.CurrentID = ColliderIndex;
                colliderParams.VoxelNum = VoxelCount;
                colliderParams.BBoxMin = BoundingBoxMin;
                colliderParams.BBoxMax = BoundingBoxMax;
                colliderParams.colliderIndex = ColliderIndex;

                CommandBuffer VoxelColliderCreation = new CommandBuffer();

                // Have to be before RegisterVoxelCollider
                // Otherwise it breaks Metal
                VoxelPositions.SetData(VoxPositionArray);
                VoxelEmbeddings.SetData(EmbeddingsArray);

                Marshal.StructureToPtr(colliderParams, NativeDataPtr, true);

                ZibraLiquidBridge.RegisterVoxelCollider(
                    InstanceID, VoxelIDGrid[0].GetNativeBufferPtr(), VoxelIDGrid[1].GetNativeBufferPtr(),
                    VoxelPositions.GetNativeBufferPtr(), VoxelEmbeddings.GetNativeBufferPtr(), VoxelCount,
                    ColliderIndex);

                VoxelColliderCreation.IssuePluginEventAndData(
                    ZibraLiquidBridge.RunSDFShaderWithDataPtr(),
                    ZibraLiquidBridge.EventAndInstanceID(ZibraLiquidBridge.EventID.PrepareSDF, InstanceID),
                    NativeDataPtr);

                Graphics.ExecuteCommandBuffer(VoxelColliderCreation);
            }
            else // If was initialized more than 0 times then just register the buffers
            {
                ZibraLiquidBridge.RegisterVoxelCollider(
                    InstanceID, VoxelIDGrid[0].GetNativeBufferPtr(), VoxelIDGrid[1].GetNativeBufferPtr(),
                    VoxelPositions.GetNativeBufferPtr(), VoxelEmbeddings.GetNativeBufferPtr(), VoxelCount,
                    ColliderIndex);
            }

            IsInitialized.Add(InstanceID);
        }

        // on game start
        protected void Start()
        {
            colliderParams = new ColliderParams();
            NativeDataPtr = Marshal.AllocHGlobal(Marshal.SizeOf(colliderParams));
        }

        public VoxelEmbedding[] UnpackRepresentation()
        {
            if (currentRepresentation == null || string.IsNullOrEmpty(currentRepresentation.embeds) ||
                string.IsNullOrEmpty(currentRepresentation.vox_ids))
            {
                return null;
            }

            var embeddings = currentRepresentation.embeds.StringToFloat();
            var voxIds = currentRepresentation.vox_ids.StringToInt();

            VoxelCount = voxIds.Length / EMBED_COORDINATES_COUNT;

            if (currentRepresentation.shape == 0 || currentRepresentation.shape != EMBEDDING_SIZE ||
                (embeddings.Length % currentRepresentation.shape) != 0 ||
                (voxIds.Length % EMBED_COORDINATES_COUNT) != 0 ||
                (embeddings.Length / currentRepresentation.shape) != (voxIds.Length / EMBED_COORDINATES_COUNT))
            {
                Debug.LogError("Incorrect data format after parsing base64 strings");
                return null;
            }

            var unpackedRepresentation = new VoxelEmbedding[VoxelCount];

            for (var i = 0; i < unpackedRepresentation.Length; i++)
            {
                var currentEmbedding = new float[currentRepresentation.shape];
                Array.Copy(embeddings, i * currentRepresentation.shape, currentEmbedding, 0,
                           currentRepresentation.shape);
                unpackedRepresentation[i] =
                    new VoxelEmbedding() { coords = new Vector3Int(voxIds[i * EMBED_COORDINATES_COUNT],
                                                                   voxIds[i * EMBED_COORDINATES_COUNT + 1],
                                                                   voxIds[i * EMBED_COORDINATES_COUNT + 2]),
                                           embedding = currentEmbedding };
            }

            return unpackedRepresentation;
        }

        public Mesh GetMesh()
        {
            Renderer currentRenderer = GetComponent<Renderer>();

            if (currentRenderer == null)
            {
#if UNITY_EDITOR
                EditorUtility.DisplayDialog(
                    "Zibra Liquid Mesh Error",
                    "Render component absent on this object. " +
                        "Add this component only to objects with MeshFilter or SkinnedMeshRenderer components",
                    "Ok");
#endif
                return null;
            }

            if (currentRenderer is MeshRenderer meshRenderer)
            {
                var MeshFilter = meshRenderer.GetComponent<MeshFilter>();

                if (MeshFilter == null)
                {
#if UNITY_EDITOR
                    EditorUtility.DisplayDialog(
                        "Zibra Liquid Mesh Error",
                        "MeshFilter absent on this object. MeshRenderer requires MeshFilter to operate correctly.",
                        "Ok");
#endif
                    return null;
                }

                if (MeshFilter.sharedMesh == null)
                {
#if UNITY_EDITOR
                    EditorUtility.DisplayDialog(
                        "Zibra Liquid Mesh Error",
                        "No mesh found on this object. Attach mesh to the MeshFilter before generating representation.",
                        "Ok");
#endif
                    return null;
                }

                return MeshFilter.sharedMesh;
            }

            if (currentRenderer is SkinnedMeshRenderer skinnedMeshRenderer)
            {
                var mesh = new Mesh();
                skinnedMeshRenderer.BakeMesh(mesh);

                return mesh;
            }

#if UNITY_EDITOR
            EditorUtility.DisplayDialog(
                "Zibra Liquid Mesh Error",
                "Unsupported Renderer type. Only MeshRenderer and SkinnedMeshRenderer are supported at the moment.",
                "Ok");
#endif
            return null;
        }

        protected void OnDestroy()
        {
            if (IsInitialized.Count > 0)
            {
                if (VoxelIDGrid != null)
                {
                    VoxelIDGrid[0]?.Release();
                    VoxelIDGrid[1]?.Release();
                }
                VoxelPositions?.Release();
                VoxelEmbeddings?.Release();

                IsInitialized.Clear();
            }
        }

        private void ComputeSDF_Shader(int InstanceID, CommandBuffer SolverCommandBuffer, ComputeBuffer Coordinates,
                                       ComputeBuffer Output, ComputeBuffer OutputID, int ID, int Elements,
                                       CalculationType type)
        {
            Initialize(InstanceID);

            Vector3 position = transform.position;
            Quaternion rotation = transform.rotation;
            Vector3 scale = transform.lossyScale;
            Vector3 newpos =
                position; // + *rotation*new Vector3(meshcenter.x*scale.x, meshcenter.y*scale.y, meshcenter.z*scale.z);

            colliderParams.Position = newpos;
            colliderParams.Rotation = new Vector4(rotation.x, rotation.y, rotation.z, rotation.w);
            colliderParams.Scale = scale;
            colliderParams.Elements = Elements;
            colliderParams.OpType = (int)type;
            colliderParams.CurrentID = ID;
            colliderParams.VoxelNum = VoxelCount;
            colliderParams.BBoxMin = BoundingBoxMin;
            colliderParams.BBoxMax = BoundingBoxMax;
            colliderParams.colliderIndex = ColliderIndex;

            Marshal.StructureToPtr(colliderParams, NativeDataPtr, true);
            SolverCommandBuffer.IssuePluginEventAndData(
                ZibraLiquidBridge.RunSDFShaderWithDataPtr(),
                ZibraLiquidBridge.EventAndInstanceID(ZibraLiquidBridge.EventID.ComputeNeuralSDF, InstanceID),
                NativeDataPtr);
        }

        /*Compute the SDF and unite it with the SDF in Output*/
        public override void ComputeSDF_Unite(int InstanceID, CommandBuffer SolverCommandBuffer,
                                              ComputeBuffer Coordinates, ComputeBuffer Output, ComputeBuffer OutputID,
                                              int ID, int Elements)
        {
            ComputeSDF_Shader(InstanceID, SolverCommandBuffer, Coordinates, Output, OutputID, ID, Elements,
                              CalculationType.Unite);
        }
        public override int GetMemoryFootrpint()
        {
            int result = 0;
            if (currentRepresentation.vox_ids == null)
                return result;

            result += GRID_SIZE * 4 * sizeof(int);       // VoxelPositions
            result += 2 * GRID_SIZE * 4 * sizeof(float); // VoxelIDGrid
            VoxelCount = currentRepresentation.vox_ids.StringToInt().Length / EMBED_COORDINATES_COUNT;
            result += EMBEDDING_SIZE * VoxelCount * sizeof(float); // VoxelEmbeddings

            return result;
        }
    }
}

#endif