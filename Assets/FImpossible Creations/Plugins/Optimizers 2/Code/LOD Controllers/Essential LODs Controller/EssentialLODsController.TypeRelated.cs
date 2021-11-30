using UnityEngine;
using UnityEngine.AI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.FOptimizing
{

    public partial class EssentialLODsController
    {

        public override ILODInstance GetLODSetting(int lod)
        {
            switch (ControlerType)
            {
                case EEssType.Particle: return LODs_Particle[lod];
                case EEssType.Light: return LODs_Light[lod];
                case EEssType.MonoBehaviour: return LODs_Mono[lod];
                case EEssType.Renderer: return LODs_Renderer[lod];
                case EEssType.NavMeshAgent: return LODs_NavMesh[lod];
                case EEssType.AudioSource: return LODs_Audio[lod];
                case EEssType.Rigidbody: return LODs_Rigidbody[lod];
            }

            return null;
        }


        public override int GetLODSettingsCount()
        {
            switch (ControlerType)
            {
                case EEssType.Particle: if (LODs_Particle == null) return 0; else return LODs_Particle.Count;
                case EEssType.Light: if (LODs_Light == null) return 0; else return LODs_Light.Count;
                case EEssType.MonoBehaviour: if (LODs_Mono == null) return 0; else return LODs_Mono.Count;
                case EEssType.Renderer: if (LODs_Renderer == null) return 0; else return LODs_Renderer.Count;
                case EEssType.NavMeshAgent: if (LODs_NavMesh == null) return 0; else return LODs_NavMesh.Count;
                case EEssType.AudioSource: if (LODs_Audio == null) return 0; else return LODs_Audio.Count;
                case EEssType.Rigidbody: if (LODs_Rigidbody == null) return 0; else return LODs_Rigidbody.Count;
            }

            return 0;
        }


        public override ILODInstance InitialSettings
        {
            get
            {
                switch (ControlerType)
                {
                    case EEssType.Particle: return Ini_Particle;
                    case EEssType.Light: return Ini_Light;
                    case EEssType.MonoBehaviour: return Ini_Mono;
                    case EEssType.Renderer: return Ini_Rend;
                    case EEssType.NavMeshAgent: return Ini_Nav;
                    case EEssType.AudioSource: return Ini_Audio;
                    case EEssType.Rigidbody: return Ini_Rigidbody;
                }

                return null;
            }

            protected set
            {
                switch (ControlerType)
                {
                    case EEssType.Particle: Ini_Particle = value as LODI_ParticleSystem; break;
                    case EEssType.Light: Ini_Light = value as LODI_Light; break;
                    case EEssType.MonoBehaviour: Ini_Mono = value as LODI_MonoBehaviour; break;
                    case EEssType.Renderer: Ini_Rend = value as LODI_Renderer; break;
                    case EEssType.NavMeshAgent: Ini_Nav = value as LODI_NavMeshAgent; break;
                    case EEssType.AudioSource: Ini_Audio = value as LODI_AudioSource; break;
                    case EEssType.Rigidbody: Ini_Rigidbody = value as LODI_Rigidbody; break;
                }
            }
        }

        internal static EEssType GetEssentialType(Component target)
        {
            ParticleSystem ps = target as ParticleSystem; if (ps) return EEssType.Particle;
            Light l = target as Light; if (l) return EEssType.Light;
            MonoBehaviour m = target as MonoBehaviour; if (m) return EEssType.MonoBehaviour;

            Renderer r = target as Renderer; if (r)
            {
                if (r as ParticleSystemRenderer) return EEssType.Unknown;
                else
                return EEssType.Renderer;
            }

            NavMeshAgent nv = target as NavMeshAgent; if (nv) return EEssType.NavMeshAgent;
            AudioSource au = target as AudioSource; if (au) return EEssType.AudioSource;

            return EEssType.Unknown;
        }

        /// <summary>
        /// Separated method to detect any type of component to optimize, also for advanced users
        /// </summary>
        internal static EEssType GetEssentialTypeAll(Component target)
        {
            EEssType type = GetEssentialType(target);

            if ( type == EEssType.Unknown)
            {
                Rigidbody rg = target as Rigidbody; if (rg) return EEssType.Rigidbody;
            }

            return type;
        }
    }
}
