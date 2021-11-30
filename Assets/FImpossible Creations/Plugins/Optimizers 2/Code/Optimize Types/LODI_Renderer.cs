using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.FOptimizing
{
    /// <summary>
    /// FM: Helper class for single LOD level settings on Renderer
    /// </summary>
    [System.Serializable]
    public sealed class LODI_Renderer : ILODInstance
    {
        #region Main Settings : Interface Properties

        public int Index { get { return index; } set {  index = value; } }
        internal int index = -1;
        public string Name { get { return LODName; } set {  LODName = value; } }
        internal string LODName = "";
        public bool CustomEditor { get { return false; } }
        public bool Disable { get { return SetDisabled; } set {  SetDisabled = value; } }
        [HideInInspector] public bool SetDisabled = false;
        public bool DrawDisableOption { get { return true; } }
        public bool SupportingTransitions { get { return false; } }
        public bool DrawLowererSlider { get { return false; } }
        public float QualityLowerer { get { return 1f; } set {  new System.NotImplementedException(); } }
        public string HeaderText { get { return "Renderer LOD Settings"; } }
        public float ToCullDelay { get { return 0f; } }
        public int DrawingVersion { get { return 1; } set {  new System.NotImplementedException(); } }
        public Texture Icon { get { return
#if UNITY_EDITOR
            EditorGUIUtility.IconContent("SkinnedMeshRenderer Icon").image;
#else
        null;
#endif
            } }

        public Component TargetComponent { get { return cmp; } }
        [SerializeField] [HideInInspector] private Renderer cmp;

        #endregion

        [Space(4)]
        [Tooltip("If model should cast and receive shadows (receive will be always false if renderer have it marked as false by default)")]
        public bool UseShadows = true;
        internal UnityEngine.Rendering.ShadowCastingMode ShadowsCast = UnityEngine.Rendering.ShadowCastingMode.On;
        internal bool ShadowsReceive;

        public MotionVectorGenerationMode MotionVectors = MotionVectorGenerationMode.Object;

        [Tooltip("If it is skinned mesh renderer we can switch bones weights spread quality")]
        public SkinQuality SkinnedQuality = SkinQuality.Auto;






        public void SetSameValuesAsComponent(Component component)
        {
            if (component == null) Debug.LogError("[OPTIMIZERS] Given component is null instead of Renderer!");

            Renderer comp = component as Renderer;

            if (comp != null)
            {
                cmp = comp;

                UseShadows = true;
                if (comp.shadowCastingMode == UnityEngine.Rendering.ShadowCastingMode.Off) UseShadows = false;

                ShadowsCast = comp.shadowCastingMode;
                ShadowsReceive = comp.receiveShadows;
                MotionVectors = comp.motionVectorGenerationMode;

                SkinnedMeshRenderer skin = component as SkinnedMeshRenderer;
                if (skin) SkinnedQuality = skin.quality;
            }
        }


        public void ApplySettingsToTheComponent(Component component, ILODInstance initialSettingsRef)
        {
            LODI_Renderer initialSettings = initialSettingsRef as LODI_Renderer;

            #region Security

            if (component == null) { Debug.Log("[OPTIMIZERS] Target component is null"); return; }
            if (initialSettings == null) { Debug.Log("[OPTIMIZERS] Target LOD is not Renderer LOD or is null"); return; }

            #endregion

            Renderer comp = component as Renderer;

            if (UseShadows)
            {
                comp.shadowCastingMode = initialSettings.ShadowsCast;
                comp.receiveShadows = initialSettings.ShadowsReceive;
            }
            else
            {
                comp.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                comp.receiveShadows = false;
            }

            comp.motionVectorGenerationMode = MotionVectors;


#if UNITY_2019_1_OR_NEWER
            if (QualitySettings.skinWeights != SkinWeights.OneBone)
                if (comp is SkinnedMeshRenderer)
                {
                    if (QualitySettings.skinWeights == SkinWeights.TwoBones)
                    {
                        if (SkinnedQuality == SkinQuality.Bone4) SkinnedQuality = SkinQuality.Bone2;
                    }

                    SkinnedMeshRenderer skin = comp as SkinnedMeshRenderer;
                    skin.quality = SkinnedQuality;
                }
#else
            if (QualitySettings.blendWeights != BlendWeights.OneBone)
                if (comp is SkinnedMeshRenderer)
                {
                    if (QualitySettings.blendWeights == BlendWeights.TwoBones)
                    {
                        if (SkinnedQuality == SkinQuality.Bone4) SkinnedQuality = SkinQuality.Bone2;
                    }

                    SkinnedMeshRenderer skin = comp as SkinnedMeshRenderer;
                    skin.quality = SkinnedQuality;
                }
#endif


            if (Disable) comp.enabled = false; else comp.enabled = true;
        }


        public void AssignAutoSettingsAsForLODLevel(int lodIndex, int lodCount, Component component)
        {
            Renderer comp = component as Renderer;
            if (comp == null) Debug.LogError("[OPTIMIZERS] Given component for reference values is null or is not Renderer Component!");

            float mul = FLOD.GetValueForLODLevel(1f, 0f, lodIndex, lodCount);
            UseShadows = !(comp.shadowCastingMode == UnityEngine.Rendering.ShadowCastingMode.Off);

            if (lodIndex >= 0)
            {
                if (comp.motionVectorGenerationMode != MotionVectorGenerationMode.ForceNoMotion)
                    MotionVectors = MotionVectorGenerationMode.Camera;
            }

            if (lodCount == 2) if (comp.motionVectorGenerationMode == MotionVectorGenerationMode.Object) MotionVectors = MotionVectorGenerationMode.Camera;

            //if (mul > 0.43f) SkinnedQuality = SkinQuality.Bone2;
            //if (lodCount > 4)
            //    if (mul > 0.6f) SkinnedQuality = SkinQuality.Bone1;

            SkinnedMeshRenderer sk = comp as SkinnedMeshRenderer;
            SkinQuality defQ = SkinQuality.Auto; if (sk) defQ = sk.quality;

            if (mul < 0.6f) SkinnedQuality = defQ == SkinQuality.Bone4 ? SkinQuality.Bone2 : SkinQuality.Auto;
            if (mul < 0.4f) SkinnedQuality = SkinQuality.Bone1;
            if (mul < 0.55f) UseShadows = false;

            if (lodIndex == lodCount - 2)
            {
                UseShadows = false;
                if (lodCount != 2) MotionVectors = MotionVectorGenerationMode.ForceNoMotion;
                SkinnedQuality = SkinQuality.Bone1;
            }

            //name = "LOD" + (lodIndex + 2); // + 2 to view it in more responsive way for user inside inspector window
        }


        public void AssignSettingsAsForCulled(Component component)
        {
            FLOD.AssignDefaultCulledParams(this);
            UseShadows = false;
            MotionVectors = MotionVectorGenerationMode.ForceNoMotion;
            SkinnedQuality = SkinQuality.Bone1;
        }

        public void AssignSettingsAsForNearest(Component component)
        {
            FLOD.AssignDefaultNearestParams(this);
        }

        public void AssignSettingsAsForHidden(Component component)
        {
            FLOD.AssignDefaultHiddenParams(this);
            UseShadows = false;
            MotionVectors = MotionVectorGenerationMode.ForceNoMotion;
            SkinnedQuality = SkinQuality.Bone1;
        }


        public ILODInstance GetCopy() { return MemberwiseClone() as ILODInstance; }

        public void InterpolateBetween(ILODInstance a, ILODInstance b, float transitionToB)
        { FLOD.DoBaseInterpolation(this, a, b, transitionToB); }

#if UNITY_EDITOR
        public void AssignToggler(ILODInstance reference)
        { }

        public void DrawTogglers(SerializedProperty iflodProp)
        { }

        public void CustomEditorWindow(SerializedProperty iflodProp)
        { }
#endif

    }
}
