using FIMSpace.FEditor;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.FOptimizing
{

    [CustomEditor(typeof(TerrainOptimizer))]
    public class OptimizerEditorTerrain : ScriptableOptimizerEditor
    {
        private SerializedProperty sp_Terrain;
        private SerializedProperty sp_TerrainC;
        private SerializedProperty sp_SafeBorders;

        protected override string TargetName() { return " Multi-Terrains Optimizer"; }
        protected override string TargetTooltip() { return "Optimizer Component to use with maps which are using multiple terrain components. It is deriving from ScriptableOptimizer."; }
        protected override Texture2D GetSmallIcon() { if (__texOptimizersIcon != null) return __texOptimizersIcon; __texOptimizersIcon = Resources.Load<Texture2D>("FIMSpace/Optimizers 2/OptTerrIconSmall"); return __texOptimizersIcon; }

        private TerrainOptimizer TGet { get { if (_tGet == null) _tGet = target as TerrainOptimizer; return _tGet; } }
        private TerrainOptimizer _tGet;

        protected override void OnEnable()
        {
            base.OnEnable();
            drawHiddenRange = false;
            drawDetectionSphereHandle = false;
            drawNothingToOptimizeWarning = false;
            sp_Terrain = serializedObject.FindProperty("Terrain");
            sp_TerrainC = serializedObject.FindProperty("TerrainCollider");
            sp_SafeBorders = serializedObject.FindProperty("SafeBorders");

            TGet.UseMultiShape = false;
            TGet.UseObstacleDetection = false;

            visibleOptMethod = false;
            visibleCamRelation = false;
            visibleToOptList = false;
            visibleAddFeatures = false;
        }

        protected override void GUI_SetupFooter()
        {
            EditorGUILayout.PropertyField(sp_SafeBorders);
        }

        protected override void GUI_PreLODLevelsSetup()
        {
            GUI.color = new Color(1f, 1f, 0.3f, .75f);
            GUILayout.BeginVertical(FGUI_Resources.BGInBoxLightStyle);
            GUI.color = c;
            EditorGUILayout.PropertyField(sp_Terrain, true);
            EditorGUILayout.PropertyField(sp_TerrainC, true);
            EditorGUILayout.EndVertical();
        }

        protected override void OnSceneGUI() { }

        protected override void DefaultInspectorStack(Optimizer_Base targetScript, bool endVert = true)
        {
            TerrainOptimizer targetTerr = targetScript as TerrainOptimizer;

            if (Application.isPlaying) GUI.enabled = false;
            EditorGUILayout.BeginVertical(FEditor.FGUI_Inspector.Style(new Color(0.975f, 0.975f, 0.975f, .325f)));

            EditorGUI.indentLevel++;
            DrawSetup = EditorGUILayout.Foldout(DrawSetup, Lang("Optimizer Setup"), true, new GUIStyle(EditorStyles.foldout) { fontStyle = FontStyle.Bold });
            EditorGUI.indentLevel--;

            if (DrawSetup)
            {
                FGUI_Inspector.DrawUILine(new Color(0.5f, 0.5f, 0.5f, 0.5f), 2, 4);
                GUILayout.Space(1f);

                EditorGUILayout.PropertyField(sp_MaxDist);
                targetScript.DetectionRadius = EditorGUILayout.FloatField(new GUIContent("Detection Radius", "Radius for controll spheres placed on terrain, they will define visibility triggering when camera lookin on or away"), targetScript.DetectionRadius);
                targetScript.DetectionRadius = targetTerr.LimitRadius(targetScript.DetectionRadius);
                EditorGUILayout.PropertyField(sp_SafeBorders);
                EditorGUILayout.PropertyField(sp_GizmosAlpha);

                //EditorGUILayout.PropertyField(serializedObject.FindProperty("ToOptimize"), true);

                GUILayout.Space(3f);
            }

            EditorGUILayout.EndVertical();
            if (Application.isPlaying) GUI.enabled = true;

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
        }

        protected override void DrawFadeDurationSlider(Optimizer_Base targetScript) { }

    }

}
