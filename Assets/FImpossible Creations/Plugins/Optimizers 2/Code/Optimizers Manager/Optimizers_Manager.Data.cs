using System;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.FOptimizing
{
    public partial class OptimizersManager
    {
        //private readonly List<FOptimizer_Base> staticOptimizers = new List<FOptimizer_Base>();
        //private readonly List<FOptimizer_Base> dynamicOptimizers = new List<FOptimizer_Base>();
        //private readonly List<FOptimizer_Base> effectiveOptimizers = new List<FOptimizer_Base>();
        //private readonly List<FOptimizer_Base> triggerOptimizers = new List<FOptimizer_Base>();

        public List<Optimizer_Base> notContainedStaticOptimizers = new List<Optimizer_Base>();
        public List<Optimizer_Base> notContainedDynamicOptimizers = new List<Optimizer_Base>();
        public List<Optimizer_Base> notContainedEffectiveOptimizers = new List<Optimizer_Base>();
        public List<Optimizer_Base> notContainedTriggerOptimizers = new List<Optimizer_Base>();

        public void RegisterNotContainedOptimizer(Optimizer_Base optimizer, bool init = false)
        {
            switch (optimizer.OptimizingMethod)
            {
                case EOptimizingMethod.Static: RegisterNotContainedStaticOptimizer(optimizer, init); break;
                case EOptimizingMethod.Dynamic: RegisterNotContainedDynamicOptimizer(optimizer, init); break;
                case EOptimizingMethod.Effective: RegisterNotContainedEffectiveOptimizer(optimizer, init); break;
                case EOptimizingMethod.TriggerBased: RegisterNotContainedTriggerOptimizer(optimizer, init); break;
            }
        }

        public void RegisterNotContainedStaticOptimizer(Optimizer_Base optimizer, bool init = false)
        {
            if (init) notContainedStaticOptimizers.Add(optimizer); else if (!notContainedStaticOptimizers.Contains(optimizer)) notContainedStaticOptimizers.Add(optimizer);
        }

        public void RegisterNotContainedDynamicOptimizer(Optimizer_Base optimizer, bool init = false)
        {
            if (init) notContainedDynamicOptimizers.Add(optimizer); else if (!notContainedDynamicOptimizers.Contains(optimizer)) notContainedDynamicOptimizers.Add(optimizer);
        }

        public void RegisterNotContainedEffectiveOptimizer(Optimizer_Base optimizer, bool init = false)
        {
            if (init) notContainedEffectiveOptimizers.Add(optimizer); else if (!notContainedEffectiveOptimizers.Contains(optimizer)) notContainedEffectiveOptimizers.Add(optimizer);
        }

        public void RegisterNotContainedTriggerOptimizer(Optimizer_Base optimizer, bool init = false)
        {
            if (init) notContainedTriggerOptimizers.Add(optimizer); else if (!notContainedTriggerOptimizers.Contains(optimizer)) notContainedTriggerOptimizers.Add(optimizer);
        }



        public void UnRegisterOptimizer(Optimizer_Base optimizer)
        {
            if (optimizer.AddToContainer) return;

            switch (optimizer.OptimizingMethod)
            {
                case EOptimizingMethod.Static: UnRegisterStaticOptimizer(optimizer); break;
                case EOptimizingMethod.Dynamic: UnRegisterDynamicOptimizer(optimizer); break;
                case EOptimizingMethod.Effective: UnRegisterEffectiveOptimizer(optimizer); break;
                case EOptimizingMethod.TriggerBased: UnRegisterTriggerOptimizer(optimizer); break;
            }
        }

        public void UnRegisterStaticOptimizer(Optimizer_Base optimizer)
        {
            if (notContainedStaticOptimizers.Contains(optimizer)) notContainedStaticOptimizers.Remove(optimizer);
        }

        public void UnRegisterDynamicOptimizer(Optimizer_Base optimizer)
        {
            if (!notContainedDynamicOptimizers.Contains(optimizer)) notContainedDynamicOptimizers.Remove(optimizer);
        }

        public void UnRegisterEffectiveOptimizer(Optimizer_Base optimizer)
        {
            if (!notContainedEffectiveOptimizers.Contains(optimizer)) notContainedEffectiveOptimizers.Remove(optimizer);
        }

        public void UnRegisterTriggerOptimizer(Optimizer_Base optimizer)
        {
            if (!notContainedTriggerOptimizers.Contains(optimizer)) notContainedTriggerOptimizers.Remove(optimizer);
        }

        private Optimizers_CullingContainer _editorToDrawContainer;
        public void DrawBounds(Optimizers_CullingContainer cont)
        {
            _editorToDrawContainer = cont;
        }
    }
}
