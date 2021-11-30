#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace FIMSpace.FOptimizing
{
    /// <summary>
    /// FM: Class with data for unity component LOD level
    /// </summary>
    [System.Serializable]
    public sealed class LODI_AudioSource : ILODInstance
    {
        #region Main Settings : Interface Properties

        public int Index { get { return index; } set { index = value; } }
        internal int index = -1;
        public string Name { get { return LODName; } set { LODName = value; } }
        internal string LODName = "";
        public bool CustomEditor { get { return false; } }
        public bool Disable { get { return SetDisabled; } set { SetDisabled = value; } }
        [HideInInspector] public bool SetDisabled = false;
        public bool DrawDisableOption { get { return true; } }
        public bool SupportingTransitions { get { return true; } }
        public bool DrawLowererSlider { get { return false; } }
        public float QualityLowerer { get { return 1f; } set { new System.NotImplementedException(); } }
        public string HeaderText { get { return "AudioSource LOD Settings"; } }
        public int DrawingVersion { get { return 1; } set { new System.NotImplementedException(); } }
        public float ToCullDelay { get { return 0f; } }
        public Texture Icon { get { return
#if UNITY_EDITOR
            EditorGUIUtility.IconContent("AudioSource Icon").image;
#else
        null;
#endif
            } }

        public Component TargetComponent { get { return cmp; } }
        [SerializeField][HideInInspector] private AudioSource cmp;

        #endregion


        [Range(0f, 1f)]
        [Tooltip("Setted to zero will result with priority = 256 so marked as NOT important audio source, marked as 100% will result with priority level like audio source had when initialized")]
        public float PriorityFactor = 1f;

        [HideInInspector]
        public float Volume = 1f;
        private bool unPause = false;


        public void SetSameValuesAsComponent(Component component)
        {
            if (component == null) return;

            AudioSource aS = component as AudioSource;
            cmp = aS;
            PriorityFactor = aS.priority;
            Volume = aS.volume;
        }


        public void InterpolateBetween(ILODInstance a, ILODInstance b, float transitionToB)
        {
            FLOD.DoBaseInterpolation(this, a, b, transitionToB);

            LODI_AudioSource aa = a as LODI_AudioSource;
            LODI_AudioSource bb = b as LODI_AudioSource;

            PriorityFactor = bb.PriorityFactor;
            Volume = Mathf.Lerp(aa.Volume, bb.Volume, transitionToB);
        }


        public void ApplySettingsToTheComponent(Component component, ILODInstance initialSettings)
        {
            AudioSource comp = component as AudioSource;

            LODI_AudioSource initials = initialSettings as LODI_AudioSource;

            comp.priority = (int)Mathf.Lerp(255, initials.PriorityFactor, PriorityFactor);
            comp.volume = initials.Volume * Volume;

            if (Disable)
            {
                if (comp.isPlaying) if (comp.loop) { comp.Pause(); unPause = true; }
                comp.enabled = false;
            }
            else
            {
                if (unPause)
                { unPause = false; comp.UnPause(); }
                comp.enabled = true;
            }
        }


        public void AssignAutoSettingsAsForLODLevel(int lodIndex, int lodCount, Component source)
        {
            AudioSource comp = source as AudioSource;
            if (comp == null) Debug.LogError("[OPTIMIZERS] Given component for reference values is null or is not AudioSource Component!");

            float mul = FLOD.GetValueForLODLevel(1f, 0f, lodIndex - 1, lodCount);
            if (lodIndex > 0) PriorityFactor = mul;
            //name = "LOD" + (lodIndex + 2); // + 2 to view it in more responsive way for user inside inspector window
            Volume = 1f;
        }


        public void AssignSettingsAsForCulled(Component component)
        {
            FLOD.AssignDefaultCulledParams(this);
            PriorityFactor = 0f;
            Volume = 0f;
        }


        public void AssignSettingsAsForNearest(Component component)
        {
            FLOD.AssignDefaultNearestParams(this);
            PriorityFactor = 1f;
            Volume = 1f;
        }

        public ILODInstance GetCopy() { return MemberwiseClone() as ILODInstance; }

        public void AssignSettingsAsForHidden(Component component)
        { FLOD.AssignDefaultHiddenParams(this); }


        // Custom Inspector Features ---------------------------------------------


#if UNITY_EDITOR
        public void AssignToggler(ILODInstance reference)
        { }

        public void DrawTogglers(SerializedProperty iflodProp)
        { }

        public void CustomEditorWindow( SerializedProperty prop)
        { }
#endif

    }
}
