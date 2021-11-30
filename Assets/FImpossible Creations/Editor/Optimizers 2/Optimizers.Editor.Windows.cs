using FIMSpace.FEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.FOptimizing
{

    public partial class Optimizer_BaseEditor
    {
        public void LODFrame(Optimizer_Base Get)
        {
            if (Application.isPlaying)
            {
                if (Get.IsCulled)
                    EditorGUILayout.BeginVertical(FEditor.FGUI_Inspector.Style(Optimizers_LODGUI.culledLODColor * new Color(1.5f, 1.5f, 1.5f, 0.325f)));
                else
                {
                    if (Get.TransitionPercent <= 0f)
                        EditorGUILayout.BeginVertical(FEditor.FGUI_Inspector.Style(Optimizers_LODGUI.lODColors[Get.CurrentLODLevel] * new Color(1f, 1f, 1f, 0.2f * (Get.OutOfCameraView ? 0.5f : 1f))));
                    else
                    {
                        Color c = Color.Lerp(Optimizers_LODGUI.lODColors[Get.CurrentLODLevel], Optimizers_LODGUI.lODColors[Get.TransitionNextLOD], Get.TransitionPercent);
                        c *= new Color(1f, 1f, 1f, 0.2f * (Get.OutOfCameraView ? 0.5f : 1f));
                        EditorGUILayout.BeginVertical(FEditor.FGUI_Inspector.Style(c));
                    }
                }
            }

            serializedObject.Update();
        }


        protected virtual void DrawFadeDurationSlider(Optimizer_Base Get)
        {
            GUI.color = c;

            if (Get.FadeDuration <= 0f)
            {
                bool transitions = false;
                transitions = EditorGUILayout.Toggle(new GUIContent("Use Transitioning", "If you want changing LOD levels to be smooth changed in time \n\nLOD class of component needs to support transitioning, otherwise change will be done immediately anyway.\n\n(looking away/hiding will do it immediately anyway)"), transitions);
                if (transitions) Get.FadeDuration = 0.5f;
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUIUtility.fieldWidth = 35;
                EditorGUILayout.PropertyField(sp_FadeDuration);
                EditorGUIUtility.fieldWidth = 0;
                EditorGUIUtility.labelWidth = 60;
                EditorGUILayout.PropertyField(sp_FadeVisibility, new GUIContent("Out View", sp_FadeVisibility.tooltip), GUILayout.Width(74) );
                EditorGUILayout.EndHorizontal();
            }

            //GUI.color = preCol;
        }


        protected virtual void DefaultInspectorStack(Optimizer_Base Get, bool endVert = true)
        {

            if (Application.isPlaying) GUI.enabled = false;
            EditorGUILayout.BeginVertical(FEditor.FGUI_Inspector.Style(new Color(0.975f, 0.975f, 0.975f, .325f)));

            EditorGUI.indentLevel++;
            DrawSetup = EditorGUILayout.Foldout(DrawSetup, "Optimizer Setup", true, new GUIStyle(EditorStyles.foldout) { fontStyle = FontStyle.Bold });
            EditorGUI.indentLevel--;

            if (DrawSetup)
            {
                List<string> excluded = new List<string> { "MaxDistance", "DetectionBounds", "m_Script", "LODLevels", "FadeDuration", "DeactivateObject", "DrawSharedSettingsOptions" };

                if (Get.OptimizingMethod == EOptimizingMethod.Static || Get.OptimizingMethod == EOptimizingMethod.Effective)
                {
                    // Culling groups related settings
                    if (!Get.CullIfNotSee) excluded.Add("DetectionRadius");
                }
                else // Repeating clocks related settings
                {
                    excluded.Add("DetectionRadius");
                    //excluded.Add("CullIfNotSee");
                }

                if (Get.CullIfNotSee) excluded.Add("Hideable");

                if (!drawDetectionOffset)
                    if (!excluded.Contains("DetectionOffset")) excluded.Add("DetectionOffset");

                if (!drawHideable)
                    if (!excluded.Contains("Hideable")) excluded.Add("Hideable");

                if (Get.OptimizingMethod == EOptimizingMethod.Dynamic || Get.OptimizingMethod == EOptimizingMethod.TriggerBased)
                    if (Get.CullIfNotSee)
                    {
                        if (excluded.Contains("DetectionBounds")) excluded.Remove("DetectionBounds");
                    }

                if (!drawCullIfNotSee)
                {
                    if (!drawDetectionRadius) if (!excluded.Contains("DetectionRadius")) excluded.Add("DetectionRadius");
                    if (!excluded.Contains("CullIfNotSee")) excluded.Add("CullIfNotSee");
                }

                FEditor.FGUI_Inspector.DrawUILine(new Color(0.5f, 0.5f, 0.5f, 0.5f), 2, 4);

                EditorGUILayout.BeginHorizontal();


                if (Get.AutoDistance) GUI.enabled = false;

                EditorGUILayout.PropertyField(sp_MaxDist);

                if (Get.AutoDistance) GUI.enabled = true;

                if (Get.DrawAutoDistanceToggle)
                {
                    //, camera fov and camera far clip planes
                    if (Get.AutoDistance) GUI.color = new Color(0.5f, 0.5f, 0.5f, 0.925f);

                    if (GUILayout.Button(new GUIContent("Auto", "Automatic max distance basing on detection shape size, algorithm will try to set max distance to value where object would be so small on screen it would be almost not visible.\nIf you want to cull small objects ou can click auto, unclick and set even lower value."), new GUILayoutOption[2] { GUILayout.Width(42), GUILayout.Height(15) }))
                    {
                        Get.AutoDistance = !Get.AutoDistance;
                        Get.SetAutoDistance(1f);

                    }

                    GUI.color = preCol;

                    if (Get.AutoDistance) GUI.enabled = false;
                    if (GUILayout.Button(new GUIContent("Set Far", "Automatic max distance basing on detection shape size, algorithm will try to set max distance to value where object would be so small on screen it would be very small but still kinda visible."), new GUILayoutOption[2] { GUILayout.Width(55), GUILayout.Height(15) }))
                    {
                        Get.SetAutoDistance(0.7f);
                    }



                    if (Get.AutoDistance) GUI.enabled = true;

                }


                EditorGUILayout.EndHorizontal();

                FEditor.FGUI_Inspector.DrawUILine(new Color(0.5f, 0.5f, 0.5f, 0.5f), 1, 5);

                GUILayout.Space(1f);
                DrawPropertiesExcluding(serializedObject, excluded.ToArray());
                GUILayout.Space(3f);
            }

            if (endVert) EditorGUILayout.EndVertical();
            if (Application.isPlaying) GUI.enabled = true;

        }


        protected virtual void DrawLODSettingsStack(Optimizer_Base Get)
        {
            SceneView view = SceneView.lastActiveSceneView;

            if (!ClickedOnSlider)
            {
                EditorGUILayout.HelpBox("Click on L.O.D. distance state to view settings, drag the edges to change distance ranges", MessageType.None);
                if (view != null) GUILayout.Space(3);
            }

            if (view != null) GUILayout.Space(6);

            LODSettingsStackStart();

            GUILayout.Space(2);

            #region LODs reset and preparation to draw

            if (selectedLOD > Get.LODLevels + 1) selectedLOD = Get.LODLevels;

            bool hiddenRange = false;
            if (drawHiddenRange) if (Get.CullIfNotSee || Get.Hideable) hiddenRange = true;

            Rect sliderRect = GUILayoutUtility.GetRect(0, 30 + (hiddenRange ? 14 : 0), GUILayout.ExpandWidth(true));
            Rect buttonsRect = sliderRect;
            if (hiddenRange) buttonsRect = new Rect(sliderRect.x, sliderRect.y, sliderRect.width, sliderRect.height - 14);

            List<Optimizers_LODGUI.Optimizers_LODInfo> infos = new List<Optimizers_LODGUI.Optimizers_LODInfo>();
            for (int i = 0; i < Get.LODLevels; i++)
            {
                string name = GetLODName(i, Get.LODLevels);

                if (i >= Get.LODPercent.Count) return;

                Optimizers_LODGUI.Optimizers_LODInfo info = new Optimizers_LODGUI.Optimizers_LODInfo(i, name, Get.LODPercent[i], Get);
                info.ButtonRect = Optimizers_LODGUI.CalcLODButton(buttonsRect, info.LODPercentage, hiddenRange);
                info.MaxDist = Get.MaxDistance;

                float previousPerc = 0f;
                if (i != 0) previousPerc = infos[i - 1].LODPercentage;
                float percentage = info.LODPercentage;

                info.RangeRect = Optimizers_LODGUI.CalcLODRange(buttonsRect, previousPerc, info.LODPercentage);

                infos.Add(info);
            }

            #endregion

            if (view != null)
            {
                for (int i = 0; i < infos.Count; i++)
                    Optimizers_LODGUI.DrawCameraButtonForRange(infos[i], view, Get.GetDistanceMeasures()[infos[i].LODLevel]);
                
                //FOptimizers_LODGUI.FOptimizers_LODInfo cullInfo = new FOptimizers_LODGUI.FOptimizers_LODInfo(Get.LODLevels, "", 1f, Get);
                //FOptimizers_LODGUI.DrawCameraButtonForRange(cullInfo, view, Get.MaxDistance, infos[infos.Count - 1]);
            }

            DrawLODLevelsSlider(Get, sliderRect, infos, hiddenRange);

            if (selectedLOD >= 0 && selectedLOD <= Get.LODLevels + 1)
            {
                #region Drawing info about selected LOD Level

                Color frameCol = Optimizers_LODGUI.culledLODColor;
                if (selectedLOD < Get.LODLevels) frameCol = Optimizers_LODGUI.lODColors[selectedLOD];

                GUI.color = frameCol * new Color(1f, 1f, 1f, 0.9f);
                EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);
                //EditorGUILayout.BeginVertical(FEditor.FGUI_Inspector.Style(frameCol * new Color(1f, 1f, 1f, 0.135f)));
                GUI.color = c;
                GUIStyle nameStyle = new GUIStyle(EditorStyles.boldLabel);
                nameStyle.alignment = TextAnchor.MiddleLeft;

                string lodName = "Culled";
                if (selectedLOD != Get.LODLevels)
                    if (selectedLOD < Get.LODLevels + 1) lodName = infos[selectedLOD].LODName;
                    else
                        lodName = "Hide";

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(lodName, nameStyle, new GUILayoutOption[1] { GUILayout.Width(lodName.Length * 8) });

                float startAmount = 0f;
                float endAmount = Get.MaxDistance;

                float[] measure = Get.GetDistanceMeasures();

                if (selectedLOD > 0)
                {
                    if (selectedLOD != Get.LODLevels)
                        if (selectedLOD < Get.LODLevels + 1)
                        {
                            startAmount = measure[selectedLOD - 1];
                            endAmount = measure[selectedLOD];

                            //startAmount = Mathf.Lerp(Get.MinMaxDistance.x, Get.MaxDistance, infos[selectedLOD - 1].LODPercentage);
                            //endAmount = Mathf.Lerp(Get.MinMaxDistance.x, Get.MaxDistance, infos[selectedLOD].LODPercentage);
                        }
                }
                else
                {
                    if (Get.LODLevels > 1)
                    {
                        endAmount = measure[0];
                        //endAmount = Mathf.Lerp(Get.MinMaxDistance.x, Get.MaxDistance, infos[0].LODPercentage);
                    }
                }

                startAmount = (float)Math.Round(startAmount, 1);
                endAmount = (float)Math.Round(endAmount, 1);

                GUIStyle infoStyle = new GUIStyle(EditorStyles.label);
                infoStyle.alignment = TextAnchor.MiddleLeft;

                if (selectedLOD < Get.LODLevels)
                    EditorGUILayout.LabelField(new GUIContent("| Distance between " + startAmount + " - " + endAmount + " units", "Distance from object to main camera"), infoStyle);
                else
                    if (selectedLOD < Get.LODLevels + 1)
                    EditorGUILayout.LabelField(new GUIContent("| Distance above " + Get.MaxDistance + " units", "Distance from object to main camera"), infoStyle);
                else
                {
                    if (Get.Hideable == false)
                    {
                        if (Get.CullIfNotSee)
                        {
                            EditorGUILayout.LabelField(new GUIContent("| When camera looking away"), infoStyle);
                        }
                    }
                    else
                    {
                        if (Get.CullIfNotSee)
                        {
                            EditorGUILayout.LabelField(new GUIContent("| When camera looking away or used Optimizer.SetHidden()"), infoStyle);
                        }
                        else
                        {
                            EditorGUILayout.LabelField(new GUIContent("| When object used SetHidden() through code"), infoStyle);
                        }
                    }
                }

                EditorGUILayout.EndHorizontal();

                FEditor.FGUI_Inspector.DrawUILine(new Color(0.5f, 0.5f, 0.5f, 0.5f), 2, 4);

                #endregion

                PreOptionsStack();

                DrawLODOptionsStack(selectedLOD);

                EditorGUILayout.EndVertical();
            }

        }


        protected virtual void LODSettingsStackStart() { }
        protected virtual void PreOptionsStack() { }

        private string GetLODName(int i, int count)
        {
            string name = "LOD " + (i);
            if (i == 0) name = "Nearest";
            if (i == count) name = "Farthest";
            if (count <= 1) name = "Active";
            return name;
        }


        protected virtual void DrawToOptimizeStack(Optimizer_Base Get)
        {
            EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);
            //EditorGUILayout.BeginVertical(FEditor.FGUI_Inspector.Style(new Color(0.75f, 0.75f, 0.15f, 0.2f)));

            EditorGUI.indentLevel++;
            if (!Get.OptimizationListExists()) Get.AssignComponentsToOptimizeFrom(Get);
            DrawToOptimize = EditorGUILayout.Foldout(DrawToOptimize, "To Optimize (" + Get.GetToOptimizeCount() + ")", true, new GUIStyle(EditorStyles.foldout) { fontStyle = FontStyle.Bold });
            EditorGUI.indentLevel--;

            if (DrawToOptimize)
            {
                DrawToOptimizeComponents();
            }

            EditorGUILayout.EndVertical();
        }


        protected virtual void DrawToOptimizeComponents()
        {
            EditorGUILayout.LabelField("What was there?");
        }

        protected virtual void DrawLODOptionsStack(int lodId)
        {
            //Optimizer_Base scr = target as Optimizer_Base;
            if (Get.OptimizationListExists())
            {
                if (lodId == Get.LODLevels) if (Get.DrawDeactivateToggle)
                    {
                        GUI.color = GUI.color * new Color(1f, 1f, 1f, 0.7f);

                        if (!Application.isPlaying)
                            if (Get.transform.childCount > 0)
                            {
                                if (Get.DeactivateObject)
                                {
                                    EditorGUILayout.HelpBox("Whole GameObject deactivation sometimes can cause lags. Enter on red '!' on the right for tooltip", MessageType.Info);
                                }
                            }

                        GUI.color = individualColor;

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("DeactivateObject"));

                        if (Get.DeactivateObject) GUI.color = Color.red; else GUI.color = Color.green;

                        GUIStyle whiteL = new GUIStyle(EditorStyles.whiteBoldLabel);
                        whiteL.normal.textColor = Color.white; // Unity 2019.3 makes "WhiteBoldLabel" black
                        EditorGUILayout.LabelField(new GUIContent("!", "Deactivating whole game object sometimes can be hard to compute for Unity.\nIf you experience lags when rotating camera, try to NOT disable whole game object, but only disable components."), whiteL, new GUILayoutOption[1] { GUILayout.Width(20) });

                        EditorGUILayout.EndHorizontal();
                        GUI.color = preCol;
                    }

                Undo.RecordObject(serializedObject.targetObject, "Changing LOD Settings");

                if (Get.CullIfNotSee || Get.Hideable)
                    if (lodId == Get.LODLevels + 1)
                    {
                        GUI.color = individualColor;
                        if (Get.HiddenCullAt == -1)
                        {
                            bool toggle = true;
                            toggle = EditorGUILayout.Toggle("Same as culling", toggle);
                            if (toggle == false) Get.HiddenCullAt = 0;
                        }
                        else
                        {
                            Get.HiddenCullAt = EditorGUILayout.IntSlider(new GUIContent("Cull from LOD", "From which LOD level, looking away or hiding object will apply culling LOD settings"), Get.HiddenCullAt + 1, 0, Get.LODLevels) - 1;
                        }

                        GUI.color = preCol;
                    }


                if (lodId == 0)
                {
                    if (!Get.UnlockFirstLOD)
                    {
                        GUILayout.Space(2);
                        EditorGUILayout.BeginHorizontal();
                        GUI.enabled = false;
                        EditorGUILayout.LabelField("First LOD - Default Settings - Nothing to change", new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.UpperCenter, fontSize = 9 });
                        GUI.enabled = true;
                        EditorGUIUtility.labelWidth = 2;
                        Get.UnlockFirstLOD = EditorGUILayout.Toggle(new GUIContent(" ", "Toggle to enable editing first LOD level (experimental) - can be helpful if you would need to make lower parameters when object is near to camera then change it to higher values when camera goes further."), Get.UnlockFirstLOD, new GUILayoutOption[2] { GUILayout.Width(20), GUILayout.Height(12) });
                        EditorGUILayout.EndHorizontal();
                    }
                    else
                    {
                        GUILayout.Space(2);
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("First LOD - Default Settings", new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.UpperCenter, fontSize = 9 });
                        EditorGUIUtility.labelWidth = 2;
                        Get.UnlockFirstLOD = EditorGUILayout.Toggle(new GUIContent(" ", "Toggle to enable editing first LOD level (experimental) - can be helpful if you would need to make lower parameters when object is near to camera then change it to higher values when camera goes further."), Get.UnlockFirstLOD, new GUILayoutOption[2] { GUILayout.Width(20), GUILayout.Height(12) });
                        EditorGUILayout.EndHorizontal();
                    }

                    EditorGUIUtility.labelWidth = 0;
                }

                serializedObject.ApplyModifiedProperties();

                bool preEnabled = GUI.enabled;

                if (lodId == Get.LODLevels + 1)
                    if (Get.HiddenCullAt < 0)
                    {
                        lodId = Get.LODLevels;
                        GUI.enabled = false;
                    }

                EditorGUILayout.BeginHorizontal();

                //if (Get.ToOptimize.Count > 3)
                //{
                //    if (lodId > 0 || Get.UnlockFirstLOD)
                //    {
                //        if (GUILayout.Button("Set Enabled All")) for (int i = 0; i < Get.ToOptimize.Count; i++) { Get.ToOptimize[i].LODSet.LevelOfDetailSets[selectedLOD].Disable = false; }
                //        if (GUILayout.Button("Set Disabled All")) for (int i = 0; i < Get.ToOptimize.Count; i++) { Get.ToOptimize[i].LODSet.LevelOfDetailSets[selectedLOD].Disable = true; }
                //    }
                //}

                EditorGUILayout.EndHorizontal();

                DrawLODOptionsFor(lodId);

                GUI.enabled = preEnabled;
            }
        }

        protected virtual void DrawLODOptionsFor(int lodID)
        {

        }


        private void DrawLODLevelsSlider(Optimizer_Base script, Rect lodRect, List<Optimizers_LODGUI.Optimizers_LODInfo> lods, bool hidden)
        {
            Optimizer_Base Get = (Optimizer_Base)target;

            int sliderId = GUIUtility.GetControlID(sliderControlId, FocusType.Passive);
            Event evt = Event.current;

            bool canInPlaymode = true;
            if (Application.isPlaying) canInPlaymode = Get.OptimizingMethod != EOptimizingMethod.Static;

            switch (evt.GetTypeForControl(sliderId))
            {
                case EventType.Repaint:
                    {
                        Optimizers_LODGUI.DrawLODSlider(Get, lodRect, lods, selectedLOD, distance, Get.MaxDistance, hidden, Get.HiddenCullAt); break;
                    }

                case EventType.MouseDown:
                    {
                        Get.OnValidate();

                        // Grow position on the x because edge buttons overflow by 5 pixels
                        Rect barRect = lodRect;
                        barRect.x -= 5;
                        barRect.width += 10;

                        if (barRect.Contains(evt.mousePosition))
                        {
                            evt.Use();
                            GUIUtility.hotControl = sliderId;

                            // Check for button click
                            bool clickedSliderButton = false;

                            // Re-sort the LOD array for these buttons to get the overlaps in the right order
                            var lodsLeft = lods.Where(lod => lod.LODPercentage > 0.5f).OrderByDescending(x => x.LODLevel);
                            var lodsRight = lods.Where(lod => lod.LODPercentage <= 0.5f).OrderBy(x => x.LODLevel);

                            var lodButtonOrder = new List<Optimizers_LODGUI.Optimizers_LODInfo>();
                            lodButtonOrder.AddRange(lodsLeft);
                            lodButtonOrder.AddRange(lodsRight);

                            foreach (Optimizers_LODGUI.Optimizers_LODInfo lod in lodButtonOrder)
                                if (lod.ButtonRect.Contains(evt.mousePosition))
                                {
                                    selectedLODSlider = lod.LODLevel;
                                    clickedSliderButton = true;
                                    ClickedOnSlider = true;
                                    break;
                                }

                            if (!clickedSliderButton)
                            {
                                // Check for culled selection
                                if (Optimizers_LODGUI.GetCulledBox(lodRect, lods[lods.Count - 1].LODPercentage).Contains(evt.mousePosition))
                                {
                                    ClickedOnSlider = true;
                                    selectedLOD = script.LODLevels; break;
                                }

                                // Check for range click
                                foreach (Optimizers_LODGUI.Optimizers_LODInfo lod in lodButtonOrder)
                                    if (lod.RangeRect.Contains(evt.mousePosition))
                                    {
                                        ClickedOnSlider = true;
                                        selectedLOD = lod.LODLevel; break;
                                    }

                                if (hidden)
                                {
                                    Rect hiddenRect = new Rect(barRect.x, barRect.y + 30, barRect.width, 14); ;
                                    if (hiddenRect.Contains(evt.mousePosition))
                                    {
                                        ClickedOnSlider = true;
                                        selectedLOD = script.LODLevels + 1; break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            Rect safeRect = lodRect;
                            safeRect.height += 1900;

                            if (!safeRect.Contains(evt.mousePosition))
                            {
                                selectedLOD = -1;
                                Get.Gizmos_SelectLOD(-1);
                            }

                            safeRect.height -= 1900;
                        }

                        serializedObject.ApplyModifiedProperties();
                        break;

                    }


                case EventType.MouseDrag:
                    {
                        if (Application.isPlaying) break;

                        Rect barRect = lodRect;
                        barRect.x -= 5;
                        barRect.width += 10;

                        if (barRect.Contains(evt.mousePosition)) // If mouse is on lod slider then we using drag event elseware it whould be used and not allow dragging properties sliders
                        {
                            if (selectedLODSlider < script.LODLevels - 1)
                            {
                                evt.Use();

                                if (canInPlaymode)
                                {
                                    float cameraPercent = Optimizers_LODGUI.GetLODSliderPercent(evt.mousePosition, lodRect);
                                    Optimizers_LODGUI.SetSelectedLODLevelPercentage(cameraPercent - 0.001f, selectedLODSlider, lods);
                                    if (selectedLODSlider > -1) script.LODPercent[selectedLODSlider] = lods[selectedLODSlider].LODPercentage;
                                    ClickedOnSlider = true;
                                    Get.Gizmos_IsResizingLOD(selectedLODSlider);

                                }
                                else
                                    Debug.Log("[OPTIMIZERS EDITOR] It's not allowed to change culling size in playmode!");
                            }
                        }

                        break;
                    }

                case EventType.MouseUp:
                    {

                        if (GUIUtility.hotControl == sliderId)
                        {
                            Get.Gizmos_StopChanging();

                            GUIUtility.hotControl = 0;
                            selectedLODSlider = -1;
                            evt.Use();
                        }

                        EditorUtility.SetDirty(Get);
                        serializedObject.ApplyModifiedProperties();
                        break;
                    }

                case EventType.DragUpdated:
                case EventType.DragPerform:
                    {
                        int lodLevel = -2;

                        foreach (Optimizers_LODGUI.Optimizers_LODInfo lod in lods)
                            if (lod.RangeRect.Contains(evt.mousePosition))
                            {
                                lodLevel = lod.LODLevel;
                                break;
                            }

                        if (lodLevel == -2)
                        {
                            Rect culledRange = Optimizers_LODGUI.GetCulledBox(lodRect, lods.Count > 0 ? lods[lods.Count - 1].LODPercentage : 1.0f);
                            if (culledRange.Contains(evt.mousePosition)) lodLevel = -1;
                        }

                        if (lodLevel >= -1)
                        {
                            selectedLOD = lodLevel;
                            evt.Use();
                        }

                        break;
                    }

                case EventType.DragExited: { evt.Use(); break; }
            }

        }


        protected void DrawDragAndDropSquare(Optimizer_Base optimizer)
        {
            Color c = GUI.color;

            Color preCol = GUI.color;
            GUI.color = new Color(0.5f, 1f, 0.5f, 0.9f);

            var drop = GUILayoutUtility.GetRect(0f, 45f, new GUILayoutOption[1] { GUILayout.ExpandWidth(true) });
            GUI.Box(drop, "Drag & Drop your GameObjects / Components here", new GUIStyle(EditorStyles.helpBox) { alignment = TextAnchor.MiddleCenter, fixedHeight = drop.height });
            var dropEvent = Event.current;
            GUILayout.Space(1);

            switch (dropEvent.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!drop.Contains(dropEvent.mousePosition)) break;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (dropEvent.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        foreach (var dragged in DragAndDrop.objectReferences)
                        {
                            GameObject draggedObject = dragged as GameObject;
                            if (!draggedObject)
                            {
                                MonoBehaviour mono = dragged as MonoBehaviour;
                                if (mono)
                                {
                                    Undo.RecordObject(serializedObject.targetObject, "Adding MonoBehaviour to optimize");
                                    optimizer.AssignCustomComponentToOptimize(mono);
                                    EditorUtility.SetDirty(target);
                                }
                                else
                                {
                                    Component comp = dragged as Component;
                                    if (comp)
                                    {
                                        Undo.RecordObject(serializedObject.targetObject, "Adding Component to optimize");
                                        optimizer.AssignComponentsToOptimizeFrom(comp);
                                        EditorUtility.SetDirty(target);
                                    }
                                }
                            }
                            else
                            {
                                Undo.RecordObject(serializedObject.targetObject, "Adding MonoBehaviours to optimize");
                                MonoBehaviour[] comps = draggedObject.GetComponents<MonoBehaviour>();

                                int optims = 0;
                                for (int i = 0; i < comps.Length; i++)
                                {
                                    //Debug.Log("Trajing " + comps[i].GetType().ToString());
                                    optimizer.AssignCustomComponentToOptimize(comps[i]);
                                    if (comps[i] is Optimizer_Base) optims++;
                                }

                                EditorUtility.SetDirty(target);

                                if (comps.Length == 0 || comps.Length == optims)
                                {
                                    Undo.RecordObject(serializedObject.targetObject, "Adding Component to optimize");
                                    optimizer.AssignComponentsToOptimizeFrom(draggedObject.transform);
                                    EditorUtility.SetDirty(target);
                                }
                            }
                        }

                    }

                    Event.current.Use();
                    break;
            }


            GUILayout.BeginVertical();
            if (ActiveEditorTracker.sharedTracker.isLocked) GUI.color = new Color(0.44f, 0.44f, 0.44f, 0.8f); else GUI.color = new Color(0.9f, 0.9f, 0.9f, 0.85f);
            GUILayout.Space(7);
            if (GUILayout.Button(new GUIContent("Lock Inspector for Drag & Drop", "Drag & drop components or game objects with components to the box"), EditorStyles.miniButton)) ActiveEditorTracker.sharedTracker.isLocked = !ActiveEditorTracker.sharedTracker.isLocked;
            GUI.color = c;
            GUILayout.EndVertical();


            GUI.color = preCol;
        }


        private void DrawAddRigidbodyToCamera()
        {
            Camera c = Camera.main;
            if (!c) c = GameObject.FindObjectOfType<Camera>();
            if (!c) return;

            Rigidbody rig = c.GetComponent<Rigidbody>();
            Collider col = c.GetComponent<Collider>();

            if (!rig & !!!col)
            {
                EditorGUILayout.HelpBox("If you are using Trigger Based method, your camera needs to have rigidbody and small collider to make it work correctly.", MessageType.Info);
                if (GUILayout.Button("Add Rigidbody And/Or Collider to Main Camera"))
                {
                    if (!rig)
                    {
                        rig = c.gameObject.AddComponent<Rigidbody>();
                        rig.isKinematic = true;
                        rig.useGravity = false;
                    }

                    if (!col)
                    {
                        SphereCollider sph = c.gameObject.AddComponent<SphereCollider>();
                        sph.radius = 0.1f;
                    }
                }
            }
        }


        protected void SaveAllLODSets(ScriptableOptimizer optimizer)
        {
            for (int i = 0; i < optimizer.ToOptimize.Count; i++)
                optimizer.ToOptimize[i].SaveLODSet();

#if UNITY_EDITOR
            AssetDatabase.SaveAssets();
#endif
        }
    }


}

