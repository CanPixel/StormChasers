using UnityEngine;

namespace FIMSpace.FOptimizing
{
    public abstract partial class Optimizer_Base
    {
        private void InitEffectiveOptimizer()
        {
            if (!AddToContainer) OptimizersManager.Instance.RegisterNotContainedEffectiveOptimizer(this, true);

            InitCullingGroups(GetDistanceMeasures(), DetectionRadius, OptimizersManager.MainCamera);
            InitDynamicOptimizer(false);
        }

        private void EffectiveLODUpdate()
        {
            float dist = (PreviousPosition - mainVisibilitySphere.position).magnitude;
            if (dist > moveTreshold) RefreshEffectiveCullingGroups();
        }

        protected virtual void RefreshEffectiveCullingGroups()
        {
            if (!UseMultiShape)
            {
                if (OwnerContainer != null)
                    OwnerContainer.CullingSpheres[ContainerSphereId].position = GetReferencePosition();
                else
                    mainVisibilitySphere.position = GetReferencePosition();
            }
            else
            {
                RefreshEffectiveCullingGroupsMultiShape();
            }
        }
    }
}
