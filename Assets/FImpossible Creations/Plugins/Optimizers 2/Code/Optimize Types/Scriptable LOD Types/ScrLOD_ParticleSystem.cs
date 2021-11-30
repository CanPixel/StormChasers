using UnityEngine;

namespace FIMSpace.FOptimizing
{
    /// <summary>
    /// FC: Scriptable container for IFLOD
    /// </summary>
    //[CreateAssetMenu(menuName = "Custom Optimizers/FLOD_ParticleSystem Reference")]
    public sealed class ScrLOD_ParticleSystem : ScrLOD_Base
    {
        [SerializeField]
        private LODI_ParticleSystem settings;
        public override ILODInstance GetLODInstance() { return settings; }
        public ScrLOD_ParticleSystem() { settings = new LODI_ParticleSystem(); }


        public override ScrLOD_Base GetScrLODInstance()
        {
            ScrLOD_ParticleSystem lod = CreateInstance<ScrLOD_ParticleSystem>();
            //lod.CopyBase(this);
            return lod;
        }


        public override ScrLOD_Base CreateNewScrCopy()
        {
            ScrLOD_ParticleSystem newP = CreateInstance<ScrLOD_ParticleSystem>();
            //newP.CopyBase(this);
            //newP.EmmissionAmount = EmmissionAmount;
            //newP.OverDistanceMul = OverDistanceMul;
            //newP.BurstsAmount = BurstsAmount;
            //newP.Bursts = Bursts;
            //newP.MaxParticlAmount = MaxParticlAmount;
            //newP.LifetimeAlpha = LifetimeAlpha;
            //newP.ColorOverLifetime = ColorOverLifetime;
            //newP.ParticleSizeMul = ParticleSizeMul;
            return newP;
        }


        public override ScriptableLODsController GenerateLODController(Component target, ScriptableOptimizer optimizer)
        {
            ParticleSystem p = target as ParticleSystem;
            if (!p) p = target.GetComponentInChildren<ParticleSystem>();
            if (p) if (!optimizer.ContainsComponent(p))
                {
                    return new ScriptableLODsController(optimizer, p, -1, "Particles", this);
                }

            return null;
        }
    }
}
