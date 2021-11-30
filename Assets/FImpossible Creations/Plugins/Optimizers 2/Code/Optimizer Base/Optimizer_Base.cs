using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace FIMSpace.FOptimizing
{
    /// <summary>
    /// FM: Base class with methods to support culling groups and other methods useful for optimizer classes.
    /// > Defining how many LODs should be used and on what distances
    /// > Handling detecting distance ranges and object visibility
    /// > Supporting different algorithms for detecting object visibility and distance
    /// > Handling adding new components to be optimized
    /// </summary>
    public abstract partial class Optimizer_Base : MonoBehaviour
    {
        // --- THIS IS PARTIAL CLASS - REFT OF THE CODE IS IN SEPARATED FILES --- \\


        /// <summary>
        /// Checking correctness and initializing optimizer component
        /// </summary>
        protected virtual void Start()
        {
            StartVariablesRefresh();
            RefreshInitialSettingsForOptimized();

            // Triggering correct initialization methods
            switch (OptimizingMethod)
            {
                case EOptimizingMethod.Static:
                    InitStaticOptimizer();
                    break;

                case EOptimizingMethod.Dynamic:
                    InitDynamicOptimizer(true);
                    break;

                case EOptimizingMethod.Effective:
                    InitEffectiveOptimizer();
                    break;

                case EOptimizingMethod.TriggerBased:
                    InitTriggerOptimizer();
                    break;
            }

            moveTreshold = (DetectionRadius * transform.lossyScale.x) / 100f;
            if (OptimizersManager.Instance) moveTreshold *= (1f - OptimizersManager.Instance.UpdateBoost * 0.999f);
            //initialized = true;
        }


        /// <summary> Checking if derived type to optimize list is valid </summary>
        public abstract bool OptimizationListExists();


        /// <summary>
        /// Executed every when component added and every change inside inspector window
        /// </summary>
        public virtual void OnValidate()
        {
#if UNITY_EDITOR

            if (Selection.gameObjects.Contains(gameObject))
            {
                if (FadeDuration <= 0f) FadeViewVisibility = false;

                if (!Application.isPlaying)
                {
                    EditorUpdate();

                    if (UseObstacleDetection)
                        if (!wasSearching)
                        {
                            ignoredObstacleColliders = FTransformMethods.FindComponentsInAllChildren<Collider>(transform, true).ToArray();
                            wasSearching = true;
                        }
                }

                OptimizerOnValidate();

                if (UseMultiShape) OnValidateMultiShape();

            }
#endif
        }



        /// <summary>
        /// Method called when component is added to any game object
        /// </summary>
        protected virtual void Reset()
        {
#if UNITY_EDITOR
            OptimizerReset();
#endif
        }


    }
}
