using FIMSpace.FEditor;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.FOptimizing
{
    public partial class Optimizer_BaseEditor
    {

        protected void DefaultInspectorStackMultiShape()
        {
            GUILayout.Space(5);
            EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);

            if ( Get.UseObstacleDetection)
            {
                Get.UseObstacleDetection = false;
            }

            if (Get.Shapes == null) Get.Shapes = new List<FOptimizing.Optimizer_Base.MultiShapeBound>();

            EditorGUI.indentLevel++;
            if (!DrawDetectionSetup)
            {
                DrawDetectionSetup = EditorGUILayout.Foldout(DrawDetectionSetup, " Detection Spheres Setup (" + Get.Shapes.Count + ")", true, new GUIStyle(EditorStyles.foldout) { fontStyle = FontStyle.Bold });
                EditorGUI.indentLevel--;
            }
            else
            {
                EditorGUILayout.BeginHorizontal();

                DrawDetectionSetup = EditorGUILayout.Foldout(DrawDetectionSetup, " Detection Spheres Setup (" + Get.Shapes.Count + ")", true, new GUIStyle(EditorStyles.foldout) { fontStyle = FontStyle.Bold });
                EditorGUI.indentLevel--;
                GUILayout.FlexibleSpace();

                if (GUILayout.Button(new GUIContent("+", "Adding new detection sphere to optimizer list"), new GUILayoutOption[2] { GUILayout.Width(20), GUILayout.Height(15) }))
                {
                    Undo.RecordObject(serializedObject.targetObject, "Adding Detection Sphere");

                    Vector3 newPos = Vector3.zero;
                    if (Get.Shapes.Count > 0) newPos = (Get.Shapes[Get.Shapes.Count - 1].position + Vector3.up / 2);
                    FOptimizing.Optimizer_Base.MultiShapeBound sph = new FOptimizing.Optimizer_Base.MultiShapeBound();
                    sph.position = newPos;

                    Get.Shapes.Add(sph);
                    serializedObject.Update();
                    serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(Get);
                }

                EditorGUILayout.EndHorizontal();
                GUILayout.Space(5);

                EditorGUIUtility.labelWidth = 20;

                GUILayoutOption[] op = new GUILayoutOption[1] { GUILayout.MinWidth(60) };

                for (int i = 0; i < Get.Shapes.Count; i++)
                {
                    if (Get.Shapes[i] == null) continue;

                    EditorGUILayout.BeginHorizontal();
                    Vector3 vPos = Get.Shapes[i].position;

                    Undo.RecordObject(serializedObject.targetObject, "Changing Detection Sphere");

                    EditorGUILayout.LabelField("[" + i + "] ", new GUILayoutOption[1] { GUILayout.MaxWidth(28) });
                    GUILayout.FlexibleSpace();
                    EditorGUI.BeginChangeCheck();

                    Get.Shapes[i].transform = (Transform)EditorGUILayout.ObjectField(Get.Shapes[i].transform, typeof(Transform), true, new GUILayoutOption[1] { GUILayout.Width(66) });//EditorGUILayout.ObjectField(GUIContent.none, complex.Shapes[i].transform, op);
                    //EditorGUILayout.ObjectField(sp_shapes.GetArrayElementAtIndex(i), GUIContent.none, new GUILayoutOption[1] { GUILayout.Width(50) });//EditorGUILayout.ObjectField(GUIContent.none, complex.Shapes[i].transform, op);
                    //EditorGUILayout.PropertyField(sp_shapes.GetArrayElementAtIndex(i), new GUIContent("E"), new GUILayoutOption[1] { GUILayout.Width(30) });//EditorGUILayout.ObjectField(GUIContent.none, complex.Shapes[i].transform, op);
                    vPos.x = EditorGUILayout.FloatField("x", Get.Shapes[i].position.x, op);
                    vPos.y = EditorGUILayout.FloatField("y", Get.Shapes[i].position.y, op);
                    vPos.z = EditorGUILayout.FloatField("z", Get.Shapes[i].position.z, op);
                    Get.Shapes[i].radius = EditorGUILayout.FloatField("s", Get.Shapes[i].radius, op);

                    if (EditorGUI.EndChangeCheck())
                    {
                        serializedObject.ApplyModifiedProperties();
                        EditorUtility.SetDirty(Get);
                    }

                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button(new GUIContent("X", "Removing detection sphere from list"), new GUILayoutOption[2] { GUILayout.Width(20), GUILayout.Height(15) }))
                    {
                        Undo.RecordObject(serializedObject.targetObject, "Removing Detection Sphere");
                        EditorGUI.BeginChangeCheck();
                        Get.Shapes.RemoveAt(i);
                        EditorGUI.EndChangeCheck();

                        return;
                    }

                    EditorGUILayout.EndHorizontal();

                    Get.Shapes[i].position = vPos;
                }

                EditorGUIUtility.labelWidth = 0;
                EditorGUIUtility.fieldWidth = 0;
                GUILayout.Space(3);
            }

            EditorGUILayout.EndVertical();

            GUILayout.Space(6);

            Undo.RecordObject(target, "Detection Sphere Variable");
            EditorGUI.BeginChangeCheck();
            Get.DetectionRadius = EditorGUILayout.FloatField(new GUIContent("Detection Radius", "Radius multiplier for all detection spheres"), Get.DetectionRadius);
            EditorGUIUtility.labelWidth = 158;
            EditorGUILayout.PropertyField(sp_DrawPositionHandles);
            if (Get.DrawPositionHandles)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(sp_ScalingHandles);
                EditorGUI.indentLevel--;
            }
            EditorGUI.EndChangeCheck();

            GUILayout.Space(7);
            EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxLightStyle);
            EditorGUILayout.PropertyField(sp_AutoReferenceMesh);
            EditorGUIUtility.labelWidth = 0;
            if (!Get.AutoReferenceMesh) GUI.enabled = false;
            GUILayout.Space(3);
            if (GUILayout.Button(new GUIContent("Auto Detect Spheres For Mesh", "Automatically creating detection spheres basing on target mesh structure")))
            {
                Undo.RecordObject(serializedObject.targetObject, "Generating Auto Shape");
                EditorGUI.BeginChangeCheck();
                Get.GenerateAutoShape();
                EditorGUI.EndChangeCheck();
            }
            EditorGUILayout.PropertyField(sp_AutoPrecision);
            EditorGUILayout.EndVertical();

            if (!Get.AutoReferenceMesh) GUI.enabled = true;
        }


        protected void OnSceneGUIMultiShape()
        {
            //if (!DrawDetectionSetup)
            if (Get.DrawPositionHandles)
                if (Get.Shapes != null)
                {
                    Matrix4x4 m = Get.transform.localToWorldMatrix;
                    Matrix4x4 mw = Get.transform.worldToLocalMatrix;

                    Undo.RecordObject(Get, "Changing position of detection spheres");

                    for (int i = 0; i < Get.Shapes.Count; i++)
                    {
                        Matrix4x4 mt;
                        Matrix4x4 mtw;

                        if (Get.Shapes[i].transform == null)
                        {
                            mt = m;
                            mtw = mw;
                        }
                        else
                        {
                            mt = Get.Shapes[i].transform.localToWorldMatrix;
                            mtw = Get.Shapes[i].transform.worldToLocalMatrix;
                        }

                        Vector3 pos = mt.MultiplyPoint(Get.Shapes[i].position);

                        if (Get.ScalingHandles)
                        {
                            Vector3 scaled = FEditor_TransformHandles.ScaleHandle(Vector3.one * Get.Shapes[i].radius, pos, Quaternion.identity, .3f, true, true);
                            Get.Shapes[i].radius = scaled.x;
                        }

                        Vector3 transformed = FEditor_TransformHandles.PositionHandle(pos, Quaternion.identity, .3f, true, !Get.ScalingHandles);
                        if (Vector3.Distance(transformed, pos) > 0.00001f) Get.Shapes[i].position = mtw.MultiplyPoint(transformed);
                    }
                }
        }

    }
}

