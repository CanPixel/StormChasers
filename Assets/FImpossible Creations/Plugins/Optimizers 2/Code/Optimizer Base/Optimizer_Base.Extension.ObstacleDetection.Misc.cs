using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace FIMSpace.FOptimizing
{
    public abstract partial class Optimizer_Base
    {
        
        /// <summary>
        /// Updating list of coverage detection points
        /// </summary>
        public void RefreshCoverageDetectionPoints(List<Vector3> coverageOffsets, Vector3 origin)
        {
            if (coverageActiveArray == null) coverageActiveArray = new Vector3[0];

            if (coverageActiveArray.Length != CoverageOffsets.Count)
                coverageActiveArray = new Vector3[CoverageOffsets.Count];

            float scale = (CoverageScale) * 0.7f;

            if (OptimizingMethod == EOptimizingMethod.Effective)
            {
                if (CustomCoveragePoints)
                {
                    Quaternion flatDirection = Quaternion.LookRotation(Camera.main.transform.position - origin);
                    //Vector3 offset = flatDirection * CoverageOffset;

                    for (int i = 0; i < coverageOffsets.Count; i++)
                    {
                        coverageActiveArray[i] = origin;
                        coverageActiveArray[i] += (flatDirection) * Vector3.Scale(coverageOffsets[i] * scale, Vector3.one * DetectionRadius);// + offset;
                    }
                }
                else
                {
                    Quaternion flatDirection = Quaternion.LookRotation(Camera.main.transform.position - origin);
                    //Vector3 offset = flatDirection * CoverageOffset;

                    for (int i = 0; i < coverageOffsets.Count; i++)
                    {
                        coverageActiveArray[i] = origin;
                        coverageActiveArray[i] += (flatDirection) * coverageOffsets[i].normalized * DetectionRadius * scale;// + offset;
                    }
                }
            }
            else
            {
                //Quaternion flatDirection = Camera.main.transform.rotation;  // flatDirection * -coverag...
                Quaternion flatDirection = Quaternion.LookRotation(Camera.main.transform.position - origin);
                //Vector3 offset = flatDirection * CoverageOffset;

                for (int i = 0; i < coverageOffsets.Count; i++)
                {
                    coverageActiveArray[i] = origin;
                    coverageActiveArray[i] += (flatDirection) * Vector3.Scale(coverageOffsets[i] * scale, DetectionBounds / 2);// + offset;
                }
            }
        }


        /// <summary>
        /// Refreshing target points for coverage check feature
        /// It is creating array of percentage-like values to identify raycast points
        /// </summary>
        private void RefreshCoverageOffsets()
        {
            if (CustomCoveragePoints) return;
            if (currentCoveragePrecision == CoveragePrecision) return;
            if (CoveragePrecision == -1) return;
            currentCoveragePrecision = CoveragePrecision;

            CoverageOffsets = new List<Vector3>();
            Vector3[] coverageOffsets = new Vector3[0];

            #region Method points baked

            if (OptimizingMethod == EOptimizingMethod.Effective)
            {   // Sphere based coverage offsets
                if (CoveragePrecision == 0)
                {
                    coverageOffsets = new Vector3[1];
                    coverageOffsets[0] = new Vector3(0f, 0f, 1f);
                }
                else
                if (CoveragePrecision == 4)
                {
                    coverageOffsets = new Vector3[13];
                    coverageOffsets[0] = new Vector3(0f, 0f, 1f);
                    coverageOffsets[1] = new Vector3(-1f, 0f, 0f);
                    coverageOffsets[2] = new Vector3(1f, 0f, 0f);
                    coverageOffsets[3] = new Vector3(0f, 1f, 0f);
                    coverageOffsets[4] = new Vector3(0f, -1f, 0f);

                    coverageOffsets[5] = new Vector3(-.5f, .5f, .85f);
                    coverageOffsets[6] = new Vector3(.5f, .5f, .85f);
                    coverageOffsets[7] = new Vector3(.5f, -.5f, .85f);
                    coverageOffsets[8] = new Vector3(-.5f, -.5f, .85f);

                    coverageOffsets[9] = new Vector3(.5f, .5f, 0f);
                    coverageOffsets[11] = new Vector3(-.5f, .5f, 0f);
                    coverageOffsets[10] = new Vector3(-.5f, -.5f, 0f);
                    coverageOffsets[12] = new Vector3(.5f, -.5f, 0f);
                }
                else
                if (CoveragePrecision == 5)
                {
                    //coverageOffsets = new Vector3[13];
                    coverageOffsets = new Vector3[25];
                    coverageOffsets[0] = new Vector3(0f, 0f, 1f);
                    coverageOffsets[1] = new Vector3(-1f, 0f, 0f);
                    coverageOffsets[2] = new Vector3(1f, 0f, 0f);
                    coverageOffsets[3] = new Vector3(0f, 1f, 0f);
                    coverageOffsets[4] = new Vector3(0f, -1f, 0f);

                    coverageOffsets[5] = new Vector3(-.5f, .5f, .85f);
                    coverageOffsets[6] = new Vector3(.5f, .5f, .85f);
                    coverageOffsets[7] = new Vector3(.5f, -.5f, .85f);
                    coverageOffsets[8] = new Vector3(-.5f, -.5f, .85f);

                    coverageOffsets[9] = new Vector3(.5f, .5f, 0f);
                    coverageOffsets[11] = new Vector3(-.5f, .5f, 0f);
                    coverageOffsets[10] = new Vector3(-.5f, -.5f, 0f);
                    coverageOffsets[12] = new Vector3(.5f, -.5f, 0f);

                    for (int i = 13; i < coverageOffsets.Length; i++)
                        coverageOffsets[i] = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
                }
                else if (CoveragePrecision == 3)
                {
                    coverageOffsets = new Vector3[9];

                    coverageOffsets[0] = new Vector3(0f, 0f, 1f);
                    coverageOffsets[1] = new Vector3(-1f, 0f, 0f);
                    coverageOffsets[2] = new Vector3(1f, 0f, 0f);
                    coverageOffsets[3] = new Vector3(0f, 1f, 0f);
                    coverageOffsets[4] = new Vector3(0f, -1f, 0f);

                    coverageOffsets[5] = new Vector3(-.7f, .7f, .85f);
                    coverageOffsets[6] = new Vector3(.7f, .7f, .85f);
                    coverageOffsets[7] = new Vector3(.7f, -.7f, .85f);
                    coverageOffsets[8] = new Vector3(-.7f, -.7f, .85f);
                }
                else if (CoveragePrecision == 2)
                {
                    coverageOffsets = new Vector3[5];
                    coverageOffsets[0] = new Vector3(0f, 0f, 1f);
                    coverageOffsets[1] = new Vector3(-1f, 1f, .4f);
                    coverageOffsets[2] = new Vector3(1f, -1f, .4f);
                    coverageOffsets[3] = new Vector3(1f, 1f, .4f);
                    coverageOffsets[4] = new Vector3(-1f, -1f, .4f);
                }
                else if (CoveragePrecision == 1)
                {
                    coverageOffsets = new Vector3[4];
                    coverageOffsets[0] = new Vector3(0f, 0f, 1f);
                    coverageOffsets[1] = new Vector3(0f, 0.4f, .1f);
                    coverageOffsets[2] = new Vector3(-.6f, -0.3f, 0.15f);
                    coverageOffsets[3] = new Vector3(.6f, -0.3f, 0.15f);
                }
            }
            else // Bounds based coverage offsets
            {
                if (CoveragePrecision == 0)
                {
                    coverageOffsets = new Vector3[1];
                    coverageOffsets[0] = new Vector3(0f, 0f, 1f);
                }
                else
                if (CoveragePrecision == 4)
                {
                    coverageOffsets = new Vector3[13];
                    coverageOffsets[0] = new Vector3(0f, 0f, 1f);
                    coverageOffsets[1] = new Vector3(-1f, 1f, .4f);
                    coverageOffsets[2] = new Vector3(1f, -1f, .4f);
                    coverageOffsets[3] = new Vector3(1f, 1f, .4f);
                    coverageOffsets[4] = new Vector3(-1f, -1f, .4f);

                    coverageOffsets[5] = new Vector3(-.7f, .4f, .85f);
                    coverageOffsets[6] = new Vector3(.7f, .4f, .85f);
                    coverageOffsets[7] = new Vector3(.7f, -.4f, .85f);
                    coverageOffsets[8] = new Vector3(-.7f, -.4f, .85f);

                    coverageOffsets[9] = new Vector3(-1f, 0f, .0f);
                    coverageOffsets[10] = new Vector3(1f, 0f, .0f);

                    coverageOffsets[11] = new Vector3(0f, 1f, .0f);
                    coverageOffsets[12] = new Vector3(0f, -1f, .0f);
                }
                else
                if (CoveragePrecision == 5)
                {
                    coverageOffsets = new Vector3[25];
                    coverageOffsets[0] = new Vector3(0f, 0f, 1f);
                    coverageOffsets[1] = new Vector3(-1f, 1f, .4f);
                    coverageOffsets[2] = new Vector3(1f, -1f, .4f);
                    coverageOffsets[3] = new Vector3(1f, 1f, .4f);
                    coverageOffsets[4] = new Vector3(-1f, -1f, .4f);

                    coverageOffsets[5] = new Vector3(-.7f, .4f, .85f);
                    coverageOffsets[6] = new Vector3(.7f, .4f, .85f);
                    coverageOffsets[7] = new Vector3(.7f, -.4f, .85f);
                    coverageOffsets[8] = new Vector3(-.7f, -.4f, .85f);

                    coverageOffsets[9] = new Vector3(-1f, 0f, .0f);
                    coverageOffsets[10] = new Vector3(1f, 0f, .0f);

                    coverageOffsets[11] = new Vector3(0f, 1f, .0f);
                    coverageOffsets[12] = new Vector3(0f, -1f, .0f);

                    for (int i = 13; i < coverageOffsets.Length; i++)
                        coverageOffsets[i] = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(0, 1f));
                }
                else if (CoveragePrecision == 3)
                {
                    coverageOffsets = new Vector3[9];

                    coverageOffsets[0] = new Vector3(0f, 0f, 1f);
                    coverageOffsets[1] = new Vector3(-1f, 1f, .4f);
                    coverageOffsets[2] = new Vector3(1f, -1f, .4f);
                    coverageOffsets[3] = new Vector3(1f, 1f, .4f);
                    coverageOffsets[4] = new Vector3(-1f, -1f, .4f);

                    coverageOffsets[5] = new Vector3(-.7f, .4f, .85f);
                    coverageOffsets[6] = new Vector3(.7f, .4f, .85f);
                    coverageOffsets[7] = new Vector3(.7f, -.4f, .85f);
                    coverageOffsets[8] = new Vector3(-.7f, -.4f, .85f);
                }
                else if (CoveragePrecision == 2)
                {
                    coverageOffsets = new Vector3[5];
                    coverageOffsets[0] = new Vector3(0f, 0f, 1f);
                    coverageOffsets[1] = new Vector3(-1f, 1f, .4f);
                    coverageOffsets[2] = new Vector3(1f, -1f, .4f);
                    coverageOffsets[3] = new Vector3(1f, 1f, .4f);
                    coverageOffsets[4] = new Vector3(-1f, -1f, .4f);
                }
                else if (CoveragePrecision == 1)
                {
                    coverageOffsets = new Vector3[4];
                    coverageOffsets[0] = new Vector3(0f, 0f, 1f);
                    coverageOffsets[1] = new Vector3(0f, 0.8f, .1f);
                    coverageOffsets[2] = new Vector3(-1f, -0.85f, 0.15f);
                    coverageOffsets[3] = new Vector3(1f, -0.85f, 0.15f);
                }
            }

            #endregion

            CoverageOffsets.Clear();
            for (int i = 0; i < coverageOffsets.Length; i++)
                CoverageOffsets.Add(coverageOffsets[i]);
        }



        #region Gizmos

#if UNITY_EDITOR
        protected void Gizmos_DrawObstacleDetection()
        {
            if (CoveragePrecision == -1) return;
            bool camG = false;
            if (OptimizersManager.Exists) if (!OptimizersManager.MainCamera) camG = true;
            if (!camG) if (Camera.main == null) return;

            RefreshCoverageOffsets();
            RefreshCoverageDetectionPoints(CoverageOffsets, GetReferencePosition());
            
            float scale = DetectionRadius / 15f;

            if (OptimizingMethod != EOptimizingMethod.Effective) scale = DetectionBounds.x / 14f;

            Gizmos.color = new Color(0.22f, 0.88f, 0.22f, 0.2f * GizmosAlpha);
            for (int i = 0; i < CoverageOffsets.Count; i++)
            {
                Vector3 origin = coverageActiveArray[i]; // new Vector3(CoverageArea.x * coverageOffsets[i].x / 2f, CoverageArea.y * coverageOffsets[i].y / 2f, CoverageAreaOffset.z) + new Vector3(CoverageAreaOffset.x, CoverageAreaOffset.y, 0f);

                Gizmos.DrawLine(origin, Camera.main.transform.position);
                Gizmos.DrawRay(origin, -Vector3.forward * scale * 0.75f);
                Gizmos.DrawWireSphere(origin, scale);
                UnityEditor.Handles.Label(origin, "[" + i + "]", EditorStyles.whiteLabel);
            }

        }
#endif

        #endregion

    }
}
