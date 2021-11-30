using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.FOptimizing
{
    public partial class OptimizersManager
    {
        /// <summary> Transitions between LOD levels </summary>
        private readonly List<Optimizers_Transitioning> transitioning = new List<Optimizers_Transitioning>();

        /// <summary>
        /// Starting transition to next LOD level for optimizer object reference
        /// </summary>
        public void TransitionTo(Optimizer_Base optimizer, int targetLODLevel, float duration = 0f)
        {
            int optimizerId = optimizer.GetInstanceID(); // Id code for quicker search in list
            Optimizers_Transitioning helper = null; // If we are already transitioning provided LODs controller we will find it  

            for (int i = 0; i < transitioning.Count; i++)
            {
                if (transitioning[i].Id == optimizerId)
                {
                    helper = transitioning[i]; // LODs controller is already transitioning
                    break;
                }
            }

            if (helper != null) // Let's break current transition and assign new target LOD
            {
                helper.BreakCurrentTransition(duration, targetLODLevel);
            }
            else // Creating new transitioning task and adding to list
            {
                helper = new Optimizers_Transitioning(optimizerId, optimizer, targetLODLevel, duration, transitioning.Count);
                transitioning.Add(helper);
            }
        }


        public void EndTransition(Optimizer_Base optimizer)
        {
            int optimizerId = optimizer.GetInstanceID(); // Hash code for quicker search in list

            for (int i = 0; i < transitioning.Count; i++)
                if (transitioning[i].Id == optimizerId)
                {
                    transitioning[i].Finish();
                    transitioning.RemoveAt(i);
                    break;
                }
        }


        /// <summary>
        /// Updating transitioning in MonoBehaviour's life cycle
        /// </summary>
        private void TransitionsUpdate()
        {
            float delta = Time.unscaledDeltaTime;

            for (int i = transitioning.Count - 1; i >= 0; i--)
            {
                transitioning[i].Update(delta);
                if (transitioning[i].Finished) transitioning.RemoveAt(i);
            }
        }
    }
}
