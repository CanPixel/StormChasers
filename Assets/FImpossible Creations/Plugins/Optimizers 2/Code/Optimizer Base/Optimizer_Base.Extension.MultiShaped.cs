using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.FOptimizing
{
    public abstract partial class Optimizer_Base
    {
        public bool UseMultiShape = false;

        [HideInInspector]
        [Range(0f, 1f)]
        [Tooltip("How many spheres should be created in auto detection process")]
        public float AutoPrecision = 0.25f;
        [HideInInspector]
        [Tooltip("[Optional] Mesh to create detection spheres on it's structure")]
        public Mesh AutoReferenceMesh;
        [HideInInspector]
        public bool DrawPositionHandles = true;
        [HideInInspector]
        public bool ScalingHandles = true;

        [HideInInspector]
        public List<MultiShapeBound> Shapes;

        [HideInInspector]
        public List<Vector3> ShapePos;
        [HideInInspector]
        public List<float> ShapeRadius;



        #region Multi spheres detection variables

        protected int nearestDistanceLevel = 0;
        protected int preNearestDistanceLevel = 0;
        protected int[] sphereState;
        protected int spheresVisible = 0;
        protected int[] spheresWithLOD;

        #endregion


        #region Culling Overrides etc.

        protected void InitCullingGroupsMultiShape(float[] distances, float detectionSphereRadius = 2.5F, Camera targetCamera = null)
        {
            distancePoint = transform.position;



            if (!AddToContainer) // Culling group just in optimizer component
            {
                DistanceLevels = new float[distances.Length + 2];
                DistanceLevels[0] = Mathf.Epsilon; // I'm disappointed I have to use additional distance to allow detect initial culling event catch everything

                for (int i = 1; i < distances.Length + 1; i++) DistanceLevels[i] = distances[i - 1];

                // Additional distance level to be able detecting frustum ranges, instead of frustum with distance ranges combined
                DistanceLevels[DistanceLevels.Length - 1] = distances[distances.Length - 1] * 2;

                CullingGroup = new CullingGroup { targetCamera = targetCamera };

                visibilitySpheres = GetBoundingSpheresMultiShape();
                sphereState = new int[visibilitySpheres.Length];
                mainVisibilitySphere = visibilitySpheres[0];

                for (int i = 0; i < sphereState.Length; i++) sphereState[i] = 0;
                spheresWithLOD = new int[LODLevels + 2];
                spheresWithLOD[1] = visibilitySpheres.Length;

                CullingGroup.SetBoundingSpheres(visibilitySpheres);
                CullingGroup.SetBoundingSphereCount(visibilitySpheres.Length);

                CullingGroup.onStateChanged = CullingGroupStateChangedMultiShape;

                CullingGroup.SetBoundingDistances(DistanceLevels);
                CullingGroup.SetDistanceReferencePoint(targetCamera.transform);

                spheresVisible = 0;
                //spheresInvisible = visibilitySpheres.Length;
            }
            else // Culling group in container
            {
                sphereState = new int[Shapes.Count];
                for (int i = 0; i < sphereState.Length; i++) sphereState[i] = 0;
                spheresWithLOD = new int[LODLevels + 2];
                spheresWithLOD[1] = LODLevels + 2;
                spheresVisible = 0;

                SetDistanceLevels(distances);
                OptimizersManager.Instance.AddToContainer(this);
            }

            float[] elements = GetCenterPosAndFarthest();
            distancePoint = new Vector3(elements[0], elements[1], elements[2]);
        }

        public void CullingGroupStateChangedMultiShape(CullingGroupEvent cullingEvent)
        {
            int multiIndex = cullingEvent.index;
            if (OwnerContainer != null) multiIndex = GetIndexForCullEventMultiShape(multiIndex);
            else if (UseMultiShape) return;

            int distInd = cullingEvent.currentDistance; if (distInd == 0) distInd = 1; if (distInd >= spheresWithLOD.Length) distInd = spheresWithLOD.Length - 1;
            sphereState[multiIndex] = distInd;

            int preInd = cullingEvent.previousDistance; if (preInd == 0) preInd = 1; if (preInd >= spheresWithLOD.Length) preInd = spheresWithLOD.Length - 1;

            spheresWithLOD[preInd]--;
            spheresWithLOD[distInd]++;

            if (cullingEvent.hasBecomeInvisible)
                spheresVisible--;

            if (cullingEvent.hasBecomeVisible)
                spheresVisible++;

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
                    float dist = Vector3.Distance(GetVisibilitySphere(i).position, TargetCamera.position);
                    if (dist < nearDist)
                    {
                        nearestI = i;
                        nearDist = dist;
                    }
                }
            }

            //nearestDistance = Mathf.Max(0f, nearDist - DetectionRadius);
            distancePoint = GetVisibilitySphere(nearestI).position;
#endif

            //if (spheresVisible == 0 && preVisible > 0)
            //{
            //    bool steppedOutRange = false;
            //    if (preInd == DistanceLevels.Length - 2 && distInd == DistanceLevels.Length - 1)
            //        steppedOutRange = true;

            //    if (!steppedOutRange) OutOfCameraView = true;
            //}



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
                RefreshVisibilityState(Mathf.Max(0, nearestDistanceLevel - 1));
                preNearestDistanceLevel = nearestDistanceLevel;
            }
        }

        public Vector3 GetReferencePositionMultiShape()
        {
            return distancePoint;
        }

        #endregion


        #region Editor stuff

        public void OnValidateMultiShape()
        {
            if (OptimizingMethod == EOptimizingMethod.Dynamic || OptimizingMethod == EOptimizingMethod.TriggerBased)
            {
                Debug.LogError("[OPTIMIZERS] Optimization Method " + OptimizingMethod + " is not supported by Complex Shape Component!");
                OptimizingMethod = EOptimizingMethod.Effective;
            }

            CullIfNotSee = true;
            Hideable = true;

            // Auto check for reference mesh
            if (!AutoReferenceMesh)
            {
                MeshFilter meshF = GetComponentInChildren<MeshFilter>();
                if (meshF) AutoReferenceMesh = meshF.sharedMesh;
                if (!AutoReferenceMesh)
                {
                    SkinnedMeshRenderer skin = gameObject.GetComponentInChildren<SkinnedMeshRenderer>();
                    if (skin) AutoReferenceMesh = skin.sharedMesh;
                }
            }

            if (ShapePos.Count > 0)
            {
                for (int i = 0; i < ShapePos.Count; i++)
                {
                    Shapes.Add(new MultiShapeBound());
                    Shapes[i].position = ShapePos[i];
                    Shapes[i].radius = ShapeRadius[i];
                }

                ShapePos.Clear();
                ShapeRadius.Clear();
            }
        }

        public void DynamicLODUpdateMiltiShape(EOptimizingDistance category, float distance)
        {
            PreviousPosition = visibilitySpheres[0].position + Vector3.right * moveTreshold * 2f; // To update positions every frame, not basing only on one detection sphere
        }

        protected void RefreshEffectiveCullingGroupsMultiShape()
        {
            if (!AddToContainer)
                for (int i = 0; i < Shapes.Count; i++)
                {
                    if (Shapes[i].transform == null)
                        visibilitySpheres[i].position = transform.TransformPoint(Shapes[i].position);
                    else
                        visibilitySpheres[i].position = Shapes[i].transform.TransformPoint(Shapes[i].position);
                }
            else // Changing reference spheres position (can't define Array without loosing reference to structs)
            {
                for (int i = 0; i < ContainerSphereIds.Length; i++)
                {
                    if (Shapes[i].transform == null)
                        OwnerContainer.CullingSpheres[ContainerSphereIds[i]].position = transform.TransformPoint(Shapes[i].position);
                    else
                        OwnerContainer.CullingSpheres[ContainerSphereIds[i]].position = Shapes[i].transform.TransformPoint(Shapes[i].position);
                }
            }
        }


        #endregion


        /// <summary>
        /// Getting visibility sphere for target index
        /// </summary>
        private BoundingSphere GetVisibilitySphere(int i)
        {
            if (OwnerContainer == null)
                return visibilitySpheres[i];
            else
                if (!UseMultiShape)
                return OwnerContainer.CullingSpheres[ContainerSphereId];
            else
                return OwnerContainer.CullingSpheres[ContainerSphereIds[i]];
        }


        [System.Serializable]
        public class MultiShapeBound
        {
            public Vector3 position;
            public float radius = 1f;
            public Transform transform;
        }


        /// <summary>
        /// Getting index of saved id in ids array for multi shape
        /// </summary>
        internal int GetIndexForCullEventMultiShape(int containerSphere)
        {
            for (int i = 0; i < ContainerSphereIds.Length; i++)
            {
                if (ContainerSphereIds[i] == containerSphere)
                {
                    //string ids = "";
                    //for (int x = 0; x < ContainerSphereIds.Length; x++) ids += "," + ContainerSphereIds[x];
                    //Debug.LogWarning("! Cont: " + containerSphere + " i = " + i + " In: " + ids);

                    return i;
                }
            }


            Debug.LogWarning("[Optimizers Multi Shape Container] Wrong container sphere id! " + containerSphere );
            return -1;
        }
    }
}
