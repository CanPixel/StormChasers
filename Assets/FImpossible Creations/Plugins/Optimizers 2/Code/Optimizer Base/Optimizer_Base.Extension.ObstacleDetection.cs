using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.FOptimizing
{
    public abstract partial class Optimizer_Base
    {
        public bool UseObstacleDetection = false;

        [HideInInspector]
        [Range(0, 5)]
        [Tooltip("Allowing component to do more raycasts to detect obstacles covering it")]
        public int CoveragePrecision = 1;
        [HideInInspector]
        [Range(0f, 1.5f)]
        [Tooltip("If you want to avoid casting some raycasts from below ground")]
        public float CoverageScale = 1;

        [HideInInspector]
        [Tooltip("Layer mask for raycasts checking obstacles in front of object in direction to camera")]
        public LayerMask CoverageMask = 1 << 0;

        [HideInInspector]
        [Tooltip("Draw menu for customized raycasting points")]
        public bool CustomCoveragePoints = false;

        #region Backup
        //[HideInInspector]
        //[Tooltip("Adjusting raycasts placement")]
        //public Vector3 CoverageOffset = Vector3.zero;

        //[HideInInspector]
        //[Range(0f, 1f)]
        //[Tooltip("When using raycast memory (Optimizers Manager), we can define how unprecise can be checking for sake of better performance - lower value -> higher precision and cpu usage")]
        //public float MemoryTolerance = 0f;
        #endregion

        [HideInInspector]
        public List<Vector3> CoverageOffsets;
        private int currentCoveragePrecision = -1;
        //[SerializeField] [HideInInspector]
        public Collider[] ignoredObstacleColliders;
        private Vector3[] coverageActiveArray;

        /// <summary>
        /// Using raycast for obstacle detection
        /// </summary>
        private void ObstacleCheck()
        {
            RefreshCoverageDetectionPoints(CoverageOffsets, PreviousPosition);

            for (int i = 0; i < coverageActiveArray.Length; i++)
            {
                RaycastHit hit;
                Physics.Linecast(TargetCamera.position, coverageActiveArray[i], out hit, CoverageMask, QueryTriggerInteraction.Ignore);

#if UNITY_EDITOR
                OptimizersManager.RaycastsInThisFrame++;
#endif
                bool hitted = false;

                if (hit.transform)
                {
                    hitted = true;
                    if (ignoredObstacleColliders != null)
                        if (ignoredObstacleColliders.Length > 0)
                            foreach (Collider c in ignoredObstacleColliders)
                                if ( c == hit.collider)
                                {
                                    hitted = false;
                                    break;
                                }
                }

                if (!hitted)
                {
                    SetHidden(false);
                    return;
                }
            }

            SetHidden(true);
        }



        public void ObstacleDetectionOnValidate()
        {
            CullIfNotSee = true;
            if (OptimizingMethod == EOptimizingMethod.Static)
            {
                Debug.LogError("[OPTIMIZERS] " + OptimizingMethod + " method is not supported for FOptimizer_ObstacleDetection component!");
                OptimizingMethod = EOptimizingMethod.Effective;
            }
        }


    }
}
