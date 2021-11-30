using System;

#if UNITY_EDITOR
using FIMSpace.FEditor;
using UnityEditor;
using UnityEngine;
#endif

namespace FIMSpace.FOptimizing
{
    public abstract partial class LODsControllerBase
    {

        /// <summary>
        /// Drawing settings to tweak on LODs for the inspector window
        /// </summary>
        public void Editor_DrawValues(int selectedLOD = 0, int index = 0)
        {

            #region Checking for References Correctness


#if UNITY_EDITOR

            if (optimizer == null)
            {
                EditorGUILayout.HelpBox("Optimizer Reference Lost! You probably removed component to optimize from main object or something similar! You have to remove than add optimizer again or remove this component in 'To Optimize' list (" + Component + ")", MessageType.Error);
                return;
            }
            else
            if (!CheckCoreRequirements(true))
            {
                return;
            }
            else if (ReferenceLOD == null)
            {
                EditorGUILayout.HelpBox("Reference LOD Lost!", MessageType.Warning);

                if (GUILayout.Button(new GUIContent("Retry"), new GUILayoutOption[2] { GUILayout.Width(50), GUILayout.Height(15) }))
                {
                    optimizer.RemoveAllComponentsFromToOptimize();
                    optimizer.AssignComponentsToBeOptimizedFromAllChildren(optimizer.gameObject);
                    if (optimizer.GetToOptimizeCount() == 0) optimizer.AssignComponentsToBeOptimizedFromAllChildren(optimizer.gameObject, true);
                }

                return;
            }

            if (selectedLOD < 0 || selectedLOD > GetLODSettingsCount())
            {
                if (ReferenceLOD != null)
                    if (Component != null)
                        Debug.Log("[OPTIMIZERS DEBUG] selected LOD = " + selectedLOD + " not drawing LODs for " + Component.GetType() + ". You can go to 'To Optimize' tab and add components to optimize once again.");
                return;
            }
#endif
            #endregion


#if UNITY_EDITOR

            #region Base frame GUI

            string headerText = " Draw Properties";
            if (editorHeader != "")
            {
                if (Component) headerText = " " + Component.name;
                else headerText = " " + editorHeader;
            }

            Color preCol = GUI.color;

            if (index % 2 == 0)
                EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxLightStyle);
            else
                EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);

            GUI.color = preCol * new Color(1f, 1f, 1f, 0.825f);

            #endregion


            EditorGUI.indentLevel++;
            ILODInstance refLod = ReferenceLOD;


            #region Header box with additional buttons if needed

