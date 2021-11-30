using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.FOptimizing
{
    public abstract partial class Optimizer_Base
    {
        private Transform triggersContainer;
        [HideInInspector]
        [Tooltip("Layer for triggers container to detect intersections only with Camera layer\n(camera and containers can have the same layer but change collision matrix)")]
        public LayerMask OnlyCamCollLayer;

        protected int triggerDistanceState = -1;
        protected int preTriggerDistanceState = -1;
        protected List<int> triggersEntered;


        private void InitTriggerOptimizer()
        {
            if (triggersEntered == null) triggersEntered = new List<int>();
            Transform cam = OptimizersManager.MainCamera != null ? OptimizersManager.MainCamera.transform : null;
            if (cam) OnlyCamCollLayer = cam.gameObject.layer;

            TargetCamera = cam;

            float[] distances = GetDistanceMeasures();
            DistanceLevels = new float[distances.Length];

            for (int i = 0; i < distances.Length; i++)
                DistanceLevels[i] = distances[i];

            OptimizersManager.Instance.RegisterNotContainedTriggerOptimizer(this, true);
            if ( CullIfNotSee) InitDynamicOptimizer(false);

            TriggerLODUpdate();
            GenerateTriggerHelpers();
            OutOfDistance = true;
            RefreshVisibilityState(CurrentDistanceLODLevel);
        }


        private void TriggerLODUpdate()
        {
            // Camera look away quick cull feature support
            if (CullIfNotSee) // If we want to cull object when camera look away
            {
                optimizerBounds.center = GetReferencePosition();
                //optimizerBounds.size = DetectionBounds;
                OutOfCameraView = !GeometryUtility.TestPlanesAABB(manager.CurrentFrustumPlanes, optimizerBounds);

                if (WasOutOfCameraView != OutOfCameraView) // If change occured
                    RefreshVisibilityState(CurrentDistanceLODLevel);
            }
            else // When we only want to use distance to cull object
                OutOfCameraView = false;
        }


        internal virtual void OnTriggerChange(Optimizers_TriggerHelper helper, bool exit)
        {
            int distInd;
            if (!exit)
            {
                if ( !triggersEntered.Contains(helper.TriggerIndex)) triggersEntered.Add(helper.TriggerIndex);
                distInd = helper.TriggerIndex;
            }
            else
            {
                triggersEntered.Remove(helper.TriggerIndex);
                if (triggersEntered.Count == 0) distInd = LODLevels;
                else
                    distInd = triggersEntered[triggersEntered.Count - 1];
            }

            if (distInd >= LODLevels + 1) distInd = LODLevels;
            triggerDistanceState = distInd;

            bool changeOccured = false;
            if (preTriggerDistanceState != distInd) changeOccured = true;

            if (triggersEntered.Count == 0) OutOfDistance = true; else OutOfDistance = false;

            if (changeOccured)
            {
                RefreshVisibilityState(distInd);
                preTriggerDistanceState = distInd;
            }
        }


        protected void GenerateTriggerHelpers()
        {
            if (triggersContainer == null)
            {
                GameObject triggers = new GameObject("Optimizers-" + name + "-Triggers");
                triggersContainer = triggers.transform;
                triggersContainer.SetParent(transform);
                triggersContainer.localPosition = DetectionOffset;
                triggersContainer.localRotation = Quaternion.identity;
                triggersContainer.localScale = Vector3.one;
                triggersContainer.gameObject.layer = OnlyCamCollLayer;

                for (int i = 0; i < DistanceLevels.Length; i++)
                {
                    GameObject go = new GameObject(i.ToString());
                    Transform t = go.transform;
                    t.SetParent(triggersContainer, false);
                    t.localPosition = Vector3.zero;
                    t.localRotation = Quaternion.identity;
                    t.localScale = Vector3.one;

                    SphereCollider sph = go.AddComponent<SphereCollider>();
                    sph.isTrigger = true;
                    float div = transform.lossyScale.x; if (div == 0) div = 1f;
                    sph.radius = DistanceLevels[i] / div;
                    go.AddComponent<Optimizers_TriggerHelper>().Initialize(this, i);
                }
            }
        }

    }
}
