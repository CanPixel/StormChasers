using FIMSpace.FEditor;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.FOptimizing
{
    [CustomEditor(typeof(OptimizersManager))]
    public partial class Optimizer_EditorManager : Editor
    {

        #region GUI Helpers

        private OptimizersManager Get { get { if (_get == null) _get = target as OptimizersManager; return _get; } }
        private OptimizersManager _get;
        private Color c;
        //protected virtual string TargetName() { return "Optimizer"; }
        //protected virtual string TargetTooltip() { return ""; }

        #endregion


        void OnEnable()
        {
            OptimizersManager targetScript = (OptimizersManager)target;
            targetScript.OnValidate();
            targetScript.SetGet();
            zeroOff = new RectOffset(0, 0, 0, 0);
        }


        private static bool loadTry = false;
        RectOffset zeroOff;
        float bgAlpha = 0.05f;


        public override void OnInspectorGUI()
        {
            GUILayout.Space(5f);

            #region Preparing

            OptimizersManager targetScript = (OptimizersManager)target;
            c = GUI.color;

#if UNITY_2019_3_OR_NEWER
            bgAlpha = 0.05f; if (EditorGUIUtility.isProSkin) bgAlpha = 0.1f;
#else
            float bgAlpha = 0.05f; if (EditorGUIUtility.isProSkin) bgAlpha = 0.2f;
#endif

            #endregion


            //GUI.enabled = false;
            //UnityEditor.EditorGUILayout.ObjectField("Script", UnityEditor.MonoScript.FromMonoBehaviour(targetScript), typeof(FOptimizers_Manager), false);
            //GUI.enabled = true;

            Foldout_DrawSetup();

            GUILayout.BeginVertical(FGUI_Inspector.Style(zeroOff, zeroOff, new Color(1f, 1f, .4f, bgAlpha * 0.8f), Vector4.one * 2, 1));
            Foldout_DrawDynamicOpt();
            GUILayout.EndVertical();

            GUILayout.Space(3f);
            EditorGUILayout.HelpBox("This manager component is supporting work of dynamic objects optimization and handling smooth transitions between LOD levels.", UnityEditor.MessageType.Info);
            serializedObject.ApplyModifiedProperties();

            Foldout_DrawTools();

            GUILayout.Space(3f);
            GUILayout.BeginVertical(FGUI_Inspector.Style(zeroOff, zeroOff, new Color(.5f, 1f, .83f, bgAlpha * 0.6f), Vector4.one * 1, 1));
            Foldout_DrawDebug();
            GUILayout.EndVertical();

            GUILayout.Space(5f);
        }


        int GetSceneToolsCount()
        {
            int count = 0;

            if (Get.TargetCamera != null)
            {
                float finDistance = GetFinDistance();
                float targetFogDensity = CalculateTargetFogDensity(finDistance, RenderSettings.fogMode);

                if (Get.TargetCamera.farClipPlane != GetFinDistance()) count++;

                if (RenderSettings.fog)
                {
                    if (RenderSettings.fogMode == FogMode.Linear)
                    {
                        if (Mathf.Round(RenderSettings.fogEndDistance) != Mathf.Round(finDistance * 0.965f)) count++;
                    }
                    else
                        if (RenderSettings.fogDensity != targetFogDensity) count++;
                }
                else
                    count++;
            }

            return count;
        }

        float GetFinDistance()
        {
            if (Get.Distances == null) return 1000f;
            if (Get.Distances.Length < 3) return 1000;
            return Get.Distances[Get.Distances.Length - 1] + Get.Distances[Get.Distances.Length - 2];
        }

        public static float CalculateTargetFogDensity(float maxDistance, FogMode mode)
        {
            float targetFogDensity = RenderSettings.fogDensity;

            if (mode == FogMode.Exponential)
                targetFogDensity = Mathf.Pow(maxDistance, -0.705899f);
            else
                if (mode == FogMode.ExponentialSquared)
                targetFogDensity = Mathf.Pow(maxDistance, -0.841899f);

            if (maxDistance > 1000)
            {
                float prog = Mathf.InverseLerp(400f, 10000f, maxDistance);

                if (mode == FogMode.ExponentialSquared)
                {
                    targetFogDensity *= Mathf.Lerp(1f, 0.6f, 1f - Mathf.Pow(2f, -6f * prog));

                    float prog2 = Mathf.InverseLerp(500f, 5000f, maxDistance);
                    targetFogDensity *= Mathf.Lerp(1f, 0.9f, prog2);
                }
                else
                {
                    targetFogDensity *= Mathf.Lerp(1f, 0.5f, 1f - Mathf.Pow(2f, -7.5f * prog));

                    float prog2 = Mathf.InverseLerp(500f, 4000f, maxDistance);
                    targetFogDensity *= Mathf.Lerp(1f, 0.9f, prog2);
                }
            }

            return targetFogDensity;
        }

        public static void SyncFog(float maxDistance)
        {
            RenderSettings.fogDensity = CalculateTargetFogDensity(maxDistance, RenderSettings.fogMode);
            RenderSettings.fogEndDistance = maxDistance * 0.965f;
        }

    }

}
