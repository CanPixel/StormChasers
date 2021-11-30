using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.FOptimizing
{
    /// <summary>
    /// FM: Class which is helping holding settings and references for one optimized component.
    /// > Containing reference to target optimized component from scene/prefab
    /// > Handling applying changes to target optimized component in playmode
    /// > Handling drawing editor windows elements for optimization settings etc.
    /// > Containing ref to LOD settings of target component
    /// </summary>
    [System.Serializable]
    public partial class ScriptableLODsController : LODsControllerBase
    {
        /// <summary> Current used LOD Set: shared or unique </summary>
        public ScrOptimizer_LODSettings LODSet;
        /// <summary> If LOD Set is assigned from asset </summary>
        [SerializeField]
        private ScrOptimizer_LODSettings sharedLODSet;
        /// <summary> If LOD Set for this component is individual </summary>
        [SerializeField]
        private ScrOptimizer_LODSettings uniqueLODSet;

        public ScrOptimizer_LODSettings GetSharedSet()
        { return sharedLODSet; }

        public ScrOptimizer_LODSettings GetUniqueSet()
        { return uniqueLODSet; }

        /// <summary> Optimizer which is optimizing target component </summary>
        [SerializeField] private ScriptableOptimizer sOptimizer;
        public ScriptableOptimizer GetOptimizer { get { return sOptimizer; } }

        [HideInInspector]
        /// <summary> Reference to target type of component to optimize, it needs to be saved as asset inside project to make it work with prefabs </summary>
        public ScrLOD_Base RootReference;

        [SerializeField] [HideInInspector] private ILODInstance initialSettings;
        public override ILODInstance InitialSettings { get { return initialSettings; } protected set { initialSettings = value; } }


        public ScriptableLODsController(Optimizer_Base sourceOptimizer, Component toOptimize, int index, string header = "", ScrLOD_Base rootReference = null) : base(sourceOptimizer, toOptimize, index, header)
        {
            sOptimizer = sourceOptimizer as ScriptableOptimizer;
            RootReference = rootReference;
        }


        [HideInInspector]
        public bool UsingShared = false;

        public override ILODInstance GetLODSetting(int i)
        {
            if (LODSet == null) GenerateLODParameters(); else if (LODSet.LevelOfDetailSets == null) GenerateLODParameters();
            return LODSet.LevelOfDetailSets[i].GetLODInstance();
        }

        public override int GetLODSettingsCount()
        {
            return LODSet.LevelOfDetailSets.Count;
        }

        public override void OnStart()
        {
            if (!RootReference) return;
            if (InitialSettings == null) InitialSettings = RootReference.GetScrLODInstance().GetLODInstance();
            InitialSettings.SetSameValuesAsComponent(Component);
        }

        internal override void ApplyLODLevelSettings(ILODInstance currentLOD)
        {
            if (currentLOD == null)
            {
                if (RootReference == null)
                    Debug.LogError("[OPTIMIZERS] CRITICAL ERROR: There is no root reference in Optimizer's LOD Controller! (" + optimizer + ") " + "Try adding Optimizers Manager again to the scene or import newest version from the Asset Store!");

                Debug.LogError("[OPTIMIZERS] Target LOD is NULL! (" + optimizer.name + " - " + RootReference.name + ")");
                return;
            }

            CurrentLODLevel = currentLOD.Index;
            if (IsTransitioningOrOther()) CurrentLODLevel = -1;
            currentLOD.ApplySettingsToTheComponent(Component, InitialSettings);
        }

        protected override void RefreshToOptimizeIndex()
        {
            for (int i = 0; i < sOptimizer.ToOptimize.Count; i++)
            {
                if (sOptimizer.ToOptimize[i] == this)
                {
                    ToOptimizeIndex = i;
                    return;
                }
            }
        }

        internal override ILODInstance GetCurrentLOD()
        {
            return LODSet.LevelOfDetailSets[CurrentLODLevel].GetLODInstance();
        }

        internal override ILODInstance GetCullingLOD()
        {
            return LODSet.LevelOfDetailSets[LODSet.LevelOfDetailSets.Count - 2].GetLODInstance();
        }

        internal override ILODInstance GetHiddenLOD()
        {
            return LODSet.LevelOfDetailSets[LODSet.LevelOfDetailSets.Count - 1].GetLODInstance();
        }


        private List<ILODInstance> _iflod;
        protected override List<ILODInstance> GetIFLODList()
        {
            if (_iflod == null || _iflod.Count != GetLODSettingsCount())
            {
                _iflod = new List<ILODInstance>();
                for (int i = 0; i < LODSet.LevelOfDetailSets.Count; i++)
                {
                    _iflod.Add(LODSet.LevelOfDetailSets[i].GetLODInstance());
                }
            }

            return _iflod;
        }


#if UNITY_2018_3_OR_NEWER
        /// <summary> Flag used in Unity Version 2018.3+ to create prefab with LOD settings from scene </summary>
        public int nullTry = 0;
#endif

        protected override void Editor_MonoSimplyfy()
        {
#if UNITY_EDITOR
            Optimizers_LODTransport.RemoveLODControllerSubAssets(this, true);
#endif
        }

    }
}
