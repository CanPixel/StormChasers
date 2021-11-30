using FIMSpace.FEditor;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.FOptimizing
{
    [CustomEditor(typeof(Optimizer_Base))]
    [CanEditMultipleObjects]
    public partial class Optimizer_BaseEditor : Editor
    {

        #region GUI Helpers

        private Optimizer_Base Get { get { if (_get == null) _get = target as Optimizer_Base; return _get; } }
        private Optimizer_Base _get;
        protected Color c;
        protected virtual string TargetName() { return "Optimizer"; }
        protected virtual string TargetTooltip() { return ""; }

        #endregion

        public override bool RequiresConstantRepaint()
        {
            return Application.isPlaying;
        }

        public override void OnInspectorGUI()
        {
            Undo.RecordObject(target, "Optimizers Inspector");

            if (Get.UseObstacleDetection)
                drawCullIfNotSee = false;
            else
                drawCullIfNotSee = true;

            if (serializedObject != null) serializedObject.Update();

            #region Unity Version Specifics

#if UNITY_2020_1_OR_NEWER
            if (target == null) return;
            if (Get == null) return;
#endif

            #endregion

            #region Corrections Checking

            if (Get.OptimizingMethod == EOptimizingMethod.TriggerBased) DrawAddRigidbodyToCamera();

            #endregion


            LODFrame(Get); // Drawing coloured LOD frame to identify LOD state in playmode

            HeaderBoxMain(new GUIContent(TargetName(), TargetTooltip()), ref Get.DrawGizmos, ref drawDefaultInspector, GetSmallIcon(), Get, 27);

            if (drawDefaultInspector)
                DrawDefaultInspector();
            else
            {
                DrawNewGUI();

                //if (target != null) // Unity 2020 have problems with 'target'
                //    if (Application.isPlaying) EditorUtility.SetDirty(target);

                if (serializedObject != null) // Unity 2020 prevention
                {
                    serializedObject.ApplyModifiedProperties();
                }
            }

            if (Application.isPlaying) EditorGUILayout.EndVertical(); // LODFrame End
        }


        void DrawNewGUI()
        {

            #region Preparations for unity versions and skin

            c = GUI.color;

            RectOffset zeroOff = new RectOffset(0, 0, 0, 0);

#if UNITY_2019_3_OR_NEWER
            float bgAlpha = 0.05f; if (EditorGUIUtility.isProSkin) bgAlpha = 0.1f;
            int headerHeight = 22;
#else
            float bgAlpha = 0.05f; if (EditorGUIUtility.isProSkin) bgAlpha = 0.2f;
            int headerHeight = 22;
#endif


            #endregion


            GUILayout.BeginVertical(FGUI_Resources.BGBoxStyle); GUILayout.Space(1f);


            GUILayout.BeginVertical(FGUI_Inspector.Style(zeroOff, zeroOff, new Color(.7f, .7f, 0.7f, bgAlpha), Vector4.one * 3, 3));

            FGUI_Inspector.HeaderBox(ref drawOptimSetup, Lang("Optimizer Setup"), true, FGUI_Resources.Tex_GearSetup, headerHeight, headerHeight - 1, LangBig());
            if (drawOptimSetup) Tab_DrawOptimSetup();

            GUILayout.EndVertical();


            if (!Application.isPlaying)
            {
                // ------------------------------------------------------------------------

                if (visibleToOptList)
                {
                    GUILayout.BeginVertical(FGUI_Inspector.Style(zeroOff, zeroOff, new Color(1f, 1f, .4f, bgAlpha * 0.8f), Vector4.one * 3, 3));
                    FGUI_Inspector.HeaderBox(ref drawToOptList, Lang("To Optimize List") + " (" + Get.GetToOptimizeCount() + ")", true, FGUI_Resources.Tex_Sliders, headerHeight, headerHeight - 1, LangBig());

                    if (drawToOptList) Tab_DrawToOptList();

                    GUILayout.EndVertical();
                }


                // ------------------------------------------------------------------------

                if (visibleAddFeatures)
                {
                    GUILayout.BeginVertical(FGUI_Inspector.Style(zeroOff, zeroOff, new Color(.5f, 1f, .83f, bgAlpha * 0.6f), Vector4.one * 3, 3));
                    FGUI_Inspector.HeaderBox(ref drawAdditionalModules, Lang("Additional Features"), true, FGUI_Resources.Tex_Module, headerHeight, headerHeight - 1, LangBig());

                    if (drawAdditionalModules) Tab_DrawAdditionalModules();

                    GUILayout.EndVertical();
                }

                GUI_PreLODLevelsSetup();
            }

            // ------------------------------------------------------------------------


            //if ( serializedObject != null ) serializedObject.ApplyModifiedProperties();
            //Get.EditorUpdate();

            string title = "";
            if (Application.isPlaying) title = Lang("Realtime Preview"); else title = Lang("LOD Levels Setup");

            GUILayout.BeginVertical(FGUI_Inspector.Style(zeroOff, zeroOff, new Color(.6f, .95f, .55f, bgAlpha * 0.375f), Vector4.one * 3, 3));
            FGUI_Inspector.HeaderBox(ref drawLODSetup, title, true, FGUI_Resources.Tex_Repair, headerHeight, headerHeight - 1, LangBig());

            if (drawLODSetup) Tab_DrawLODSetup();

            GUILayout.EndVertical();

            GUILayout.Space(2f);
            GUILayout.EndVertical();

            // ------------------------------------------------------------------------

            if (drawNothingToOptimizeWarning)
                if (Get.GetToOptimizeCount() == 0)
                {
                    string childrenInfo = "";
                    if (Get.gameObject.transform.childCount > 0) childrenInfo = " Maybe there are components to optimize in child game objects? Please check buttons inside 'To Optimize' tab.";
                    EditorGUILayout.HelpBox("Nothing to optimize! You can only cull game object with the component." + childrenInfo, MessageType.Info);
                }
        }

        protected virtual void GUI_PreLODLevelsSetup()
        {

        }
    }



}

