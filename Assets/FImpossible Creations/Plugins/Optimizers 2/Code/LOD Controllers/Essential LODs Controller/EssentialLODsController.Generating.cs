using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.FOptimizing
{

    public partial class EssentialLODsController
    {
        private List<ILODInstance> _iflod;


        /// <summary>
        /// Getting list of interface type for quicker assign stuff in loops
        /// List of interface can't be serialized by unity so we temporarily creating IFLOD list for active use
        /// </summary>
        protected override List<ILODInstance> GetIFLODList()
        {
            if (_iflod != null)
            {
                if (_iflod.Count == eOptimizer.LODLevels + 2) return _iflod;
            }

            _iflod = new List<ILODInstance>();

            switch (ControlerType)
            {
                case EEssType.Particle:
                    for (int i = 0; i < optimizer.LODLevels + 2; i++) _iflod.Add(LODs_Particle[i]);
                    break;
                case EEssType.Light:
                    for (int i = 0; i < optimizer.LODLevels + 2; i++) _iflod.Add(LODs_Light[i]);
                    break;
                case EEssType.MonoBehaviour:
                    for (int i = 0; i < optimizer.LODLevels + 2; i++) _iflod.Add(LODs_Mono[i]);
                    break;
                case EEssType.Renderer:
                    for (int i = 0; i < optimizer.LODLevels + 2; i++) _iflod.Add(LODs_Renderer[i]);
                    break;
                case EEssType.NavMeshAgent:
                    for (int i = 0; i < optimizer.LODLevels + 2; i++) _iflod.Add(LODs_NavMesh[i]);
                    break;
                case EEssType.AudioSource:
                    for (int i = 0; i < optimizer.LODLevels + 2; i++) _iflod.Add(LODs_Audio[i]);
                    break;
                case EEssType.Rigidbody:
                    for (int i = 0; i < optimizer.LODLevels + 2; i++) _iflod.Add(LODs_Rigidbody[i]);
                    break;
            }

            return _iflod;
        }


        protected override void GenerateNewLODSettings()
        {
            if (ControlerType == EEssType.Unknown) { UnityEngine.Debug.Log("[Optimizers] Unknown to optimize type!"); return; }

            switch (ControlerType)
            {
                case EEssType.Particle: LODs_Particle = new List<LODI_ParticleSystem>(); break;
                case EEssType.Light: LODs_Light = new List<LODI_Light>(); break;
                case EEssType.MonoBehaviour: LODs_Mono = new List<LODI_MonoBehaviour>(); break;
                case EEssType.Renderer: LODs_Renderer = new List<LODI_Renderer>(); break;
                case EEssType.NavMeshAgent: LODs_NavMesh = new List<LODI_NavMeshAgent>(); break;
                case EEssType.AudioSource: LODs_Audio = new List<LODI_AudioSource>(); break;
                case EEssType.Rigidbody: LODs_Rigidbody = new List<LODI_Rigidbody>(); break;
            }
        }


        /// <summary>
        /// Generating initial settings empty instance
        /// </summary>
        private void GenerateInitialSettings()
        {
            switch (ControlerType)
            {
                case EEssType.Particle: Ini_Particle = new LODI_ParticleSystem(); break;
                case EEssType.Light: Ini_Light = new LODI_Light(); break;
                case EEssType.MonoBehaviour: Ini_Mono = new LODI_MonoBehaviour(); break;
                case EEssType.Renderer: Ini_Rend = new LODI_Renderer(); break;
                case EEssType.NavMeshAgent: Ini_Nav = new LODI_NavMeshAgent(); break;
                case EEssType.AudioSource: Ini_Audio = new LODI_AudioSource(); break;
                case EEssType.Rigidbody: Ini_Rigidbody = new LODI_Rigidbody(); break;
            }
        }


        private ILODInstance GenerateInstance()
        {
            switch (ControlerType)
            {
                case EEssType.Particle: return new LODI_ParticleSystem();
                case EEssType.Light: return new LODI_Light();
                case EEssType.MonoBehaviour: return new LODI_MonoBehaviour();
                case EEssType.Renderer: return new LODI_Renderer();
                case EEssType.NavMeshAgent: return new LODI_NavMeshAgent();
                case EEssType.AudioSource: return new LODI_AudioSource();
                case EEssType.Rigidbody: return new LODI_Rigidbody();
            }

            return null;
        }


        /// <summary>
        /// Checking if LOD parameters need to be generated for needed LOD levels count.
        /// If count is invalid to needed one, new LOD parameters are generated (empty ones)
        /// </summary>
        protected override void CheckAndGenerateLODParameters()
        {
            // Checking again count in case if it was cleared in previous lines of code
            if (GetLODSettingsCount() != optimizer.LODLevels + 2)
            {
                switch (ControlerType)
                {
                    case EEssType.Particle:
                        for (int i = 0; i < optimizer.LODLevels + 2; i++) LODs_Particle.Add(new LODI_ParticleSystem());
                        break;
                    case EEssType.Light:
                        for (int i = 0; i < optimizer.LODLevels + 2; i++) LODs_Light.Add(new LODI_Light());
                        break;
                    case EEssType.MonoBehaviour:
                        for (int i = 0; i < optimizer.LODLevels + 2; i++) LODs_Mono.Add(new LODI_MonoBehaviour());
                        break;
                    case EEssType.Renderer:
                        for (int i = 0; i < optimizer.LODLevels + 2; i++) LODs_Renderer.Add(new LODI_Renderer());
                        break;
                    case EEssType.NavMeshAgent:
                        for (int i = 0; i < optimizer.LODLevels + 2; i++) LODs_NavMesh.Add(new LODI_NavMeshAgent());
                        break;
                    case EEssType.AudioSource:
                        for (int i = 0; i < optimizer.LODLevels + 2; i++) LODs_Audio.Add(new LODI_AudioSource());
                        break;
                    case EEssType.Rigidbody:
                        for (int i = 0; i < optimizer.LODLevels + 2; i++) LODs_Rigidbody.Add(new LODI_Rigidbody());
                        break;
                }
            }

            RefreshOptimizerLODCount();
        }


        internal override void ApplyLODLevelSettings(ILODInstance currentLOD)
        {
            if (currentLOD == null) return;

            CurrentLODLevel = currentLOD.Index;
            if (IsTransitioningOrOther()) CurrentLODLevel = -1;
            currentLOD.ApplySettingsToTheComponent(Component, InitialSettings);
        }
    }

}
