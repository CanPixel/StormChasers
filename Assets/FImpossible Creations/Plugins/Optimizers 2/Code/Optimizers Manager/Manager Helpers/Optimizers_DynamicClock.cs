using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.FOptimizing
{
    /// <summary>
    /// FM: Helper for managing execution of distance check for game.
    /// During game lifetime it is adapting it's update rates to not disturb game performance.
    /// </summary>
    public class Optimizers_DynamicClock
    {
        public OptimizersManager Manager { get; private set; }
        public List<Optimizer_Base> Optimizers { get; private set; }
        public EOptimizingDistance OptimizingDistanceType { get; private set; }
        public long FrameTicksConsumption { get; private set; }
        public long LastMSConsumption { get; private set; }
        public long LastTicksConsumption { get; private set; }
        public int LastTickFrame { get; private set; }
        public int DelaysCount { get; private set; }
        public float AdaptedDelay { get; private set; }

        private int[] avgTicks;
        private int avgCounter = 0;

        private System.Diagnostics.Stopwatch watch;
        private WaitForEndOfFrame waitForLateUpdate;

        private readonly float delayTolerance;
        private readonly float updateRatio;
        private readonly float maxDelay;

        public Optimizers_DynamicClock(OptimizersManager manager, EOptimizingDistance type, List<Optimizer_Base> optimizers)
        {
            Manager = manager;
            OptimizingDistanceType = type;
            Optimizers = optimizers;

            watch = new System.Diagnostics.Stopwatch();
            AdaptedDelay = 0.01f;
            LastMSConsumption = 0;
            FrameTicksConsumption = 0;
            LastTicksConsumption = 0;
            DelaysCount = 0;

            int avgCount = 10;

            switch ((int)type)
            {
                case 0: updateRatio = 0.1f; maxDelay = .3f; avgCount = 10; delayTolerance = 3.5f; break;
                case 1: updateRatio = 0.4f; maxDelay = 1.1f; avgCount = 7; delayTolerance = 1.6f; break;
                case 2: updateRatio = .75f; maxDelay = 1.5f; avgCount = 5; delayTolerance = 1.3f; break;
                case 3: updateRatio = 1.25f; maxDelay = 3f; avgCount = 4; delayTolerance = 1.15f; break;
                case 4: updateRatio = 2.25f; maxDelay = 6; avgCount = 4; delayTolerance = 1f; break;
            }

            avgTicks = new int[avgCount];
            for (int i = 0; i < avgTicks.Length; i++) avgTicks[i] = 0;

            AdaptedDelay = updateRatio + 0.001f;
        }


        public IEnumerator WatchUpdate()
        {
            yield return null; // Three frames delay before starting work
            yield return null;

            while (true)
            {
                // Initializing variables to measure time consumption
                long totalElapsed = 0;
                long totalTicks = 0;
                DelaysCount = 0;

                float booster = Mathf.Lerp(1f, 2.375f, Manager.UpdateBoost);
                int ticksLimit = (int)(5000f * booster * delayTolerance);

                // Stopping all if we don't have manager somehow
                if (!Manager)
                {
                    Debug.LogError("[OPTIMIZERS] Manager is not existing anymore! Stopping dynamic clock! (" + OptimizingDistanceType + ")");
                    yield break;
                }

                watch.Start();


                // Going through all optimizers objects and updating their LOD levels and visibility states
                if (Manager.TargetCamera)
                    for (int i = Optimizers.Count - 1; i >= 0; i--)
                    {
                        if (Optimizers[i] == null)
                        {
                            Optimizers.RemoveAt(i);
                            continue;
                        }

                        Manager.CheckElement(Optimizers[i], i);

                        // If Checking takes too much time we wait one frame and continues
                        if (watch.ElapsedTicks > ticksLimit) // 10 000 ticks = 1ms
                        {
                            watch.Stop();

                            yield return null;

                            // Updating time consumption
                            DelaysCount++;
                            totalElapsed += watch.ElapsedMilliseconds;
                            totalTicks += watch.ElapsedTicks;
                            FrameTicksConsumption = watch.ElapsedTicks;

                            // Continue measurement properly
                            watch.Reset();
                            watch.Start();
                        }
                    }

                watch.Stop();

                // Calculating time needed to go through all components
                LastMSConsumption = totalElapsed + watch.ElapsedMilliseconds;
                LastTicksConsumption = totalTicks + watch.ElapsedTicks;
                AddAverage((int)LastTicksConsumption);
                UpdateAdaptation();

                yield return new WaitForSeconds(AdaptedDelay);
                yield return waitForLateUpdate;

                FrameTicksConsumption = watch.ElapsedTicks;
                watch.Reset();

                LastTickFrame = Time.frameCount;
            }
        }


        private void UpdateAdaptation()
        {
            float max = maxDelay;
            float divider = 1f;

            // Bossting update rate if setted
            if (Manager.UpdateBoost > 0f)
            {
                divider = (1f + Manager.UpdateBoost * 2f);
                max = maxDelay / (1f + Manager.UpdateBoost);

                if ((int)OptimizingDistanceType < 3)
                {
                    max /= (1f + Manager.UpdateBoost);
                    divider = (1f + Manager.UpdateBoost * 5);
                }
                else
                if ((int)OptimizingDistanceType == 3)
                {
                    max /= (1f + Manager.UpdateBoost / 2f);
                    divider = (1f + Manager.UpdateBoost * 3);
                }
                else
                if ((int)OptimizingDistanceType == 4)
                {
                    max /= (1f + Manager.UpdateBoost / 1.5f);
                    divider = (1f + Manager.UpdateBoost * 2.5f);
                }
            }

            // Adapting update delay depending on average time needed to go through all optimizers objects
            AdaptedDelay = (((float)GetAverage() / (30000 * 1f)) * updateRatio) / divider;
            if (AdaptedDelay > max) AdaptedDelay = max;
        }


        private void AddAverage(int ticks)
        {
            avgTicks[avgCounter] = ticks;
            avgCounter++;
            if (avgCounter >= avgTicks.Length) avgCounter = 0;
        }


        public int GetAverage()
        {
            int sum = 0;
            for (int i = 0; i < avgTicks.Length; i++)
                sum += avgTicks[i];

            return sum / avgTicks.Length;
        }
    }
}
