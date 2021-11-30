using FIMSpace.FEditor;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.FOptimizing
{
    public partial class Optimizer_BaseEditor
    {

        protected void Obstacles_DrawObstaclesSettings()
        {
            if (!Get.CustomCoveragePoints)
            {
                EditorGUILayout.PropertyField(sp_CoveragePrecision);
            }

            if (Get.UseMultiShape)
            {
                Get.UseMultiShape = false;
            }

            EditorGUILayout.PropertyField(sp_CoverageMask);
            EditorGUILayout.PropertyField(sp_CoverageScale);

            EditorGUIUtility.labelWidth = 170;
            EditorGUILayout.PropertyField(sp_CustomRayPoints);
            EditorGUIUtility.labelWidth = 0;

            if (Get.CustomCoveragePoints)
            {
                DrawCoveragePoints();
            }
        }


        private void DrawCoveragePoints()
        {
            EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxLightStyle);

            EditorGUI.indentLevel++;
            if (!drawDetectionSetup)
            {
                drawDetectionSetup = EditorGUILayout.Foldout(drawDetectionSetup, " Custom coverage points", true, new GUIStyle(EditorStyles.foldout) { fontStyle = FontStyle.Bold });
                EditorGUI.indentLevel--;
            }
            else
            {
                EditorGUILayout.BeginHorizontal();

                drawDetectionSetup = EditorGUILayout.Foldout(drawDetectionSetup, " Custom coverage points", true, new GUIStyle(EditorStyles.foldout) { fontStyle = FontStyle.Bold });
                EditorGUI.indentLevel--;
                GUILayout.FlexibleSpace();

                if (GUILayout.Button(new GUIContent("+", "Adding new coverage detection point"), new GUILayoutOption[2] { GUILayout.Width(20), GUILayout.Height(15) }))
                {
                    Undo.RecordObject(serializedObject.targetObject, "Adding Coverage Point");
                    EditorGUI.BeginChangeCheck();
                    Vector3 newPos = Vector3.zero;
                    if (Get.CoverageOffsets.Count > 0) newPos = Get.CoverageOffsets[Get.CoverageOffsets.Count - 1] + Vector3.up / 2;
                    Get.CoverageOffsets.Add(newPos);
                    EditorGUI.EndChangeCheck();
                }

                EditorGUILayout.EndHorizontal();

                EditorGUIUtility.labelWidth = 20;

                GUILayoutOption[] op = new GUILayoutOption[1] { GUILayout.MinWidth(60) };

                for (int i = 0; i < Get.CoverageOffsets.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    Vector3 vPos;

                    Undo.RecordObject(serializedObject.targetObject, "Changing Coverage Point");

                    EditorGUILayout.LabelField("[" + i + "] ", new GUILayoutOption[1] { GUILayout.MaxWidth(28) });
                    GUILayout.FlexibleSpace();
                    EditorGUI.BeginChangeCheck();
                    vPos.x = EditorGUILayout.FloatField("x", Get.CoverageOffsets[i].x, op);
                    vPos.y = EditorGUILayout.FloatField("y", Get.CoverageOffsets[i].y, op);
                    vPos.z = EditorGUILayout.FloatField("z", Get.CoverageOffsets[i].z, op);
                    EditorGUI.EndChangeCheck();
                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button(new GUIContent("X", "Removing detection sphere from list"), new GUILayoutOption[2] { GUILayout.Width(20), GUILayout.Height(15) }))
                    {
                        Undo.RecordObject(serializedObject.targetObject, "Removing Detection Sphere");
                        EditorGUI.BeginChangeCheck();
                        Get.CoverageOffsets.RemoveAt(i);
                        Get.CoverageOffsets.RemoveAt(i);
                        EditorGUI.EndChangeCheck();

                        return;
                    }

                    EditorGUILayout.EndHorizontal();

                    Get.CoverageOffsets[i] = vPos;
                }
            }

            EditorGUIUtility.labelWidth = 0;
            EditorGUILayout.EndVertical();
        }
    }
}

