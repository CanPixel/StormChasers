using UnityEngine;

namespace FIMSpace.FOptimizing
{
    /// <summary>
    /// FC: Scriptable container for IFLOD
    /// </summary>
    [CreateAssetMenu(menuName = "Custom Optimizers/FLOD_Rigidbody Reference")]
    public sealed class ScrLOD_Rigidbody : ScrLOD_Base
    {
        [SerializeField]
        private LODI_Rigidbody settings;
        public override ILODInstance GetLODInstance() { return settings; }
        public ScrLOD_Rigidbody() { settings = new LODI_Rigidbody(); }

        public override ScrLOD_Base GetScrLODInstance()
        { return CreateInstance<ScrLOD_AudioSource>(); }


        public override ScrLOD_Base CreateNewScrCopy()
        {
            ScrLOD_Rigidbody newA = CreateInstance<ScrLOD_Rigidbody>();
            newA.settings = settings.GetCopy() as LODI_Rigidbody;
            return newA;
        }

        public override ScriptableLODsController GenerateLODController(Component target, ScriptableOptimizer optimizer)
        {
            Rigidbody a = target as Rigidbody;
            if (!a) a = target.GetComponentInChildren<Rigidbody>();
            if (a) if (!optimizer.ContainsComponent(a))
                {
                    return new ScriptableLODsController(optimizer, a, -1, "Rigidbody", this);
                }

            return null;
        }
    }
}