            if (refLod != null)
                if (refLod.DrawLowererSlider) // With lowerer slider
                {
                    EditorGUILayout.BeginHorizontal();

                    // Properties foldout with additionals
                    EditorGUIUtility.labelWidth = 155;
                    drawProperties = EditorGUILayout.Foldout(drawProperties, new GUIContent(headerText, FLOD.GetIcon(Component, refLod), "Level Of Detail settings for the optimizer's component: " + Component.GetType()), true, new GUIStyle(EditorStyles.foldout) { fontStyle = FontStyle.Bold });
                    if (GUILayout.Button("", EditorStyles.label, GUILayout.Width(80))) drawProperties = !drawProperties;
                    EditorGUIUtility.labelWidth = 0;
                    float preLow = refLod.QualityLowerer;

                    SerializedObject serObj = GetSerializedObject();
                    SerializedProperty prop = GetSerializedLODPropertyFor(selectedLOD);

                    GUILayout.FlexibleSpace();
                    EditorGUIUtility.labelWidth = 65;
                    float newLowerer = EditorGUILayout.FloatField(new GUIContent("Quality", "Changing value of this slider, will change quality of all LODs for this component"), refLod.QualityLowerer);
                    refLod.QualityLowerer = newLowerer;
                    EditorGUIUtility.labelWidth = 0;

                    if (newLowerer > 1f) newLowerer = 1f;
                    if (newLowerer < 0.1f) newLowerer = 0.1f;

                    if (preLow != refLod.QualityLowerer)
                        AutoQualityLowerer(newLowerer);

                    if (serObj != null)
                        serObj.ApplyModifiedProperties();

                    prop.serializedObject.Update();
                    prop.serializedObject.ApplyModifiedProperties();

                    EditorGUILayout.EndHorizontal();
                }
                else // Without lowerer slider
                {
                    if (Component == null)
                    {
                        CheckComponentsCorrectness();
                        GUI.color = preCol;
                        return;
                    }
                    else
                    {
                        EditorGUILayout.BeginHorizontal();

                        // Properties foldout
                        EditorGUIUtility.labelWidth = 185;
                        drawProperties = EditorGUILayout.Foldout(drawProperties, new GUIContent(headerText, FLOD.GetIcon(Component, refLod), "Level Of Detail settings for the optimizer's component: " + Component.GetType()), true, new GUIStyle(EditorStyles.foldout) { fontStyle = FontStyle.Bold });
                        EditorGUIUtility.labelWidth = 0;

                        // Simplify mode for mono behaviour scripts
                        LODI_MonoBehaviour monoLod = ReferenceLOD as LODI_MonoBehaviour;
                        if (monoLod != null)
                        {
                            if (drawProperties)
                            {
                                if (GUILayout.Button("", EditorStyles.label, GUILayout.Width(80))) drawProperties = !drawProperties;
                                GUILayout.FlexibleSpace();

                                GUIContent buttonText;

                                if (monoLod.DrawingVersion == 1 || Version == 1) buttonText = new GUIContent("Get Parameters", "Trying to collect monobehaviour's inspector variables to able to modify them with LOD levels");
                                else buttonText = new GUIContent("Simplify", "Making LOD only for enabling / disabling component in LOD levels");

                                if (GUILayout.Button(buttonText, new GUILayoutOption[2] { GUILayout.Width(Version == 1 ? 102 : 90), GUILayout.Height(18) }))
                                {
                                    if (Version == 1) Version = 0; else Version = 1;


                                    if (Version == 1) // Clearing mono parameters to use only event
                                    {
                                        for (int i = 0; i < GetLODSettingsCount(); i++)
                                        {
                                            LODI_MonoBehaviour mono = GetLODSetting(i) as LODI_MonoBehaviour;

                                            if (mono != null)
                                            {
                                                mono.DrawingVersion = Version;
                                                GetSerializedLODPropertyFor(i).serializedObject.ApplyModifiedProperties();
                                                mono.Simplify();
                                                GetSerializedLODPropertyFor(i).serializedObject.Update();
                                                GetSerializedLODPropertyFor(i).serializedObject.ApplyModifiedProperties();
                                            }
                                        }

                                        Editor_MonoSimplyfy();
                                    }
                                    else // Supporting parameters in LOD set
                                    {

                                        for (int i = 0; i < GetLODSettingsCount(); i++)
                                        {
                                            LODI_MonoBehaviour mono = GetLODSetting(i) as LODI_MonoBehaviour;
                                            if (mono != null)
                                            {
                                                mono.DrawingVersion = Version;
                                                GetSerializedLODPropertyFor(i).serializedObject.ApplyModifiedProperties();
                                                mono.SetSameValuesAsComponent(Component);
                                                GetSerializedLODPropertyFor(i).serializedObject.Update();
                                                GetSerializedLODPropertyFor(i).serializedObject.ApplyModifiedProperties();
                                            }
                                        }


                                        ScriptableLODsController scrLOD = this as ScriptableLODsController;
                                        if (scrLOD != null)
                                        {
                                            // Triggering generation
                                            scrLOD.LODSet.LevelOfDetailSets.RemoveAt(scrLOD.LODSet.LevelOfDetailSets.Count - 1);
                                            return;
                                        }

                                    }

                                    GenerateLODParameters();
                                }
                            }
                        }

                        EditorGUILayout.EndHorizontal();
                    }
                }

            #endregion


            EditorGUI.indentLevel--;
            GUI.color = preCol;


            // Drawing LOD properties
            if (drawProperties)
            {
                ILODInstance lod;

                if (selectedLOD == GetLODSettingsCount())
                    lod = GetHiddenLOD();
                else
                    lod = GetLODSetting(selectedLOD);

                if (lod != null)
                {
                    GUI_LODSettingHeader(lod, selectedLOD);

                    if (selectedLOD == 0 && lockFirstLOD && !optimizer.UnlockFirstLOD) GUI.enabled = false; else GUI.enabled = true;


                    #region Serialized objects check

                    bool canDrawLOD = true;
                    SerializedProperty p = GetSerializedLODPropertyFor(selectedLOD);

                    if (canDrawLOD)
                        if (p == null)
                        {
                            EditorGUILayout.LabelField(new GUIContent("'settings' or other LOD reference property not found!", FGUI_Resources.Tex_Warning));
                            canDrawLOD = false;
                        }

                    #endregion


                    if (canDrawLOD)
                    {
                        // Drawing settings without custom inspector 
                        if (lod.CustomEditor == false)
                        {
                            try
                            {
                                FEditor.FGUI_Inspector.DrawUILine(new Color(0.5f, 0.5f, 0.5f, 0.5f), 2, 4);

                                SerializedObject so = p.serializedObject;
                                //p.serializedObject.Update();

                                GUI_DrawDisableAndTogglers(lod, selectedLOD, p);

                                EditorGUI.BeginChangeCheck();

                                int safeLimit = 0; // Drawing all available public serialized parameters
                                while (p.NextVisible(true))
                                {
                                    if (p == null) break;
                                    if (p.displayName.ToLower().Contains("element")) break;
                                    if (p.displayName.ToLower().Contains("lo d")) break;
                                    EditorGUILayout.PropertyField(p, true);
                                    //Debug.Log(safeLimit + " : " + p.displayName);
                                    if (++safeLimit >= 1000) break;
                                    //if (safeLimit > endp) break;
                                }

                                if (EditorGUI.EndChangeCheck())
                                {
                                    EditorUtility.SetDirty(optimizer);
                                }

                                //so.Update();
                                so.ApplyModifiedProperties(); // Applying changes to serialized container object
                                so.Dispose();
                                //Debug.Log("JO");

                            }
                            catch (Exception e)
                            {
                                Debug.LogWarning("[Optimizers] Probably something went wrong. " + e.ToString());
                            }
                        }
                        else // Drawing LOD settings with custom inspector GUI
                        {
                            try
                            {
                                FEditor.FGUI_Inspector.DrawUILine(new Color(0.5f, 0.5f, 0.5f, 0.5f), 2, 4);

                                GUI_DrawDisableAndTogglers(lod, selectedLOD, p);

                                lod.CustomEditorWindow(p);

                                // Custom editor must do this two lines below
                                //s.ApplyModifiedProperties();
                                //s.Dispose();
                            }
                            catch (Exception e)
                            {
                                Debug.LogWarning("[Optimizers] Probably something went wrong. " + e.ToString());
                            }
                        }

                    }

                }
                else
                {
                    Debug.LogWarning("[Optimizers Editor] LOD is null");
                }

                if (selectedLOD == 0 && lockFirstLOD) GUI.enabled = true;
            }


