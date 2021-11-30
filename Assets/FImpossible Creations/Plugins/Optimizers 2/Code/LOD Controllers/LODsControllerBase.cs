using UnityEngine;

namespace FIMSpace.FOptimizing
{
    /// <summary>
    /// FC: Class which contains LOD levels for one type of component to be optimized by optimizer
    /// </summary>
    [System.Serializable]
    public partial class LODsControllerBase
    {
        /// <summary> Making sure LODs Container was created by optimizer and not by Unity's automatic serialization </summary>
        public bool Constructed { get { return constructed; } }
        [SerializeField]
        private bool constructed = false;

        public int ToOptimizeIndex = -1;

        /// <summary> Optimizer which is optimizing target component </summary>
        [SerializeField] protected Optimizer_Base optimizer;

        /// <summary> Component choosed to be optimized </summary>
        public Component Component;

        /// <summary> For custom coding, for example drawing two different versions of LOD settings (implemented in MonoBehaviour type for simple and advanced) </summary>
        public int Version = 0;

        /// <summary> Making first LOD properties view as unactive inside inspector window </summary>
        [SerializeField] [HideInInspector] protected bool lockFirstLOD = true;


#if UNITY_EDITOR
        /// <summary> Drawing optimized component name string </summary>
        [SerializeField] [HideInInspector] private string editorHeader = "";
        /// <summary> Drawing this component's properties </summary>
        [SerializeField] [HideInInspector] private bool drawProperties = true;
#endif

        /// <summary> Count of LOD Levels. NUMBER WITHOUT: + 2 (hidden and culled) </summary>
        public int LODLevelsCount { get { return optimizer.LODLevels; } }
        /// <summary> Current selected and applied LOD level </summary>
        public int CurrentLODLevel { get; protected set; }
        /// <summary> Just any LOD instance from LOD controller </summary>
        public ILODInstance ReferenceLOD { get { if (GetLODSettingsCount() > 0) return GetLODSetting(0); return null; } }

        /// <summary> Reference to FLOD_Base but with unclamped ranges pointing to true settings values of the component which is generated when game starts </summary>
        public virtual ILODInstance InitialSettings { get; protected set; }

        public LODsControllerBase(Optimizer_Base sourceOptimizer, Component toOptimize, int index, string header = "")
        {
            ToOptimizeIndex = index;
            optimizer = sourceOptimizer;
            Component = toOptimize;
#if UNITY_EDITOR
            editorHeader = header;
#endif
            constructed = true;
        }

        /// <summary> Getting LOD instance for LOD level </summary>
        public abstract ILODInstance GetLODSetting(int lod);

        /// <summary> Get LOD settings count for type of optimizer </summary>
        public abstract int GetLODSettingsCount();


        /// <summary>
        /// Refreshing values of LOD parameters to be the same like target optimized component values
        /// </summary>
        public virtual void OnStart()
        { }


        #region Playmode LOD Settings Applying Operations

        /// <summary> Setting LOD Level int value (not changing or applying lod settings) </summary>
        internal void SetCurrentLODLevel(int currentLODLevel)
        {
            CurrentLODLevel = currentLODLevel;
            if (currentLODLevel >= GetLODSettingsCount()) CurrentLODLevel = GetLODSettingsCount() - 1;
        }

        /// <summary> Apply LOD instance settings for optimized component </summary>
        internal abstract void ApplyLODLevelSettings(ILODInstance currentLOD);
        /// <summary> LOD Instance of current used LOD level </summary>
        internal abstract ILODInstance GetCurrentLOD();
        /// <summary> LOD Instance of culling LOD level </summary>
        internal abstract ILODInstance GetCullingLOD();
        /// <summary> LOD Instance of hidden LOD level </summary>
        internal abstract ILODInstance GetHiddenLOD();

        #endregion


        /// <summary>
        /// Checking if parameters transition is in progress
        /// </summary>
        public bool IsTransitioningOrOther()
        {
            if (CurrentLODLevel >= 0 && CurrentLODLevel <= GetLODSettingsCount()) return false; else return true;
        }

    }

}
