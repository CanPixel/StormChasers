using UnityEditor;
using UnityEngine;

namespace FIMSpace.FOptimizing
{
    public partial class Optimizer_BaseEditor
    {
        private static bool drawDefaultInspector = false;

        public static bool DrawSetup = true;
        public static bool DrawToOptimize = false;
        public static bool ClickedOnSlider = false;
        public static bool DrawAddCompOptions = false;

        private float distance = 0f;

        protected int selectedLOD = -1;
        protected bool drawHiddenRange = true;
        protected int selectedLODSlider = 0;
        private readonly int sliderControlId = "LODSliderIDHash".GetHashCode();

        protected SerializedProperty sp_LodLevels;
        protected SerializedProperty sp_FadeDuration;
        protected SerializedProperty sp_FadeVisibility;
        protected SerializedProperty sp_MaxDist;
        protected SerializedProperty sp_CullIfNotSee;
        protected SerializedProperty sp_DetectionRadius;
        protected SerializedProperty sp_GizmosAlpha;
        protected SerializedProperty sp_UnlockFirstLOD;
        protected SerializedProperty sp_AutoDistanceFactor;

        protected SerializedProperty sp_OptMeth;
        protected SerializedProperty sp_DetRad;
        protected SerializedProperty sp_DetBounds;
        protected SerializedProperty sp_DetOffs;
        protected SerializedProperty sp_AddToCont;


        private SerializedProperty sp_CoveragePrecision;
        private SerializedProperty sp_CoverageMask;
        //private SerializedProperty sp_MemoryTolerance;
        private SerializedProperty sp_CoverageScale;
        private SerializedProperty sp_CustomRayPoints;
        //private SerializedProperty sp_coverageOffsets;

        private SerializedProperty sp_AutoPrecision;
        private SerializedProperty sp_AutoReferenceMesh;
        private SerializedProperty sp_DrawPositionHandles;
        private SerializedProperty sp_ScalingHandles;
        //private SerializedProperty sp_shapes;

        public static bool DrawDetectionSetup = false;


        private static bool drawDetectionSetup = true;


        protected bool drawDetectionRadius = true;
        protected bool drawNothingToOptimizeWarning = true;
        protected bool drawCullIfNotSee = true;
        protected bool drawDetectionOffset = true;
        protected bool drawHideable = true;
        protected bool drawDetectionSphereHandle = true;

        protected static Color individualColor = new Color(0.725f, 0.85f, 1f, 0.9f);
        protected static Color preCol;
        private bool isRunningInEditor = false;
        private bool isPrefabInEditor = false;

        protected virtual void OnEnable()
        {
            SetupLangs();

            sp_LodLevels = serializedObject.FindProperty("LODLevels");
            sp_FadeDuration = serializedObject.FindProperty("FadeDuration");
            sp_FadeVisibility = serializedObject.FindProperty("FadeViewVisibility");

            sp_MaxDist = serializedObject.FindProperty("MaxDistance");
            sp_CullIfNotSee = serializedObject.FindProperty("CullIfNotSee");
            sp_DetectionRadius = serializedObject.FindProperty("DetectionRadius");
            sp_GizmosAlpha = serializedObject.FindProperty("GizmosAlpha");
            sp_UnlockFirstLOD = serializedObject.FindProperty("UnlockFirstLOD");
            sp_AutoDistanceFactor = serializedObject.FindProperty("AutoDistanceFactor");

            sp_OptMeth = serializedObject.FindProperty("OptimizingMethod");
            sp_DetRad = serializedObject.FindProperty("DetectionRadius");
            sp_DetBounds = serializedObject.FindProperty("DetectionBounds");
            sp_DetOffs = serializedObject.FindProperty("DetectionOffset");
            sp_AddToCont = serializedObject.FindProperty("AddToContainer");

            if (target) // Unity 2020 have problem with that
            {
                Optimizer_Base Get = (Optimizer_Base)target;
                if (Get.GetToOptimizeCount() == 0) DrawAddCompOptions = true;

                if (Application.isPlaying)
                {
                    DrawSetup = false;
                    DrawToOptimize = false;
                    drawAdditionalModules = false;
                    drawOptimSetup = false;
                }

                Get.SyncWithReferences();
                Get.EditorResetLODValues();

                if (Get.GetToOptimizeCount() > 2)
                {
                    DrawHideProperties();
                }

                isRunningInEditor = Application.isEditor && !Application.isPlaying;


                Get.Editor_InIsolatedScene = Get.gameObject.scene.rootCount == 1;
                if (Get.gameObject.scene.rootCount >= 1) isPrefabInEditor = false; else isPrefabInEditor = true;

                if (isPrefabInEditor)
                {
                    if (Get.Editor_JustCreated)
                    {

                        //if (target != null) // Unity 2020 have problem with that
                        //    EditorUtility.SetDirty(target);

                        Get.Editor_JustCreated = false;
                    }
                }

                //        Get.CheckForNullsToOptimize();

                OnStartGenerateProperties();
            }


            if (drawNothingToOptimizeWarning)

                preCol = GUI.color;

            sp_CoveragePrecision = serializedObject.FindProperty("CoveragePrecision");
            sp_CoverageMask = serializedObject.FindProperty("CoverageMask");
            sp_CoverageScale = serializedObject.FindProperty("CoverageScale");
            sp_CustomRayPoints = serializedObject.FindProperty("CustomCoveragePoints");
            //sp_coverageOffsets = serializedObject.FindProperty("coverageOffsets");
            //sp_MemoryTolerance = serializedObject.FindProperty("MemoryTolerance");

            //base.OnEnable();
            //    drawCullIfNotSee = false;

            drawCullIfNotSee = false;
            drawDetectionRadius = false;
            drawDetectionSphereHandle = false;
            drawHideable = false;
            drawDetectionOffset = false;
            sp_AutoPrecision = serializedObject.FindProperty("AutoPrecision");
            sp_AutoReferenceMesh = serializedObject.FindProperty("AutoReferenceMesh");
            sp_DrawPositionHandles = serializedObject.FindProperty("DrawPositionHandles");
            sp_ScalingHandles = serializedObject.FindProperty("ScalingHandles");
            //sp_shapes = serializedObject.FindProperty("Shapes");

            if (Get.Shapes == null || Get.Shapes.Count == 0) DrawDetectionSetup = true;

        }


        protected virtual void OnStartGenerateProperties() { }
        protected virtual void DrawHideProperties() { }
        protected virtual void OnJustCreated() { }

        void OnDestroy()
        {
            if (Application.isEditor && !Application.isPlaying && (Optimizer_Base)target == null)
            {
                if (isRunningInEditor)
                {
                    Optimizer_Base Get = (Optimizer_Base)target;
                    if (isPrefabInEditor) Get.CleanAsset();
                }
            }

            Optimizer_Base opt = (Optimizer_Base)target;
            if (opt) opt.Editor_InIsolatedScene = false;
        }


        protected void ScriptField(Optimizer_Base Get)
        {
            GUI.enabled = false;
            UnityEditor.EditorGUILayout.ObjectField("Script", UnityEditor.MonoScript.FromMonoBehaviour(Get), typeof(Optimizer_Base), false);
            GUI.enabled = true;
        }

    }

}