            GUI.color = preCol;
            EditorGUILayout.EndVertical();

#endif

        }

        /// <summary>
        /// Additional operations to do on scriptable optimizer for clearning MonoBehaviour
        /// </summary>
        protected virtual void Editor_MonoSimplyfy()
        { }


        /// <summary>
        /// When some LOD settings values are changed - for scriptable objects to set LOD Scriptables dirty
        /// </summary>
        protected virtual void Editor_ValuesChanged()
        { }

        /// <summary>
        /// Public switch to hide drawed
        /// </summary>
        public void GUI_HideProperties(bool hideThem)
        {
#if UNITY_EDITOR
            drawProperties = !hideThem;
#endif
        }

#if UNITY_EDITOR

        /// <summary>
        /// Drawing disable parameters for LOD instance or also additional togglers for example for light component
        /// </summary>
        void GUI_DrawDisableAndTogglers(ILODInstance lod, int selected, SerializedProperty p)
        {
            if (lod.DrawDisableOption) // Drawing disable component option if allowed
                if (selected > 0 || optimizer.UnlockFirstLOD)
                    GUI_DrawDisableProp(p, lod, selected);


            if (selected == 0) // Drawing additional elements if enabled
            {
                bool pre = GUI.enabled;
                GUI.enabled = true;
                lod.DrawTogglers(p);
                GUI.enabled = pre;
            }
        }


        /// <summary>
        /// Drawing disable property and additional buttons if some conditions are met
        /// </summary>
        void GUI_DrawDisableProp(SerializedProperty p, ILODInstance iflod, int selectedLOD)
        {
            SerializedProperty isDis = p.FindPropertyRelative("SetDisabled");

            if (isDis != null)
            {
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.PropertyField(isDis, new GUIContent("Disable", "If in selected LOD component (not game object) should be disabled"));

                // Drawing option to draw disable / enable all button in first component settings
                if (ToOptimizeIndex == 0)
                    if (optimizer.GetToOptimizeCount() > 3)
                        if (iflod.Disable)
                        {
                            if (GUILayout.Button(new GUIContent("Set Enabled All", "Setting all components in optimizer to be enabled in this LOD level"), EditorStyles.miniButton, GUILayout.Width(110)))
                                for (int i = 0; i < optimizer.GetToOptimizeCount(); i++) { optimizer.GetLODInstance(i, selectedLOD).Disable = false; }
                        }
                        else
                        {
                            if (GUILayout.Button(new GUIContent("Set Disabled All", "Setting all components in optimizer to be disabled in this LOD level"), EditorStyles.miniButton, GUILayout.Width(110)))
                                for (int i = 0; i < optimizer.GetToOptimizeCount(); i++) { optimizer.GetLODInstance(i, selectedLOD).Disable = true; }
                        }

                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.LabelField("'SetDisabled' variable not found!");
            }
        }
#endif


        /// <summary> Refreshing indexes of LOD instances </summary>
        protected abstract void RefreshToOptimizeIndex();
#if UNITY_EDITOR
        /// <summary> Getting serialized object of optimizer type </summary>
        protected abstract SerializedObject GetSerializedObject();
        /// <summary> Getting serialized property for LOD instance used in different way in scriptable and essential optimizer </summary>
        protected abstract SerializedProperty GetSerializedLODPropertyFor(int lod);
#endif

        /// <summary> Checking if target optimized component is lost </summary>
        public void CheckComponentsCorrectness()
        {
            if (Component == null) optimizer.RemoveToOptimize(this);
        }

        /// <summary> Drawing additional GUI on LOD settings header used for example by scriptable optimizer when drawing shared settings </summary>
        protected virtual void GUI_LODSettingHeader(ILODInstance iflod, int selectedLOD)
        { }

    }
}
