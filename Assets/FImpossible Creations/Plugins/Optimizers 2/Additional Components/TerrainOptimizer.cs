using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.FOptimizing
{
    /// <summary>
    /// FM: Optimizer class dedicated to optimizing terrain component
    /// </summary>
    [AddComponentMenu("FImpossible Creations/Optimizers 2/Terrain Optimizer",2)]
    public class TerrainOptimizer : ScriptableOptimizer, UnityEngine.EventSystems.IDropHandler, IFHierarchyIcon
    {
        #region Hierarchy icons
        new public string EditorIconPath { get { if (PlayerPrefs.GetInt("OptH", 1) == 0) return ""; else return "FIMSpace/Optimizers 2/OptTerrIconSmall"; } }
        new public void OnDrop(UnityEngine.EventSystems.PointerEventData data) { }
        #endregion


        [Tooltip("Target terrain component to be optimized")]
        public Terrain Terrain;
        public TerrainCollider TerrainCollider;
        [Tooltip("If you have island type terrain you can feel free to untoggle this, but if you're using continous terrain it should be toggled on")]
        public bool SafeBorders = true;

        [Range(0, 3)]
        [Tooltip("Amount of used raycast for checking distance to terrain if character is high above terrain (very small performance change)")]
        public int CheckQuality = 2;

        public int spheresInvisible = 0;

        protected override void Reset()
        {
            if (ToOptimize == null) ToOptimize = new List<ScriptableLODsController>();
            AddTerrainToOptimize();
            DrawAutoDistanceToggle = false;

            base.Reset();
            DrawDeactivateToggle = false;
            if (Terrain) DetectionRadius = Terrain.terrainData.size.x / 10f; else DetectionRadius = 80f;
            if (Terrain) MaxDistance = Terrain.terrainData.size.x * 1.5f; else MaxDistance = 1000f;
        }


        protected override void RefreshInitialSettingsForOptimized()
        {
            base.RefreshInitialSettingsForOptimized();
            AddToContainer = false;
        }


        #region Culling Operations


        protected override void InitCullingGroups(float[] distances, float detectionSphereRadius = 2.5F, Camera targetCamera = null)
        {
            InitBaseCullingVariables(targetCamera);

            DistanceLevels = new float[distances.Length + 2];
            DistanceLevels[0] = Mathf.Epsilon; // I'm disappointed I have to use additional distance to allow detect initial culling event catch everything

            for (int i = 1; i < distances.Length + 1; i++) DistanceLevels[i] = distances[i - 1];

            // Additional distance level to be able detecting frustum ranges, instead of frustum with distance ranges combined
            DistanceLevels[DistanceLevels.Length - 1] = distances[distances.Length - 1] * 2;

            distancePoint = transform.position;
            CullingGroup = new CullingGroup { targetCamera = targetCamera };


            visibilitySpheres = GetBoundingSpheres();
            mainVisibilitySphere = visibilitySpheres[0];
            sphereState = new int[visibilitySpheres.Length];
            for (int i = 0; i < sphereState.Length; i++) sphereState[i] = 0;
            spheresWithLOD = new int[LODLevels + 2];
            spheresWithLOD[1] = visibilitySpheres.Length;

            CullingGroup.SetBoundingSpheres(visibilitySpheres);
            CullingGroup.SetBoundingSphereCount(visibilitySpheres.Length);

            CullingGroup.onStateChanged = CullingGroupStateChanged;

            CullingGroup.SetBoundingDistances(DistanceLevels);
            CullingGroup.SetDistanceReferencePoint(targetCamera.transform);

            spheresVisible = 0;
            spheresInvisible = visibilitySpheres.Length;

            distancePoint = GetTerrainCenter();
        }


        public override void CullingGroupStateChanged(CullingGroupEvent cullingEvent)
        {
            int distInd = cullingEvent.currentDistance; if (distInd == 0) distInd = 1; if (distInd >= spheresWithLOD.Length) distInd = spheresWithLOD.Length - 1;
            sphereState[cullingEvent.index] = distInd;

            int preInd = cullingEvent.previousDistance; if (preInd == 0) preInd = 1; if (preInd >= spheresWithLOD.Length) preInd = spheresWithLOD.Length - 1;

            spheresWithLOD[preInd]--;
            spheresWithLOD[distInd]++;

            //bool? steppedOutVisible = null;
            if (cullingEvent.hasBecomeInvisible) { /*if (spheresVisible == 1) steppedOutVisible = true;*/ spheresInvisible++; spheresVisible--; }
            if (cullingEvent.hasBecomeVisible) { /*if (spheresInvisible == 1) steppedOutVisible = false;*/ spheresInvisible--; spheresVisible++; }

            int nearest = 0;
            for (int i = spheresWithLOD.Length - 1; i >= 0; i--)
                if (spheresWithLOD[i] > 0) nearest = i;

            if (nearest == 0) nearest = 1;

            nearestDistanceLevel = nearest;

            if (nearestDistanceLevel > DistanceLevels.Length - 2)
            {
                OutOfDistance = true;
                if (nearestDistanceLevel > DistanceLevels.Length - 1) FarAway = true; else FarAway = false;
            }
            else
            {
                OutOfDistance = false;
                FarAway = false;
            }

#if UNITY_EDITOR
            int nearestI = 0;
            float nearDist = float.MaxValue;

            for (int i = 0; i < sphereState.Length; i++)
            {
                if (sphereState[i] == nearestDistanceLevel)
                {
                    float dist = Vector3.Distance(visibilitySpheres[i].position, TargetCamera.position);
                    if (dist < nearDist)
                    {
                        nearestI = i;
                        nearDist = dist;
                    }
                }
            }

            //nearestDistance = Mathf.Max(0f, nearDist - DetectionRadius);
            distancePoint = visibilitySpheres[nearestI].position;
#endif

            if (spheresVisible == 0) OutOfCameraView = true; else OutOfCameraView = false;

            bool changeOccured = false;
            if (preNearestDistanceLevel != nearestDistanceLevel) changeOccured = true;
            else
            {
                if (WasOutOfCameraView != OutOfCameraView) changeOccured = true;
                else
                {
                    if (WasHidden != IsHidden) changeOccured = true;
                }
            }

            if (changeOccured)
            {
                RefreshVisibilityState(nearestDistanceLevel - 1);
                preNearestDistanceLevel = nearestDistanceLevel;
            }
        }


        protected BoundingSphere[] GetBoundingSpheres()
        {
            if (Terrain == null) return null;

            List<BoundingSphere> spheres = new List<BoundingSphere>();

            float width = this.Terrain.terrainData.size.x;
            int xSteps = Mathf.RoundToInt(width / (DetectionRadius * 2));

            float length = this.Terrain.terrainData.size.z;
            int zSteps = Mathf.RoundToInt(length / (DetectionRadius * 2));

            float sphereSize = (xSteps * (DetectionRadius * 2f)) / width;
            sphereSize = (1f - sphereSize) + 1;
            sphereSize *= DetectionRadius * 2f;

            float max = xSteps * zSteps + zSteps + 1;
            float i = 0;

            for (int x = 0; x <= xSteps; x++)
                for (int z = 0; z <= zSteps; z++)
                {
                    // Main Sphere
                    Vector3 pos = Terrain.GetPosition();
                    pos += Vector3.right * (float)x * sphereSize + Vector3.right * DetectionRadius;
                    pos += Vector3.forward * (float)z * sphereSize + Vector3.forward * DetectionRadius;
                    pos.y = Terrain.SampleHeight(pos);

                    Color sColor = Color.HSVToRGB(i / max, 0.9f, 0.8f); sColor.a = GizmosAlpha;
                    Gizmos.color = sColor;

                    if (z != zSteps && x != xSteps)
                        spheres.Add(new BoundingSphere(pos, DetectionRadius));

                    // Fill Sphere
                    pos -= Vector3.right * DetectionRadius * 1f;
                    pos -= Vector3.forward * DetectionRadius * 1f;
                    pos.y = Terrain.SampleHeight(pos);

                    if (SafeBorders)
                        spheres.Add(new BoundingSphere(pos, DetectionRadius));
                    else
                    {
                        if (z != zSteps && x != xSteps && x != 0 && z != 0)
                            spheres.Add(new BoundingSphere(pos, DetectionRadius));
                    }

                    i++;
                }

            return spheres.ToArray();
        }


        public override Vector3 GetReferencePosition()
        {
            return distancePoint;
        }

        #endregion


        #region Terrain Methods


        private bool IsTargetOutside()
        {
            return false;
        }


        private Vector3 GetNearestPointOnTerrain(Vector3 from)
        {
            return Vector3.zero;
        }


        #endregion


        #region Utilities


        private void RefreshTerrainComponents()
        {
            if (!Terrain) Terrain = GetComponentInChildren<Terrain>();
            if (Terrain) TerrainCollider = Terrain.GetComponent<TerrainCollider>();
        }


        private void AddTerrainToOptimize()
        {
            RefreshTerrainComponents();

            if (ToOptimize.Count == 0)
            {
                TryAddLODControllerFor(LoadLODReference("Optimizers/Base/FLOD_Terrain Reference"), gameObject.transform, null);
            }
            else
                if (ToOptimize[0] == null)
                {
                    ScriptableLODsController controller = LoadLODReference("Optimizers/Base/FLOD_Terrain Reference").GenerateLODController(transform, this);
                    if (controller != null) ToOptimize[0] = controller;
                }
        }

        /// <summary>
        /// Checking if correct components are assigned to optimizer
        /// </summary>
        private bool HaveTerrain()
        {
            if (!Terrain)
            {
                RefreshTerrainComponents();
                if (!Terrain)
                {
                    Debug.LogError("[OPTIMIZERS] No terrain attached to Optimizer component on object " + name);
                    return false;
                }
                else if (!TerrainCollider)
                {
                    Debug.LogError("[OPTIMIZERS] Terrain don't have Terrain Collider! (" + name + ")");
                    return false;
                }
            }

            if (!Terrain)
                return false;
            else
                return true;
        }

        public float LimitRadius(float value)
        {
            if (HaveTerrain()) if (Terrain.terrainData != null) if (value < Terrain.terrainData.size.x / 40) value = Terrain.terrainData.size.x / 40;
            return value;
        }

        private Vector3 GetTerrainCenter()
        {
            if (Terrain == null) return Vector3.zero;
            return Terrain.GetPosition() + Vector3.right * (Terrain.terrainData.size.x / 2) + Vector3.forward * (Terrain.terrainData.size.z / 2);
        }

        private float GetMinRadius()
        {
            if (Terrain == null) return 0f;
            return Vector3.Distance(Terrain.GetPosition(), GetTerrainCenter()) + (SafeBorders ? DetectionRadius : 0);
        }


        #endregion


        #region Editor Stuff


        public override void OnValidate()
        {
            if (ToOptimize == null) ToOptimize = new List<ScriptableLODsController>();
            AddTerrainToOptimize();

            DrawAutoDistanceToggle = false;
            CullIfNotSee = true;
            Hideable = true;
            HiddenCullAt = -1;
            LimitLODLevels = 5;
            DeactivateObject = false;
            DrawDeactivateToggle = false;

            base.OnValidate();
            DetectionRadius = LimitRadius(DetectionRadius);
        }

        protected override void OnValidateCheckForStatic()
        {
            OptimizingMethod = EOptimizingMethod.Static;
        }

#if UNITY_EDITOR

        protected override void OnDrawGizmosSelected()
        {
            if (gameObject.activeInHierarchy == false) return;
            if (DetectionRadius < 10f) return;

            RefreshTerrainComponents();

            if (!Application.isPlaying) visibilitySpheres = GetBoundingSpheres();
            else
                if (TargetCamera)
            {
                Gizmos.DrawLine(TargetCamera.position, distancePoint);

                //for (int i = 0; i < visibilitySpheres.Length; i++)
                //{
                //    string text = "[" + i + "]" + " dist: " + sphereState[i] + " distance: " + (Vector3.Distance(targetCamera.position, visibilitySpheres[i].position) - DetectionRadius);
                //    UnityEditor.Handles.Label(visibilitySpheres[i].position, text);
                //}
            }

            if (visibilitySpheres != null)
            {
                float alpha = GizmosAlpha;

                alpha = Mathf.Lerp(0.2f, 1.0f, Mathf.InverseLerp(0.5f, 1f, GizmosAlpha));
                if (GizmosAlpha <= 0.5f) alpha = Mathf.Lerp(0.0f, .2f, Mathf.InverseLerp(0.0f, 0.5f, GizmosAlpha));

                float smallRadius = mainVisibilitySphere.radius * 0.075f;
                for (int i = 0; i < visibilitySpheres.Length; i++)
                {
                    Color sColor = Color.HSVToRGB((float)i / (float)visibilitySpheres.Length, 0.9f, 0.8f); sColor.a = alpha * 0.7f;
                    Gizmos.DrawWireSphere(visibilitySpheres[i].position, visibilitySpheres[i].radius);
                    Gizmos.color = sColor * new Color(1.15f, 1.15f, 1.15f, 0.85f);
                    Gizmos.DrawSphere(visibilitySpheres[i].position, smallRadius);
                }
            }


            if (isResizing == -1)
            {
                if (isSelected == LODLevels)
                {
                    GUI.color = culledLODColor;
                    DrawTerrainSphere(MaxDistance, MaxDistance);
                }
                else
                    for (int i = 0; i < LODLevels; i++)
                    {
                        if (isSelected >= 0) if (i > isSelected + 1 && i != LODLevels) continue;

                        Color lodColor = lODColors[i];
                        lodColor.a = i == isSelected ? 0.9f * GizmosAlpha : 0.15f * GizmosAlpha;
                        Gizmos.color = lodColor;

                        if (i >= LODPercent.Count) continue;

                        float radius = MaxDistance * LODPercent[i];

                        float inRadius = radius;
                        if (i != 0) inRadius = radius - (MaxDistance * LODPercent[i - 1]);
                        else
                            inRadius = radius;

                        if (i == isSelected)
                            DrawTerrainSphere(radius, -inRadius);
                        else
                            DrawTerrainSphere(radius, 0f);
                    }
            }
            else // when resizing sphere
            {
                Color lodColor = lODColors[isResizing];
                lodColor.a = 0.9f * GizmosAlpha;
                Gizmos.color = lodColor;

                float radius = MaxDistance * LODPercent[isResizing];
                Gizmos.color = lodColor * new Color(1f, 1f, 1f, 0.2f);

                // Drawing lines to help visualize LOD area range
                float inRadius = radius;
                Gizmos.color = lodColor * new Color(1f, 1f, 1f, 0.24f);

                if (isResizing != 0) inRadius = radius - (MaxDistance * LODPercent[isResizing - 1]);
                else
                    inRadius = radius;

                DrawTerrainSphere(radius, -inRadius);
            }

            Gizmos.color = culledLODColor * new Color(1f, 1f, 1f, GizmosAlpha);
            DrawTerrainSphere(MaxDistance + 1f);

            if (!Application.isPlaying)
            {
                Vector3 infoDir = new Vector3(1f, 1f, 0f).normalized;
                UnityEditor.Handles.Label(GetTerrainCenter() + transform.TransformDirection(infoDir * (GetMinRadius() + MaxDistance * LODPercent[0])), new GUIContent("[i]", "This frames in different colors indicates distance levels from terrain for Level Of Details (LODs)"));
            }
        }

#endif

        private void DrawTerrainSphere(float radius, float inRadius = 0f)
        {
            if (Terrain == null) return;

            float width = this.Terrain.terrainData.size.x;
            int xSteps = Mathf.RoundToInt(width / (DetectionRadius * 2));

            float length = this.Terrain.terrainData.size.z;
            int zSteps = Mathf.RoundToInt(length / (DetectionRadius * 2));

            Vector3 diagLF = new Vector3(-1f, 0f, 1f).normalized;
            Vector3 diagRF = new Vector3(1f, 0f, 1f).normalized;

            float move = 0f;
            if (SafeBorders)
            {
                move += DetectionRadius;
                xSteps += 1;
                zSteps += 1;

                xSteps /= 2;
                zSteps /= 2;
            }


            List<Vector3> v = new List<Vector3>();// vertexes
            v.Add(Terrain.GetPosition() + Vector3.left * move + Vector3.left * radius);
            v.Add(Terrain.GetPosition() + Vector3.left * move + Vector3.left * radius + Vector3.forward * Terrain.terrainData.size.z);

            v.Add(Terrain.GetPosition() + Vector3.forward * Terrain.terrainData.size.z + diagLF * move + diagLF * radius);

            v.Add(Terrain.GetPosition() + Vector3.forward * move + Vector3.forward * radius + Vector3.forward * Terrain.terrainData.size.z);
            v.Add(Terrain.GetPosition() + Vector3.forward * move + Vector3.forward * radius + Vector3.forward * Terrain.terrainData.size.z + Vector3.right * Terrain.terrainData.size.x);

            v.Add(Terrain.GetPosition() + Vector3.forward * Terrain.terrainData.size.z + diagRF * move + diagRF * radius + Vector3.right * Terrain.terrainData.size.x);

            v.Add(Terrain.GetPosition() + Vector3.right * move + Vector3.right * radius + Vector3.right * Terrain.terrainData.size.x + Vector3.forward * Terrain.terrainData.size.z);
            v.Add(Terrain.GetPosition() + Vector3.right * move + Vector3.right * radius + Vector3.right * Terrain.terrainData.size.x);

            v.Add(Terrain.GetPosition() - diagLF * move - diagLF * radius + Vector3.right * Terrain.terrainData.size.x);

            v.Add(Terrain.GetPosition() + Vector3.back * move + Vector3.back * radius + Vector3.right * Terrain.terrainData.size.x);
            v.Add(Terrain.GetPosition() + Vector3.back * move + Vector3.back * radius);

            v.Add(Terrain.GetPosition() - diagRF * move - diagRF * radius);

            DrawVertices(v, Vector3.zero);


            if (inRadius != 0f)
            {
                float step = Terrain.terrainData.size.z / (float)zSteps;
                Vector3 sideLine = Terrain.GetPosition() + Vector3.left * move;
                for (int i = 0; i <= zSteps; i++)
                    Gizmos.DrawRay(sideLine + Vector3.forward * step * i + Vector3.left * radius, Vector3.left * inRadius);

                sideLine = Terrain.GetPosition() + Vector3.right * (Terrain.terrainData.size.x + move);
                for (int i = 0; i <= zSteps; i++)
                    Gizmos.DrawRay(sideLine + Vector3.forward * step * i + Vector3.right * radius, Vector3.right * inRadius);

                step = Terrain.terrainData.size.x / (float)xSteps;
                sideLine = Terrain.GetPosition() + Vector3.forward * (Terrain.terrainData.size.z + move);
                for (int i = 0; i <= xSteps; i++)
                    Gizmos.DrawRay(sideLine + Vector3.right * step * i + Vector3.forward * radius, Vector3.forward * inRadius);

                sideLine = Terrain.GetPosition() + Vector3.back * move;
                for (int i = 0; i <= xSteps; i++)
                    Gizmos.DrawRay(sideLine + Vector3.right * step * i + Vector3.back * radius, Vector3.back * inRadius);

                Gizmos.DrawRay(Terrain.GetPosition() - diagRF * move - diagRF * radius, diagRF * -inRadius);
                Gizmos.DrawRay(Terrain.GetPosition() + Vector3.forward * Terrain.terrainData.size.z + diagLF * move + diagLF * radius, diagLF * inRadius);
                Gizmos.DrawRay(Terrain.GetPosition() + Vector3.forward * Terrain.terrainData.size.z + diagRF * move + diagRF * radius + Vector3.right * Terrain.terrainData.size.x, diagRF * inRadius);
                Gizmos.DrawRay(Terrain.GetPosition() - diagLF * move - diagLF * radius + Vector3.right * Terrain.terrainData.size.x, diagLF * -inRadius);

                Color preCol = Gizmos.color;
                Gizmos.color = preCol * new Color(1f, 1f, 1f, 0.25f);
                DrawVertices(v, Vector3.up * radius + Vector3.up * move + Vector3.up * DetectionRadius);
                DrawVertices(v, Vector3.down * radius + Vector3.down * move + Vector3.down * DetectionRadius);
                DrawVerticesVert(v, Vector3.down * radius + Vector3.down * move + Vector3.down * DetectionRadius);
                Gizmos.color = preCol;
            }
        }

        private void DrawVertices(List<Vector3> v, Vector3 offset, bool vert = false)
        {
            for (int i = 1; i < v.Count; i++)
            {
                Gizmos.DrawLine(v[i - 1] + offset, v[i] + offset);
                if (vert) Gizmos.DrawRay(v[i - 1] + offset + Vector3.down * (MaxDistance + DetectionRadius), Vector3.up * (MaxDistance + DetectionRadius) * 2f);
            }

            Gizmos.DrawLine(v[v.Count - 1] + offset, v[0] + offset);
        }

        private void DrawVerticesVert(List<Vector3> v, Vector3 offset)
        {
            //float min = GetMinRadius();
            for (int i = 1; i < v.Count; i++)
            {
                Gizmos.DrawRay(v[i - 1] + offset, -offset * 2f);
            }

            Gizmos.DrawRay(v[v.Count - 1] + offset, -offset * 2f);
        }


        #endregion





    }
}