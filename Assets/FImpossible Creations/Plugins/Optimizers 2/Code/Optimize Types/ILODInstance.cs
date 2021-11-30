using UnityEngine;

namespace FIMSpace.FOptimizing
{
    /// <summary>
    /// FC: Complex interface model for LOD settings containers for different types of components
    /// </summary>
    public interface ILODInstance
    {
        // Propierties --------------------------------------------------------------

        /// <summary> Index of LOD Settings Instance </summary>
        int Index { get; set; }
        /// <summary> Name for editor window purpose or other custom purposes </summary>
        string Name { get; set; }
        /// <summary> If FLOD instance is using custom inspector window code </summary>
        bool CustomEditor { get; }
        /// <summary> Main setting for all FLODs to disable component (not GameObject) through LODs </summary>
        bool Disable { get; set; }
        /// <summary> If component disable button should be visible inside inspector window </summary>
        bool DrawDisableOption { get; }
        /// <summary> IF transitioning settings is supported, for exampel bools/enums can't be transitioned but ints, floats etc. can </summary>
        bool SupportingTransitions { get; }
        /// <summary> For drawing slider for option down below </summary>
        bool DrawLowererSlider { get; }
        /// <summary> For components like particle system for easier editing </summary>
        float QualityLowerer { get; set; }
        /// <summary> Simply preventively display name for inspector window if name would be empty </summary>
        string HeaderText { get; }
        /// <summary> When using transitions, time to wait after transition time then cull it completely (for example time for particles to disappear after emmission) </summary>
        float ToCullDelay { get; }
        /// <summary> Can be used for custom scripting (see FLOD_MonoBehaviour simple and advanced view) </summary>
        int DrawingVersion { get; set; }
        /// <summary> Optional icon texture which will be displayed next to component name in LOD settings tab </summary>
        Texture Icon { get; }



        // Methods --------------------------------------------------------------


        /// <summary> Used in editor mode </summary>
        void SetSameValuesAsComponent(Component component);
        /// <summary> Used in runtime </summary>
        void ApplySettingsToTheComponent(Component component, ILODInstance initialSettingsReference);


        /// <summary> Used in editor mode </summary>
        void AssignAutoSettingsAsForLODLevel(int lodIndex, int lodCount, Component source);
        /// <summary> Used in editor mode </summary>
        void AssignSettingsAsForCulled(Component component);
        /// <summary> Used in editor mode </summary>
        void AssignSettingsAsForNearest(Component component);
        /// <summary> Used in editor mode </summary>
        void AssignSettingsAsForHidden(Component component);


        /// <summary> Used in transitioning runtime </summary>
        ILODInstance GetCopy();
        /// <summary> Used in transitioning runtime </summary>
        void InterpolateBetween(ILODInstance lodA, ILODInstance lodB, float transitionToB);



        // Editor GUI Methods --------------------------------------------------------------

#if UNITY_EDITOR
        /// <summary> Used for custom inspector window </summary>
        void AssignToggler(ILODInstance reference);

        /// <summary> Used for custom inspector window </summary>
        void DrawTogglers(UnityEditor.SerializedProperty iflodProp);
        /// <summary> Used for custom inspector window </summary>
        void CustomEditorWindow(UnityEditor.SerializedProperty iflodProp);
#endif

    }


    /// <summary>
    /// FC: Class with helper methods for FLODs
    /// </summary>
    public static class FLOD
    {
        /// <summary> Setting name to 'Nearest' </summary>
        public static void AssignDefaultNearestParams(ILODInstance flod)
        {
            flod.Name = "Nearest";
        }

        /// <summary> Setting name to 'Culled' and setting 'Disable' to true </summary>
        public static void AssignDefaultCulledParams(ILODInstance flod)
        {
            flod.Disable = true;
            flod.Name = "Culled";
        }

        /// <summary> Setting name to 'Hidden' and setting 'Disable' to true </summary>
        public static void AssignDefaultHiddenParams(ILODInstance flod)
        {
            flod.Disable = true;
            flod.Name = "Hidden";
        }


        /// <summary> Setting name to 'Hidden' and setting 'Disable' to true </summary>
        public static void ApplyEnableDisableState(ILODInstance flod, Component component)
        {
            Behaviour b = component as Behaviour;
            if (b != null) b.enabled = !flod.Disable;
        }

        /// <summary> Interpolating 'Disable parameter </summary>
        public static void DoBaseInterpolation(ILODInstance current, ILODInstance lodA, ILODInstance lodB, float transitionToB)
        {
            current.Disable = BoolTransition(current.Disable, lodA.Disable, lodB.Disable, transitionToB);
        }

        /// <summary>
        /// Returning lerp value for lod level with reference of lod levels count
        /// Higher level then lower value, lower LOD level then higher value (from 0 to 1)
        /// </summary>
        public static float GetValueForLODLevel(float from, float to, float lodLevel, float lodLevels)
        {
            return Mathf.Lerp(from, to, (lodLevel + 1f) / lodLevels);
        }


        /// <summary>
        /// Helping transitioning not lerpable values
        /// </summary>
        public static bool BoolTransition(bool defaultV, bool a, bool b, float transition)
        {
            if (b == false && a == true) // Transitioning from culling to new LOD
            {
                return false;
            }
            else
            {
                if (transition >= 1f) return b;
                else
                if (transition <= 0f) return a;
            }

            return defaultV;
        }


        /// <summary>
        /// Helping transitioning not lerpable values
        /// </summary>
        public static object ObjectTransition(object defaultV, object a, object b, float transition)
        {
            if (transition >= 1f) return b;
            else
            if (transition <= 0f) return a;

            return defaultV;
        }

        /// <summary>
        /// Getting icon context for IFLOD's optimized component
        /// </summary>
        public static Texture GetIcon(Component comp, ILODInstance lod)
        {
            if (lod.Icon != null) return lod.Icon;

            if (comp == null)
                return null;
            else
#if UNITY_EDITOR
                return UnityEditor.EditorGUIUtility.ObjectContent(comp, comp.GetType()).image;
#else
            return null;
#endif
        }
    }
}