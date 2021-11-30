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
    public sealed class LODI_Light : ILODInstance
    {
        #region Main Settings : Interface Properties

        public int Index { get { return index; } set { index = value; } }
        internal int index = -1;
        public string Name { get { return LODName; } set { LODName = value; } }
        internal string LODName = "";
        public bool CustomEditor { get { return true; } }
        public bool Disable { get { return SetDisabled; } set { SetDisabled = value; } }
        [HideInInspector] public bool SetDisabled = false;
        public bool DrawDisableOption { get { return true; } }
        public bool SupportingTransitions { get { return true; } }
        public bool DrawLowererSlider { get { return false; } }
        public float QualityLowerer { get { return 1f; } set { new System.NotImplementedException(); } }
        public string HeaderText { get { return "Light LOD Settings"; } }
        public float ToCullDelay { get { return 0f; } }
        public int DrawingVersion { get { return 1; } set { new System.NotImplementedException(); } }
        public Texture Icon { get { return
#if UNITY_EDITOR
            EditorGUIUtility.IconContent("Light Icon").image;
#else
        null;
#endif
            } }

        public Component TargetComponent { get { return cmp; } }
        [SerializeField] [HideInInspector] private Light cmp;

        #endregion

        [Space(4)]
        [FPD_Suffix(0f, 1f)]
        [Tooltip("Percentage value of light intensity for LOD level (percentage of initial light intensity)")]
        public float IntensityMul = 1f;
        [FPD_Suffix(0f, 1f)]
        [Tooltip("Percentage value of light range for LOD level (percentage of initial light range)")]
        public float RangeMul = 1f;

        [Space(3)]
        public LightShadows ShadowsMode = LightShadows.Soft;
        [FPD_Suffix(0f, 1f)]
        [Tooltip("Percentage value of shadows intensity for LOD level (percentage of initial shadow value)")]
        public float ShadowsStrength = 1f;
        public EOptLightMode RenderMode = EOptLightMode.Auto;

        public enum EOptLightMode : int
        {
            Auto = 0,
            Important = 1,
            NotImportant = 2
        }

        [HideInInspector]
        [Tooltip("If component should change intensity and range of light component (disable if you using flickering or something)")]
        public bool ChangeIntensity = true;



        public void SetSameValuesAsComponent(Component component)
        {
            Light l = component as Light;
            if (l == null) return;

            cmp = l;

            // Assigning component's true values to this LOD class instance
            IntensityMul = l.intensity;
            RangeMul = l.range;
            ShadowsMode = l.shadows;
            ShadowsStrength = l.shadowStrength;
            RenderMode = (EOptLightMode)l.renderMode;
        }


        public void InterpolateBetween(ILODInstance aa, ILODInstance bb, float transitionToB)
        {
            FLOD.DoBaseInterpolation(this, aa, bb, transitionToB);

            LODI_Light a = aa as LODI_Light; // Cast to FLOD_Light is like 10x faster than Random.Range ;)
            LODI_Light b = bb as LODI_Light;

            if (ChangeIntensity)
            {
                IntensityMul = Mathf.Lerp(a.IntensityMul, b.IntensityMul, transitionToB);
                RangeMul = Mathf.Lerp(a.RangeMul, b.RangeMul, transitionToB);
            }

            if (b.ShadowsMode == LightShadows.None) b.ShadowsStrength = 0f;

            ShadowsStrength = Mathf.Lerp(a.ShadowsStrength, b.ShadowsStrength, transitionToB);

            #region Toggling bools and enums

            if (b.ShadowsStrength > 0)
            {
                if (a.ShadowsMode == LightShadows.None)
                {
                    if (transitionToB >= 1)
                    {
                        RenderMode = b.RenderMode;
                    }
                }

                ShadowsMode = b.ShadowsMode;
            }

            if ((int)RenderMode == (int)LightRenderMode.ForcePixel)
            {
                if (transitionToB >= 1)
                    RenderMode = b.RenderMode;
            }
            else
            if ((int)b.RenderMode == (int)LightRenderMode.ForcePixel || (int)b.RenderMode == (int)LightRenderMode.Auto)
            {
                RenderMode = b.RenderMode;
            }

            if (transitionToB >= 1)
            {
                ShadowsMode = b.ShadowsMode;
                RenderMode = b.RenderMode;
            }
            else if (transitionToB <= 0f)
            {
                ShadowsMode = a.ShadowsMode;
                RenderMode = a.RenderMode;
            }

            #endregion
        }


        public void ApplySettingsToTheComponent(Component component, ILODInstance initialSettings)
        {
            // Casting LOD to correct type and checking if it's right
            LODI_Light initials = initialSettings as LODI_Light;
            Light comp = component as Light;
            if (initials == null || comp == null) { Debug.Log("[OPTIMIZERS] Target LOD is not LightLOD or is null"); return; }

            // Setting new settings to optimized component
            if (ChangeIntensity)
            {
                comp.intensity = IntensityMul * initials.IntensityMul;
                comp.range = RangeMul * initials.RangeMul;
            }

            comp.shadowStrength = ShadowsStrength * initials.ShadowsStrength;

            comp.shadows = ShadowsMode;
            comp.renderMode = (LightRenderMode)RenderMode;

            if (Disable) comp.enabled = false; else comp.enabled = true;
        }


        public void AssignAutoSettingsAsForLODLevel(int lodIndex, int lodCount, Component component)
        {
            Light lSource = component as Light;
            if (lSource == null) Debug.LogError("[OPTIMIZERS] Given component for reference values is null or is not Light Component!");

            // REMEMBER: LOD = 0 is not nearest but one after nearest
            // Trying to auto configure universal LOD settings

            float mul = FLOD.GetValueForLODLevel(1f, 0f, lodIndex - 2, lodCount);

            //name = "LOD" + (lodIndex + 2); // + 2 to view it in more responsive way for user inside inspector window

            if (lodIndex > 2 && lodCount > 2)
            {
                //IntensityMul = mul;
                RangeMul = mul;
                ShadowsStrength = mul;
            }

            ShadowsMode = lSource.shadows;
            RenderMode = (EOptLightMode)lSource.renderMode;

            if (lodCount == 2) if (lSource.shadows == LightShadows.Soft) ShadowsMode = LightShadows.Hard;
            if (lodCount > 2) if (lSource.shadows == LightShadows.Soft) ShadowsMode = LightShadows.Hard;

            if (lSource.renderMode == LightRenderMode.ForcePixel) RenderMode = EOptLightMode.Auto;

            if (lodIndex > 0) if (lSource.renderMode == LightRenderMode.ForcePixel) RenderMode = EOptLightMode.Auto;


            //if (lodCount > 2) if (lodIndex > 0) RenderMode = LightRenderMode.ForceVertex;
            if (lodIndex >= lodCount - 2 && lodCount > 2) { ShadowsMode = LightShadows.None; /*RenderMode = LightRenderMode.ForceVertex;*/ ShadowsStrength = 0f; }
            if (lodIndex >= 1 && lodCount == 3) RenderMode = EOptLightMode.NotImportant;
            if (lodIndex >= 2) RenderMode = EOptLightMode.NotImportant;

            if (RenderMode == EOptLightMode.NotImportant)
            {
                IntensityMul = 0.4f;
                RangeMul = 0.5f;
            }
        }


        public void AssignSettingsAsForCulled(Component component)
        {
            FLOD.AssignDefaultCulledParams(this);
            IntensityMul = 0f;
            RangeMul = 0f;
            ShadowsStrength = 0f;
            ShadowsMode = LightShadows.None;
            RenderMode = EOptLightMode.NotImportant;
        }


        public void AssignSettingsAsForNearest(Component component)
        {
            FLOD.AssignDefaultNearestParams(this);
            Light light = component as Light;
            ShadowsMode = light.shadows;
            RenderMode = (EOptLightMode)light.renderMode;
        }


        public void AssignSettingsAsForHidden(Component componentnent)
        {
            FLOD.AssignDefaultHiddenParams(this);
        }


        public ILODInstance GetCopy() { return MemberwiseClone() as ILODInstance; }



        // Custom Inspector Features ---------------------------------------------

#if UNITY_EDITOR

        public void AssignToggler(ILODInstance reference)
        {
            LODI_Light l = reference as LODI_Light;
            if (reference != null) ChangeIntensity = l.ChangeIntensity;
        }

        public void DrawTogglers(SerializedProperty prop)
        {
            UnityEditor.EditorGUILayout.BeginVertical(FEditor.FGUI_Resources.BGInBoxLightStyle);

            EditorGUILayout.PropertyField(prop.FindPropertyRelative("ChangeIntensity"));
            //s.ApplyModifiedProperties();
            //s.Dispose();

            UnityEditor.EditorGUILayout.EndVertical();
        }

        public void CustomEditorWindow(SerializedProperty prop)
        {
            bool pre = GUI.enabled;
            if (!ChangeIntensity) GUI.enabled = false;

            if (prop != null)
            {
                int safeLimit = 0;
                if (!ChangeIntensity) // Ignoring intensity variables drawing
                {
                    prop.NextVisible(true);
                    prop.NextVisible(true);
                }

                GUI.enabled = pre;

                while (prop.NextVisible(true))
                {
                    if (prop.displayName.ToLower().Contains("element")) break;
                    if (prop.displayName.ToLower().Contains("lo d")) break;

                    EditorGUILayout.PropertyField(prop, true);
                    if (++safeLimit > 1000) break;
                }
            }

            if (prop.serializedObject != null)
            {
                prop.serializedObject.ApplyModifiedProperties();
                prop.serializedObject.Dispose();
            }
        }

#endif

    }
}