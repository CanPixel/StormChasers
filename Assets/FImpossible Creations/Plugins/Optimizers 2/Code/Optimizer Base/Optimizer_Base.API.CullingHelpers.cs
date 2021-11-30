using UnityEngine;

namespace FIMSpace.FOptimizing
{
    /// <summary>
    /// FC: Methods to help culling identify reference positions and objects
    /// </summary>
    public abstract partial class Optimizer_Base
    {
        /// <summary>
        /// IF not adding to culling containers - refreshing camera
        /// </summary>
        public void RefreshCamera(Camera camera)
        {
            if (camera == null) return;

            TargetCamera = camera.transform;

            if (OwnerContainer == null)
                if (CullingGroup != null)
                {
                    CullingGroup.targetCamera = camera;
                    CullingGroup.SetDistanceReferencePoint(TargetCamera);
                }
        }


        /// <summary>
        /// Getting list of target distance ranges for LOD levels
        /// </summary>
        public virtual float[] GetDistanceMeasures()
        {
            //EditorResetLODValues();
            float[] lods = new float[LODPercent.Count];
            for (int i = 0; i < LODPercent.Count; i++) lods[i] = MaxDistance * LODPercent[i];
            return lods;
        }


        /// <summary>
        /// Getting reference world position of this optimizer object
        /// </summary>
        public virtual Vector3 GetReferencePosition()
        {
            //if (!initialized) return Vector3.zero;

            if ( UseMultiShape)
            {
                return GetReferencePositionMultiShape();
            }

#if UNITY_EDITOR

            if (!Application.isPlaying) return transform.position + transform.TransformVector(DetectionOffset);
#endif
            if (OptimizingMethod == EOptimizingMethod.Static) if (visibilitySpheres != null) return visibilitySpheres[0].position;

            return transform.position + transform.TransformVector(DetectionOffset);
        }


        /// <summary>
        /// Reference distance for GUI inspector view data
        /// </summary>
        public virtual float GetReferenceDistance()
        {
            //if (!initialized) return 0f;

            if (OptimizingMethod == EOptimizingMethod.Static || OptimizingMethod == EOptimizingMethod.Effective)
            {
                float distance = Vector3.Distance(GetReferencePosition(), TargetCamera.position);
                if (distance < mainVisibilitySphere.radius) distance = 0f; else distance -= mainVisibilitySphere.radius;
                return distance;
            }

            return Vector3.Distance(PreviousPosition, LastDynamicCheckCameraPosition);
        }


        /// <summary>
        /// Getting additional radius from detection sphere radius or other for distance detection
        /// </summary>
        public float GetAddRadius()
        {
            if (OptimizingMethod == EOptimizingMethod.Static || OptimizingMethod == EOptimizingMethod.Effective)
                return DetectionRadius * transform.lossyScale.x;
            else
                return 0f;
        }

    }
}
