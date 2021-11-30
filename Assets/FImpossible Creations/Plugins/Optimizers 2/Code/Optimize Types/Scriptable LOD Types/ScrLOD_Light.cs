using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.FOptimizing
{
    /// <summary>
    /// FC: Scriptable container for IFLOD
    /// </summary>
    //[CreateAssetMenu(menuName = "Custom Optimizers/FLOD_Light Reference")]
    public sealed class ScrLOD_Light : ScrLOD_Base
    {
        [SerializeField]
        private LODI_Light settings;
        public override ILODInstance GetLODInstance() { return settings; }
        public ScrLOD_Light() { settings = new LODI_Light(); }

        public override ScrLOD_Base GetScrLODInstance()
        {
            return CreateInstance<ScrLOD_Light>();
        }

        public override ScrLOD_Base CreateNewScrCopy()
        {
            ScrLOD_Light lightA = CreateInstance<ScrLOD_Light>();

            //lightA.IntensityMul = IntensityMul;
            //lightA.RangeMul = RangeMul;
            //lightA.ShadowsMode = ShadowsMode;
            //lightA.ShadowsStrength = ShadowsStrength;
            //lightA.RenderMode = RenderMode;
            //lightA.ChangeIntensity = ChangeIntensity;
            return lightA;
        }


        public override ScriptableLODsController GenerateLODController(Component target, ScriptableOptimizer optimizer)
        {
            Light light = target as Light;
            if (!light) light = target.gameObject.GetComponentInChildren<Light>();

            if (light) if (!optimizer.ContainsComponent(light))
                {
                    //float scaler = optimizer.transform.lossyScale.x;
                    
                    optimizer.DetectionRadius = light.range;
                    if (optimizer.transform.lossyScale.x != 0f) optimizer.DetectionRadius *= 1f / optimizer.transform.lossyScale.x;
                    
                    optimizer.DetectionBounds = Vector3.one * light.range * 1.8f;

                    if ( optimizer.transform != light.transform)
                    {
                        optimizer.DetectionOffset = optimizer.transform.InverseTransformPoint(light.transform.position);
                    }

                    return new ScriptableLODsController(optimizer, light, -1, "Light Properties", this);
                }

            return null;
        }


        internal void GetLODsController(object target, Optimizer_Base fOptimizer_Base)
        {
            throw new NotImplementedException();
        }

    }
}