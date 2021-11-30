using System.Collections.Generic;
using UnityEngine;

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
    /// 
    /// Essential component requires hard coded references to target optimized components in order
    /// to work with unity prefabing in confortable way and support nested prefabs
    /// </summary>
    [System.Serializable]
    public partial class EssentialLODsController : LODsControllerBase
    {
        // Essential LODs Controller will use one of this lists
        // One Essential LODs Controller is supporting one type of unity component
        // All types are here for support
        public List<LODI_ParticleSystem> LODs_Particle;
        public List<LODI_Light> LODs_Light;
        public List<LODI_MonoBehaviour> LODs_Mono;
        public List<LODI_Renderer> LODs_Renderer;
        public List<LODI_NavMeshAgent> LODs_NavMesh;
        public List<LODI_AudioSource> LODs_Audio;
        public List<LODI_Rigidbody> LODs_Rigidbody;

        [UnityEngine.SerializeField] private LODI_ParticleSystem Ini_Particle;
        [UnityEngine.SerializeField] private LODI_Light Ini_Light;
        [UnityEngine.SerializeField] private LODI_MonoBehaviour Ini_Mono;
        [UnityEngine.SerializeField] private LODI_Renderer Ini_Rend;
        [UnityEngine.SerializeField] private LODI_NavMeshAgent Ini_Nav;
        [UnityEngine.SerializeField] private LODI_AudioSource Ini_Audio;
        [UnityEngine.SerializeField] private LODI_Rigidbody Ini_Rigidbody;

        [UnityEngine.SerializeField] private EssentialOptimizer eOptimizer;

        public enum EEssType : int
        {
            Unknown = 0, Particle = 1, Light = 2, MonoBehaviour = 3, Renderer = 4, NavMeshAgent = 5, AudioSource = 6, Rigidbody = 7
        }

        public EEssType ControlerType = EEssType.Unknown;

        public EssentialLODsController(Optimizer_Base sourceOptimizer, Component toOptimize, int index, string header = "") : base(sourceOptimizer, toOptimize, index, header)
        {
            eOptimizer = sourceOptimizer as EssentialOptimizer;
            // Defining type of optimized component for the controller
            ControlerType = GetEssentialTypeAll(toOptimize);
        }

        public override void OnStart()
        {
            if (InitialSettings == null) GenerateInitialSettings();
            InitialSettings.SetSameValuesAsComponent(Component);
        }


        protected override void RefreshToOptimizeIndex()
        {
            for (int i = 0; i < eOptimizer.ToOptimize.Count; i++)
            {
                if (eOptimizer.ToOptimize[i] == this)
                {
                    ToOptimizeIndex = i;
                    return;
                }
            }
        }


#if UNITY_EDITOR
        protected override SerializedObject GetSerializedObject()
        {
            return new SerializedObject(eOptimizer);
        }

        protected override SerializedProperty GetSerializedLODPropertyFor(int lod)
        {
            if (lod < 0) return null;
            if (lod >= GetLODSettingsCount()) return null;

            string serializedPropList = "";
            switch (ControlerType)
            {
                case EEssType.Particle: serializedPropList = "LODs_Particle"; break;
                case EEssType.Light: serializedPropList = "LODs_Light"; break;
                case EEssType.MonoBehaviour: serializedPropList = "LODs_Mono"; break;
                case EEssType.Renderer: serializedPropList = "LODs_Renderer"; break;
                case EEssType.NavMeshAgent: serializedPropList = "LODs_NavMesh"; break;
                case EEssType.AudioSource: serializedPropList = "LODs_Audio"; break;
                case EEssType.Rigidbody: serializedPropList = "LODs_Rigidbody"; break;
            }

            RefreshToOptimizeIndex();

            SerializedObject s = new SerializedObject(eOptimizer);
            SerializedProperty lodContr = s.FindProperty("ToOptimize").GetArrayElementAtIndex(ToOptimizeIndex);

            return lodContr.FindPropertyRelative(serializedPropList).GetArrayElementAtIndex(lod);
        }
#endif


        internal override ILODInstance GetCurrentLOD()
        {
            return GetIFLODList()[CurrentLODLevel];
        }

        internal override ILODInstance GetCullingLOD()
        {
            return GetIFLODList()[GetIFLODList().Count - 2];
        }

        internal override ILODInstance GetHiddenLOD()
        {
            return GetIFLODList()[GetIFLODList().Count - 1];
        }
    }
}
