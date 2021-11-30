using UnityEngine;

namespace FIMSpace.FOptimizing
{
    /// <summary>
    /// FC: Scriptable container for IFLOD
    /// </summary>
    //[CreateAssetMenu(menuName = "Custom Optimizers/FLOD_AudioSource Reference")]
    public sealed class ScrLOD_AudioSource : ScrLOD_Base
    {
        [SerializeField]
        private LODI_AudioSource settings;
        public override ILODInstance GetLODInstance() { return settings; }
        public ScrLOD_AudioSource() { settings = new LODI_AudioSource(); }

        public override ScrLOD_Base GetScrLODInstance()
        { return CreateInstance<ScrLOD_AudioSource>(); }


        public override ScrLOD_Base CreateNewScrCopy()
        {
            ScrLOD_AudioSource newA = CreateInstance<ScrLOD_AudioSource>();
            newA.settings = settings.GetCopy() as LODI_AudioSource;
            return newA;
        }

        public override ScriptableLODsController GenerateLODController(Component target, ScriptableOptimizer optimizer)
        {
            AudioSource a = target as AudioSource;
            if (!a) a = target.GetComponentInChildren<AudioSource>();
            if (a) if (!optimizer.ContainsComponent(a))
                {
                    return new ScriptableLODsController(optimizer, a, -1, "Audio Source", this);
                }

            return null;
        }
    }
}
