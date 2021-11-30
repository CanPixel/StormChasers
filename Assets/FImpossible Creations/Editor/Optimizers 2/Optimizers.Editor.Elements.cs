using FIMSpace.FEditor;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.FOptimizing
{
    public partial class Optimizer_BaseEditor
    {
        private bool drawHeaderFoldout = false;
        private void HeaderBoxMain(GUIContent title, ref bool drawGizmos, ref bool defaultInspector, Texture2D scrIcon, MonoBehaviour target, int height = 22)
        {
            EditorGUILayout.BeginVertical(FGUI_Resources.HeaderBoxStyle);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(new GUIContent(scrIcon), EditorStyles.label, new GUILayoutOption[2] { GUILayout.Width(height - 2), GUILayout.Height(height - 2) }))
            {
                MonoScript script = MonoScript.FromMonoBehaviour(target);
                if (script) EditorGUIUtility.PingObject(script);
                drawHeaderFoldout = !drawHeaderFoldout;
            }

            if (GUILayout.Button(title, FGUI_Resources.GetTextStyle(14, true, TextAnchor.MiddleLeft), GUILayout.Height(height)))
            {
                MonoScript script = MonoScript.FromMonoBehaviour(target);
                if (script) EditorGUIUtility.PingObject(script);
                drawHeaderFoldout = !drawHeaderFoldout;
            }

            if (EditorGUIUtility.currentViewWidth > 326)
                // Youtube channel button
                if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Tutorials, "Open FImpossible Creations Channel with tutorial videos in your web browser"), FGUI_Resources.ButtonStyle, new GUILayoutOption[2] { GUILayout.Width(height), GUILayout.Height(height) }))
                {
                    Application.OpenURL("https://www.youtube.com/c/FImpossibleCreations");
                }

            if (EditorGUIUtility.currentViewWidth > 292)
                // Store site button
                if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Website, "Open FImpossible Creations Asset Store Page inside your web browser"), FGUI_Resources.ButtonStyle, new GUILayoutOption[2] { GUILayout.Width(height), GUILayout.Height(height) }))
                {
                    Application.OpenURL("https://assetstore.unity.com/publishers/37262");
                }

            // Manual file button
            if (_manualFile == null)
            {
                string manualPath = AssetDatabase.GetAssetPath(MonoScript.FromMonoBehaviour(target));
                manualPath = manualPath.Replace("Components/", "");
                manualPath = Path.GetDirectoryName(manualPath);
                _manualFile = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(manualPath + "/Optimizers - User Manual.pdf");
            }

            if (_manualFile)
                if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Manual, "Open .PDF user manual file for Optimizers"), FGUI_Resources.ButtonStyle, new GUILayoutOption[2] { GUILayout.Width(height), GUILayout.Height(height) }))
                {
                    EditorGUIUtility.PingObject(_manualFile);
                    Application.OpenURL(Application.dataPath + "/" + AssetDatabase.GetAssetPath(_manualFile).Replace("Assets/", ""));
                }

            FGUI_Inspector.DrawSwitchButton(ref drawGizmos, FGUI_Resources.Tex_GizmosOff, FGUI_Resources.Tex_Gizmos, "Toggle drawing LOD distance spheres gizmos in scene window", height, height, true);
            FGUI_Inspector.DrawSwitchButton(ref drawHeaderFoldout, FGUI_Resources.Tex_LeftFold, FGUI_Resources.Tex_DownFold, "Toggle to view additional options for foldouts", height, height);

            EditorGUILayout.EndHorizontal();

            if (drawHeaderFoldout)
            {
                FGUI_Inspector.DrawUILine(0.07f, 0.1f, 1, 4, 0.99f);

                EditorGUILayout.BeginHorizontal();

                EditorGUI.BeginChangeCheck();
                choosedLang = (ELangs)EditorGUILayout.EnumPopup(choosedLang, new GUIStyle(EditorStyles.layerMaskField) { fixedHeight = 0 }, new GUILayoutOption[2] { GUILayout.Width(80), GUILayout.Height(22) });
                if (EditorGUI.EndChangeCheck())
                {
                    PlayerPrefs.SetInt("FimposLang", (int)choosedLang);
                    SetupLangs();
                }

                GUILayout.FlexibleSpace();

                bool hierSwitchOn = PlayerPrefs.GetInt("OptH", 1) == 1;
                FGUI_Inspector.DrawSwitchButton(ref hierSwitchOn, FGUI_Resources.Tex_HierSwitch, null, "Switch drawing small icons in hierarchy", height, height, true);
                PlayerPrefs.SetInt("OptH", hierSwitchOn ? 1 : 0);

                // Default inspector switch
                FGUI_Inspector.DrawSwitchButton(ref drawDefaultInspector, FGUI_Resources.Tex_Default, null, "Switch GUI Style to default inspector", height, height, true);
                if (!drawDefaultInspector && drawDefaultInspector) drawDefaultInspector = false;


                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }


        protected virtual void PreLODGUI()
        {

        }

        private void DrawLODRangesGUI()
        {
            EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxBlankStyle);
            GUIStyle boldCenter = new GUIStyle(EditorStyles.boldLabel);
            boldCenter.alignment = TextAnchor.MiddleCenter;
            GUILayout.Space(2);

            if (Application.isPlaying) GUI.enabled = false;

            El_DrawMaxDistance();

            // Drawing default LOD Levels slider 
            if (Get.LimitLODLevels == 0 || Get.LimitLODLevels < 2 || Get.LimitLODLevels > 7)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(sp_LodLevels, true);

                if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Refresh), FGUI_Resources.ButtonStyle, new GUILayoutOption[] { GUILayout.Width(20), GUILayout.Height(18) }))
                {
                    int levels = Get.LODLevels;
                    Get.LODLevels += 1;
                    Get.OnValidate();
                    Get.LODLevels = levels;
                    Get.OnValidate();
                    EditorUtility.SetDirty(Get);
                }

                EditorGUILayout.EndHorizontal();
            }
            else // Drawing LOD levels property with limited LOD levels
            {
                int pre = Get.LODLevels;
                EditorGUILayout.BeginHorizontal();
                Get.LODLevels = EditorGUILayout.IntSlider(new GUIContent("LOD Levels", "Level of detail (LOD) steps to configure optimization levels"), Get.LODLevels, 1, Get.LimitLODLevels);
                if (Get.LODLevels != pre) Get.OnValidate();

                if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Refresh), FGUI_Resources.ButtonStyle, new GUILayoutOption[] { GUILayout.Width(20), GUILayout.Height(18) }))
                {
                    int levels = Get.LODLevels;
                    Get.LODLevels += 1;
                    Get.OnValidate();
                    Get.LODLevels = levels;
                    Get.OnValidate();
                    EditorUtility.SetDirty(Get);
                }

                EditorGUILayout.EndHorizontal();
            }

            GUI.enabled = true;
            GUILayout.Space(2);
            DrawFadeDurationSlider(Get);
            GUILayout.Space(2);

            //GUI.color = individualColor;

            if (Application.isPlaying) GUI.enabled = false;
            PreLODGUI();

            GUI.enabled = true;
            GUI.color = preCol;
            GUILayout.Space(5);

            if (Get != null) // Unity 2020 is destroying reference somehow, what a nonsense
            {
                if (Get.enabled == false && Application.isPlaying)
                {
                    EditorGUILayout.HelpBox("Optimizer is disabled so I can't draw more here.", MessageType.Info);
                }
                else
                {

                    if (Application.isPlaying)
                    {
                        GUI.color = new Color(1f, 1f, 1f, 0.8f);
                        GUILayout.Space(2);

                        string lodNameTxt = "Active";
                        if (Get.IsCulled) lodNameTxt = "Culled";
                        else
                        {
                            if (Get.CurrentLODLevel < Get.LODLevels)
                                lodNameTxt = GetLODName(Get.CurrentLODLevel, Get.LODLevels);
                            else
                                lodNameTxt = "Hide";
                        }

                        string dist = "";

                        if (Get.TargetCamera != null)
                            if (Get.GetReferenceDistance() != 0f)
                            {
                                distance = Get.GetReferenceDistance();
                                dist = "Distance: " + System.Math.Round(distance, 1) + " ";
                            }

                        string transition = "";
                        if (Get.TransitionPercent > 0f)
                        {
                            transition = " Transition: " + Mathf.Round(Mathf.Min(Get.TransitionPercent, 1f) * 100f) + "%" + (Get.TransitionPercent > 1.1f ? " (Add Delay " + (Get.TransitionPercent - 1f) + ")" : "");
                        }

                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Current LOD: " + lodNameTxt + transition, new GUIStyle(EditorStyles.boldLabel));
                        GUILayout.FlexibleSpace();
                        GUILayout.Label(dist, new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleRight });
                        GUILayout.EndHorizontal();

                        if (Get.OutOfCameraView && !Get.FarAway) GUILayout.Label("Camera Looking Away", new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter });

                        GUILayout.Space(4);
                        GUI.color = preCol;
                    }


                    DrawLODSettingsStack(Get);


                    Get.Gizmos_SelectLOD(selectedLOD);

                }
            }

            EditorGUILayout.EndVertical();
        }



        /// <summary>
        /// Searching through component's owner to find head or neck bone
        /// </summary>
        private void SuggestDistanceMenu()
        {
            GenericMenu optionsMenu = new GenericMenu();
            Optimizer_Base opt = Get;
            SerializedObject ser = serializedObject;
            GUIContent title;

            title = new GUIContent("Very Near (disappearing noticable)");
            optionsMenu.AddItem(title, false, () => { opt.SetAutoDistance(0.075f); if (ser != null) ser.ApplyModifiedProperties(); });

            title = new GUIContent("Near (disappearing noticable)");
            optionsMenu.AddItem(title, false, () => { opt.SetAutoDistance(0.165f); if (ser != null) ser.ApplyModifiedProperties(); reloadGUI = true; });

            title = new GUIContent("Mid Far (common)");
            optionsMenu.AddItem(title, false, () => { opt.SetAutoDistance(0.365f); if (ser != null) ser.ApplyModifiedProperties(); });

            title = new GUIContent("Far (less common)");
            optionsMenu.AddItem(title, false, () => { opt.SetAutoDistance(0.625f); if (ser != null) ser.ApplyModifiedProperties(); });

            title = new GUIContent("Very Far - Not Visible (in most cases too far)");
            optionsMenu.AddItem(title, false, () => { opt.SetAutoDistance(1f); if (ser != null) ser.ApplyModifiedProperties(); });

            optionsMenu.ShowAsContext();
        }

        bool reloadGUI = false;

        private void El_DrawMaxDistance()
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUIUtility.labelWidth = 100;
            EditorGUILayout.PropertyField(sp_MaxDist);
            EditorGUIUtility.labelWidth = 0;

            if (!Application.isPlaying)
            {
                if (Get.AutoDistanceFactor <= 0f)
                {
                    Get.AutoDistanceFactor = 0f;

                    if (GUILayout.Button(new GUIContent(Lang("Suggest"), "Opening menu with suggested distances values.\n\nBasing on detection shape size algorithm will try to set max distance to value where object would be so small on screen it would be almost not visible."), new GUILayoutOption[] { GUILayout.Width(68), GUILayout.Height(17) }))
                    {
                        SuggestDistanceMenu();
                    }

                    if (reloadGUI)
                    {
                        reloadGUI = false;
                        serializedObject.ApplyModifiedProperties();
                    }

                    GUI.color = new Color(1f, 1f, 1f, 0.75f);
                    EditorGUILayout.LabelField("", GUILayout.Width(2));
                    EditorGUIUtility.labelWidth = 40; EditorGUIUtility.fieldWidth = 10;
                    EditorGUILayout.PropertyField(sp_AutoDistanceFactor, new GUIContent("Auto", sp_AutoDistanceFactor.tooltip), GUILayout.Width(60));
                    GUI.color = c; EditorGUIUtility.labelWidth = 0; EditorGUIUtility.fieldWidth = 0;
                }
                else
                {
                    if (Get.AutoDistanceFactor > 1.5f) Get.AutoDistanceFactor = 1.5f;

                    EditorGUILayout.LabelField("", GUILayout.Width(2));
                    EditorGUIUtility.labelWidth = 40; EditorGUIUtility.fieldWidth = 34;
                    EditorGUILayout.PropertyField(sp_AutoDistanceFactor, new GUIContent("Auto", sp_AutoDistanceFactor.tooltip), GUILayout.Width(76));
                    GUI.color = c; EditorGUIUtility.labelWidth = 0; EditorGUIUtility.fieldWidth = 0;
                    EditorGUILayout.LabelField("", GUILayout.Width(2));

                    Get.SetAutoDistance(Get.AutoDistanceFactor);
                }
            }

            EditorGUILayout.EndHorizontal();
        }
    }

}

