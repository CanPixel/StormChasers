using FIMSpace.FEditor;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.FOptimizing
{
    public partial class Optimizer_EditorManager : Editor
    {
        int selectedContID = 0;
        int selectedContainer = 0;

        void DrawContainersBrowser()
        {

            OptimizersManager m = OptimizersManager.Instance;

            if (!m)
                EditorGUILayout.HelpBox("No Optimizers Controller!", MessageType.Error);
            else
            {
                EditorUtility.SetDirty(m);

                int contListsCount = m.CullingContainersIDSpecific.Count;

                if (contListsCount == 0)
                {
                    EditorGUILayout.HelpBox("No Container Defined Yet.", MessageType.Info);
                }
                else
                {
                    // id list of different LOD ranges (different LOD count or distance) object sets
                    int[] cIds = m.GetContainersIDs();
                    Optimizers_CullingContainersList contList = m.CullingContainersIDSpecific[cIds[selectedContID]];

                    EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);


                    EditorGUILayout.BeginHorizontal(FGUI_Resources.BGInBoxBlankStyle);

                    if (GUILayout.Button("<", GUILayout.Width(32))) { SwitchSelectedContainer(ref selectedContID, -1, cIds.Length); selectedContainer = 0; }
                    EditorGUILayout.LabelField((1 + selectedContID) + " / " + cIds.Length, FGUI_Resources.HeaderStyle);
                    if (GUILayout.Button(">", GUILayout.Width(32))) { SwitchSelectedContainer(ref selectedContID, 1, cIds.Length); selectedContainer = 0; }

                    EditorGUILayout.EndHorizontal();


                    Optimizers_CullingContainer cont = contList[selectedContainer];

                    GUILayout.Space(4);
                    EditorGUILayout.BeginVertical();

                    EditorGUILayout.BeginHorizontal(FGUI_Resources.BGInBoxLightStyle);

                    if (contList.Count > 1)
                    {
                        if (GUILayout.Button("<", GUILayout.Width(32))) SwitchSelectedContainer(ref selectedContainer, -1, contList.Count);
                        EditorGUILayout.LabelField("[" + (1 + selectedContainer) + " / " + contList.Count + "]", FGUI_Resources.HeaderStyle, GUILayout.Width(62));
                    }

                    EditorGUILayout.LabelField("Container ID: '" + cont.ID + "'     LOD Distances: " + (cont.DistanceLevels.Length - 2) + "     Slots Taken: " + cont.SlotsTaken, FGUI_Resources.HeaderStyle);
                    //EditorGUILayout.LabelField("Container ID: '" + cont.ID + "'     Bounds: " + (cont.BoundingCount) + "     Optimizers: " + cont.AddedOptimizers, FGUI_Resources.HeaderStyle);

                    if (contList.Count > 1)
                    {
                        if (GUILayout.Button(">", GUILayout.Width(32))) SwitchSelectedContainer(ref selectedContainer, 1, contList.Count);
                    }


                    EditorGUILayout.EndHorizontal();

                    GUILayout.Space(6);

                    string distLvls = ""; // Ignoring init epsilon and far end
                    for (int i = 1; i < cont.DistanceLevels.Length - 1; i++)
                        distLvls += "[" + i + "] " + System.Math.Round(cont.DistanceLevels[i], 1) + "   ";

                    EditorGUILayout.LabelField("Capacity: " + cont.SlotsTaken + " / " + cont.Optimizers.Length + "      Have Free Slots: " + cont.HaveFreeSlots + "    Destroying: " + cont.Destroying);
                    EditorGUILayout.LabelField("Distance Levels: " + (cont.DistanceLevels.Length - 2) + "    " + distLvls);

                    GUILayout.Space(6);

                    EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);
                    EditorGUILayout.LabelField("Culling Group", FGUI_Resources.HeaderStyle);
                    EditorGUILayout.EndVertical();
                    GUILayout.Space(4);

                    //GUI.enabled = false;

                    EditorGUILayout.ObjectField("Target Camera Check: ", cont.CullingGroup.targetCamera, typeof(Camera), true);
                    //EditorGUILayout.LabelField(new GUIContent("Bounds: " + cont.BoundingCount, "There will be more bounds than optimizers added when you use multi-shape optimizer feature")); ;
                    GUILayout.Space(3);
                    EditorGUILayout.LabelField("Highest i: " + cont.HighestIndex + "       Next Free i: " + cont.LowestFreeIndex + "      Bounds: " + cont.BoundingCount);
                    GUILayout.Space(6);

                    EditorGUI.indentLevel++;
                    drawOptimList = EditorGUILayout.Foldout(drawOptimList, "Draw Optimizers List (if multi shapes then duplicates)", true);
                    EditorGUI.indentLevel--;

                    if (drawOptimList)
                    {

                        EditorGUIUtility.labelWidth = 34;
                        for (int i = 0; i < cont.Optimizers.Length; i++)
                        {
                            if (cont.Optimizers[i] != null)
                            {
                                EditorGUILayout.BeginHorizontal();
                                string info = "";
                                if (cont.Optimizers[i].IsCulled) GUI.color = new Color(1f, 1f, 1f, 0.5f);

                                EditorGUILayout.ObjectField("[" + i + "]", cont.Optimizers[i], typeof(Optimizer_Base), true);

                                if (cont.Optimizers[i].FarAway) info += "FarAway";
                                else
                                if (cont.Optimizers[i].OutOfCameraView) info += "Camera Dont See";

                                if (cont.Optimizers[i].IsCulled)
                                {
                                    EditorGUILayout.LabelField("(" + info + ")");
                                    GUI.color = new Color(1f, 1f, 1f, 1f);
                                }
                                else
                                {
                                    EditorGUILayout.LabelField("(LOD " + cont.Optimizers[i].CurrentLODLevel + ")");
                                }

                                EditorGUILayout.EndHorizontal();
                            }
                        }
                    }

                    GUILayout.Space(5);


                    EditorGUI.indentLevel++;
                    drawOptimBoundsList = EditorGUILayout.Foldout(drawOptimBoundsList, "Draw Bounds List", true);
                    EditorGUI.indentLevel--;

                    if (drawOptimBoundsList)
                    {
                        EditorGUIUtility.labelWidth = 24;
                        for (int i = 0; i < cont.CullingSpheres.Length; i++)
                        {
                            if (cont.CullingSpheres[i].radius != 0f)
                            {
                                EditorGUILayout.BeginHorizontal();

                                Vector3 pos = cont.CullingSpheres[i].position;

                                EditorGUILayout.LabelField("["+i+"]", GUILayout.Width(32));
                                pos.x = EditorGUILayout.FloatField("x: ", pos.x, GUILayout.Width(110));
                                pos.y = EditorGUILayout.FloatField("y: ", pos.y, GUILayout.Width(110));
                                pos.z = EditorGUILayout.FloatField("z: ", pos.z, GUILayout.Width(110));
                                cont.CullingSpheres[i].radius = EditorGUILayout.FloatField("r: ", cont.CullingSpheres[i].radius, GUILayout.Width(90));

                                if (pos != cont.CullingSpheres[i].position) cont.CullingSpheres[i].position = pos;

                                EditorGUILayout.EndHorizontal();
                            }
                        }
                    }


                    EditorGUIUtility.labelWidth = 0;
                    EditorGUILayout.EndVertical();

                    GUILayout.Space(6);
                    if (GUILayout.Button(drawSelBounds ? "Stop Drawing Bounds" : "Draw Bounds (Scene View)")) drawSelBounds = !drawSelBounds;
                    GUILayout.Space(3);

                    if (drawSelBounds) m.DrawBounds(cont); else m.DrawBounds(null);


                    EditorGUILayout.EndVertical();

                    GUI.enabled = true;

                }
            }
        }

        void DrawUncontained()
        {
            OptimizersManager m = OptimizersManager.Instance;
            if (!m) return;

            // Checking not contained Optimizers
            bool anyNotContained = false;

            if (m.notContainedDynamicOptimizers.Count != 0) anyNotContained = true;
            if (m.notContainedStaticOptimizers.Count != 0) anyNotContained = true;
            if (m.notContainedEffectiveOptimizers.Count != 0) anyNotContained = true;
            if (m.notContainedTriggerOptimizers.Count != 0) anyNotContained = true;

            if (anyNotContained)
            {
                EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);
                EditorGUILayout.LabelField("Not Containered Optimizers", FGUI_Resources.HeaderStyle);
                EditorGUILayout.EndVertical();

                EditorGUILayout.LabelField("Not Containered Effective: " + m.notContainedEffectiveOptimizers.Count, EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Not Containered Static: " + m.notContainedStaticOptimizers.Count, EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Not Containered Dynamic: " + m.notContainedDynamicOptimizers.Count, EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Not Containered Trigger: " + m.notContainedTriggerOptimizers.Count, EditorStyles.boldLabel);
            }

            GUILayout.Space(4f);
        }

        void SwitchSelectedContainer(ref int counter, int by, int count)
        {
            counter += by;
            if (counter >= count) counter = 0;
            if (counter < 0) counter = count - 1;
            if (counter < 0) counter = 0;
        }

        private bool drawOptimList = false;
        private bool drawOptimBoundsList = false;
        private bool drawSelBounds = false;
    }
}
