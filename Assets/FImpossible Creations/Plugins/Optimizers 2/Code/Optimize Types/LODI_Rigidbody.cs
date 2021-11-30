using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.FOptimizing
{
    /// <summary>
    /// FM: Helper class for single LOD level settings on Unity's Light Component
    /// </summary>
    [System.Serializable]
    public sealed class LODI_Rigidbody : ILODInstance
    {
        #region Main Settings : Interface Properties

        public int Index { get { return index; } set { index = value; } }
        internal int index = -1;
        public string Name { get { return LODName; } set { LODName = value; } }
        internal string LODName = "";
        public bool CustomEditor { get { return false; } }
        public bool Disable { get { return SetDisabled; } set { SetDisabled = value; } }
        [HideInInspector] public bool SetDisabled = false;
        public bool DrawDisableOption { get { return false; } }
        public bool SupportingTransitions { get { return false; } }
        public bool DrawLowererSlider { get { return false; } }
        public float QualityLowerer { get { return 1f; } set { } }
        public string HeaderText { get { return "Rigidbody LOD Settings"; } }
        public int DrawingVersion { get { return 1; } set { } }
        public float ToCullDelay { get { return 0f; } }
        public Texture Icon
        {
            get
            {
                return
#if UNITY_EDITOR
            EditorGUIUtility.IconContent("Rigidbody Icon").image;
#else
        null;
#endif
            }
        }

        public Component TargetComponent { get { return cmp; } }
        [SerializeField] [HideInInspector] private Rigidbody cmp;

        #endregion

        [Space(4)]
        [Tooltip("Switching collision detection for rigidbody")]
        public bool DetectCollisions;
        [Tooltip("Switching kinemtic to make object freezed")]
        public bool IsKinematic;

        [Space(6)]
        public RigidbodyInterpolation Interpolation;
        [Tooltip("Continous and ContinousDynamic have big impact on rigidbodies performance, try to not use them when object is far from camera.\nSpeculative have a bit bigger impact on performance than Discrete.\nDiscrete collision is fastest.")]
        public CollisionDetectionMode CollisionMode;

        [Space(4)]
        [Tooltip("Try forcing rigidbody to Sleep state")]
        public bool TryTriggerSleep;
        [Tooltip("Try forcing rigidbody to go out of Sleep state")]
        public bool TriggerWakeup;

        public void SetSameValuesAsComponent(Component component)
        {
            Rigidbody r = component as Rigidbody;
            if (r == null) return;

            cmp = r;

            // Assigning component's true values to this LOD class instance
            IsKinematic = r.isKinematic;
            DetectCollisions = r.detectCollisions;
            Interpolation = r.interpolation;
            CollisionMode = r.collisionDetectionMode;
        }



        public void ApplySettingsToTheComponent(Component component, ILODInstance initialSettings)
        {
            Rigidbody comp = component as Rigidbody;
            //LODI_Rigidbody initials = initialSettings as LODI_Rigidbody;

            comp.isKinematic = IsKinematic;
            comp.detectCollisions = DetectCollisions;
            comp.interpolation = Interpolation;
            comp.collisionDetectionMode = CollisionMode;

            if (TriggerWakeup)
            {
                if (comp.IsSleeping()) comp.WakeUp();
            }
            else
            if (TryTriggerSleep)
                if (!comp.IsSleeping()) comp.Sleep();
        }


        public void AssignAutoSettingsAsForLODLevel(int lodIndex, int lodCount, Component source)
        {
            Rigidbody comp = source as Rigidbody;
            if (comp == null) Debug.LogError("[OPTIMIZERS] Given component for reference values is null or is not AudioSource Component!");

            DetectCollisions = comp.detectCollisions;
            Interpolation = comp.interpolation;
            CollisionMode = comp.collisionDetectionMode;

            if (lodIndex > 0)
            {
                TryTriggerSleep = true;
                CollisionMode = CollisionDetectionMode.Discrete;
            }

            // Last LOD before culled
            if (lodIndex == lodCount - 2)
            {
                TryTriggerSleep = true;
                CollisionMode = CollisionDetectionMode.Discrete;
            }

        }


        public void AssignSettingsAsForCulled(Component component)
        {
            FLOD.AssignDefaultCulledParams(this);
            DetectCollisions = false;
            IsKinematic = true;
            CollisionMode = CollisionDetectionMode.Discrete;
            TryTriggerSleep = true;
        }


        public void AssignSettingsAsForNearest(Component component)
        {
            FLOD.AssignDefaultNearestParams(this);
            SetSameValuesAsComponent(component);
            TriggerWakeup = true;
        }

        public ILODInstance GetCopy() { return MemberwiseClone() as ILODInstance; }

        public void AssignSettingsAsForHidden(Component component)
        {
            Name = "Hidden";
            TryTriggerSleep = true;
        }

        public void InterpolateBetween(ILODInstance lodA, ILODInstance lodB, float transitionToB)
        { }

        // Custom Inspector Features ---------------------------------------------


#if UNITY_EDITOR
        public void AssignToggler(ILODInstance reference)
        { }

        public void DrawTogglers(SerializedProperty iflodProp)
        { }

        public void CustomEditorWindow(SerializedProperty prop)
        { }


#endif

    }
}