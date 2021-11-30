using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.FOptimizing
{
    /// <summary>
    /// Experimental feature works
    /// </summary>
    public partial class OptimizersManager
    {
        //private List<FOptimizers_ObstacleBounds> detectedBounds;

        //public void DetectedObstacle(RaycastHit hit, Vector3 castDirection)
        //{
        //    FOptimizers_ObstacleBounds b = new FOptimizers_ObstacleBounds(hit, castDirection);
        //    //detectedBounds.Add(hit.collider.bounds);
        //    detectedBounds.Add(b);
        //}

        ///// <summary>
        ///// Checking if optimizer's cast is similar to remembered ones
        ///// </summary>
        ///// <returns> true - if object is visible | false - if invisible | null if there is no similar cast done before </returns>
        //// /// <returns> true - if object should be invisible | false if there is no similar cast done before </returns>
        //public bool CastMemoryCheck(FOptimizer_ObstacleDetection optimizer, Vector3 origin, Vector3 direction)
        //{
        //    if (detectedBounds.Count > 0)
        //    {
        //        for (int i = 0; i < detectedBounds.Count; i++)
        //        {
        //            float angle = Vector3.Angle(detectedBounds[i].CastDirection, direction);
        //            if (angle < 5 + 7 * optimizer.MemoryTolerance)
        //            {
        //                return true;
        //            }
        //        }
        //    }

        //    return false;
        //}
    }
}
