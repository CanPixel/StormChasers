using System;
using UnityEngine;

namespace FIMSpace.FOptimizing
{
    public abstract partial class Optimizer_Base
    {
        //[Tooltip("If dynamic optimization update routine should be done in different thread than Unity Engine thread (no Courutines, true second thread)")]
        //public bool DynamicAsync = true;
        public EOptimizingDistance? CurrentDynamicDistanceCategory { get; protected set; }
        public int DynamicListIndex { get; protected set; }
        public Vector3 PreviousPosition { get; protected set; }
        public Vector3 LastDynamicCheckCameraPosition { get; protected set; }
        public Vector3 LastTresholdCheckPos { get; protected set; }
        public Vector3 LastTresholdCheckCamPos { get; protected set; }
        public Quaternion LastTresholdCheckCamRot { get; protected set; }

        protected OptimizersManager manager;
        private Bounds optimizerBounds;

        //private FEOptimizingDistance lastDynamicCategory;
        private float lastDynamicDistance;


        /// <summary>
        /// Assigning Optimizers_Manager to optimizer and setting to be updated with manager's clock
        /// </summary>
        private void InitDynamicOptimizer(bool justDynamic)
        {
            PreviousPosition = GetReferencePosition();

            if (manager == null)
            {
                manager = OptimizersManager.Instance;
                if (OptimizersManager.MainCamera) TargetCamera = OptimizersManager.MainCamera.transform;
            }

            if (justDynamic) OptimizersManager.Instance.RegisterNotContainedDynamicOptimizer(this, true);

            if (TargetCamera)
            {
                LastTresholdCheckPos = transform.position + Vector3.forward * 100;
                LastTresholdCheckCamPos = TargetCamera.position + Vector3.forward * 100;
                LastTresholdCheckCamRot = TargetCamera.rotation * Quaternion.Euler(180, 0, 0);
            }

            DynamicListIndex = manager.AddToDynamic(this);

            if (OptimizingMethod != EOptimizingMethod.Effective)
            {   // Because Effective is using culling groups and updating their position with manager's clock
                optimizerBounds = new Bounds(GetReferencePosition(), Vector3.Scale( DetectionBounds, transform.lossyScale));
            }
        }


        protected void RefreshDistances()
        {
            float[] distances = GetDistanceMeasures();
            DistanceLevels = new float[distances.Length];

            for (int i = 0; i < distances.Length; i++)
                DistanceLevels[i] = distances[i];
        }

        /// <summary>
        /// Getting LOD transition instance for IFLOD
        /// </summary>
        internal abstract Optimizers_LODTransition GetLodTransitionFor(int optimizedIndex, int targetLOD);


        /// <summary>
        /// Accessing optimized type LOD settings
        /// </summary>
        internal abstract ILODInstance GetLODInstance(int i, int targetLODLevel);


        /// <summary>
        /// Accessing optimized component
        /// </summary>
        public abstract Component GetOptimizedComponent(int i);


        /// <summary>
        /// Removing pointed LODs controller from ToOptimize list
        /// </summary>
        internal abstract void RemoveToOptimize(LODsControllerBase lODsControllerBase);


        /// <summary>
        /// Safely removing object from optimizer
        /// </summary>
        private void DisposeDynamicOptimizer()
        {
            if (!isQuitting)
                if (manager)
                {
                    manager.RemoveFromDynamic(this);

#if UNITY_EDITOR
                    if (IsHidden) OptimizersManager.HiddenObjects--;
#endif
                }
        }


        /// <summary>
        /// Updating optimizer in undefined time interval - depending on distance from target camera and application performance
        /// Called only if InitDynamicOptimizer() was executed (by default only 'Static' method doesn't do this)
        /// </summary>
        public virtual void DynamicLODUpdate(EOptimizingDistance category, float distance)
        {
            if (UseMultiShape) DynamicLODUpdateMiltiShape(category, distance);

            //lastDynamicCategory = category;
            lastDynamicDistance = distance;
            CurrentDynamicDistanceCategory = category;

            if (enabled == false)
            {
                wasDisabled = true;
                return;
            }

            Vector3 pos = GetReferencePosition();
            int lod = GetLODForDistance(distance);

            if (OptimizingMethod == EOptimizingMethod.Dynamic)
            {
                #region Dynamic LOD Update

                if (distance > DistanceLevels[DistanceLevels.Length - 1])
                {
                    OutOfDistance = true;
                    FarAway = true;
                }
                else
                {
                    OutOfDistance = false;
                    FarAway = false;
                }

                // Camera look away quick cull feature support
                if (CullIfNotSee) // If we want to cull object when camera look away
                {
                    optimizerBounds.center = pos;
                    //optimizerBounds.size = optimizerBounds.size;
                    OutOfCameraView = !GeometryUtility.TestPlanesAABB(manager.CurrentFrustumPlanes, optimizerBounds);
                }
                else // When we only want to use distance to cull object
                    OutOfCameraView = false;

                bool changeOccured = false;

                if (lod != CurrentDistanceLODLevel)
                    changeOccured = true;
                else
                if (WasOutOfCameraView != OutOfCameraView)
                    changeOccured = true;
                else
                    if (WasHidden != IsHidden)
                    changeOccured = true;

                if (changeOccured) RefreshVisibilityState(lod);

                #endregion
            }
            else // When Effective or trigger method is called
            {
                if (OptimizingMethod == EOptimizingMethod.Effective)
                {
                    EffectiveLODUpdate();
                }
                else // When trigger method is called
                {
                    TriggerLODUpdate();
                }
            }


            PreviousPosition = pos;
            LastDynamicCheckCameraPosition = TargetCamera.position;
            distancePoint = PreviousPosition;


            // Obstacle detection implementation
            if (UseObstacleDetection)
            {
                if (CoveragePrecision == -1) return;
                if (!OutOfCameraView && !OutOfDistance) ObstacleCheck();
            }
        }


        /// <summary>
        /// Getting target LOD for choosed distance
        /// </summary>
        private int GetLODForDistance(float distance)
        {
            if (DistanceLevels == null)
            {
                Debug.LogWarning("[OPTIMIZERS] There was something wrong with distance ranges of this object (" + name + ")");
                RefreshDistances();
            }

            for (int i = 0; i < DistanceLevels.Length; i++)
                if (distance < DistanceLevels[i]) return i;

            return LODLevels;
        }


        /// <summary>
        /// Movement treshold check by optimizer manager for system optimization
        /// </summary>
        internal bool TresholdTrigger()
        {
            bool moved = manager.CameraMoved(LastTresholdCheckCamPos, LastTresholdCheckCamRot);

            LastTresholdCheckCamPos = TargetCamera.position;
            LastTresholdCheckCamRot = TargetCamera.rotation;

            if (moved)
            {
                LastTresholdCheckPos = transform.position;
                return true; // Camera movement/rotation treshold exceed so we update object
            }
            else
            {
                float diff = (LastTresholdCheckPos - transform.position).magnitude;
                LastTresholdCheckPos = transform.position;

                if (diff >= manager.MoveTreshold)
                    return true; // Movement treshold exceed so we update object
            }

            LastTresholdCheckPos = transform.position;
            return false; // Nothing changed in movement tresholds
        }


    }
}
