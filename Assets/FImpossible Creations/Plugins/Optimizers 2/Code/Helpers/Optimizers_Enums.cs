using UnityEngine;

namespace FIMSpace.FOptimizing
{
    public enum EOptimizingMethod
    {
        [Tooltip("Using just Unity's Culling Groups API, detection sphere and static distance ranges from initial position")]
        /// <summary> Using just Unity's Culling Groups API, detection sphere and static distance ranges from initial position </summary>
        Static,
        [Tooltip("No Unity's Culing Groups API involved, just Optimizers Manager different interval clocks")]
        /// <summary> No Unity's Culing Groups API involved, just Optimizers Manager different interval clocks </summary>
        Dynamic,
        [Tooltip("Detecting if object stays in one place, then using refreshing Culling Groups API with Optimizers Manager clocks to effectively detect object visibility and detect distances like Dynamic method")]
        /// <summary> Detecting if object stays in one place, then using refreshing Culling Groups API with Optimizers Manager clocks to effectively detect object visibility and detect distances like Dynamic method </summary>
        Effective,
        [Tooltip("Defining optimization levels with trigger colliders setup")]
        /// <summary> Defining optimization levels with trigger colliders setup </summary>
        TriggerBased
    }

    public enum EOptimizingDistance : int
    {
        Nearest,
        Near,
        MidFar,
        Far,
        Farthest
    }
}