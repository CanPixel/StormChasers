using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.FOptimizing
{
    /// <summary>
    /// FM: Manager class to support optimizng objects without CullingGroups api or support Effective technique.
    /// This calss also handles transitioning feature for LOD levels.
    /// </summary>
    [AddComponentMenu("FImpossible Creations/Optimizers 2/Optimizers Manager", 11)]
    [DefaultExecutionOrder(1001)]
    public partial class OptimizersManager : MonoBehaviour, UnityEngine.EventSystems.IDropHandler, IFHierarchyIcon
    {
        [Tooltip("(DontDestroyOnLoad - untoggled just for package examples purpose!)\n\nWith this option enabled, manager will be never destroyed, even during changing scenes. This one manager can be used as only manager in whole game time")]
        //[FPD_Width(135)]
        public bool ExistThroughScenes = true;

        [Tooltip("You should use as many as you can optimizers with same LOD distances and LODs counts to get best from culling containers.\n\nThis number defines how many slots should pre define each container for target optimizers components.\n\nWhen you use many components with different LOD counts or different LOD distance settings and there is only few (for example 200) objects to optimize in each distance range / lod count set you should change this number to be lower to not prepare too much slots for target optimizers (tiny bit higer RAM usage if capacity size is too big)")]
        public int SingleContainerCapacity = 300;

        [Tooltip("Drawing Human Size Reference sprite in scene view next to manager's position")]
        public bool DrawHumanSizeRefIcon = false;

        //[Tooltip("Put here LOD file for certain type of component, then when you drag your component to Optimizer box under 'To Optimize' / 'Assigning new components tab' there will not be added MonoBehaviour LOD settings but your custom one.\n\n(LOD must implement CheckForComponent() method)")]
        //public FLOD_Base[] CustomComponentsDefinition;


        #region List of built in references

        //[HideInInspector]
        //public FLOD_Light LightReference;
        //[HideInInspector]
        //public FLOD_AudioSource AudioSourceReference;
        //[HideInInspector]
        //public FLOD_MonoBehaviour MonoBehReference;
        //[HideInInspector]
        //public FLOD_NavMeshAgent NavMeshAgentReference;
        //[HideInInspector]
        //public FLOD_ParticleSystem ParticleSystemReference;
        //[HideInInspector]
        //public FLOD_Renderer RendererReference;
        //[HideInInspector]
        //public FLOD_Terrain TerrainReference;

        #endregion


        #region Static Flags

        public static bool DrawGizmos = false;

        #endregion


        #region Get manager stuff and initialization

        #region Construction

        public string EditorIconPath { get { if (PlayerPrefs.GetInt("OptH", 1) == 0) return ""; else return "FIMSpace/Optimizers 2/OptManagerIconSmall"; } }
        public void OnDrop(UnityEngine.EventSystems.PointerEventData data) { }

        private static OptimizersManager _get;
        public static OptimizersManager Instance
        {
            get
            {
                if (_get == null) GenerateOptimizersManager();
                if (_get == null) return FindObjectOfType<OptimizersManager>();
                return _get;
            }
            private set { _get = value; }
        }

        #endregion

        public static bool Exists
        {
            get
            {
                if (_get == null) { OptimizersManager man = FindObjectOfType<OptimizersManager>(); man.SetGet(); }
                return _get != null;
            }
        }

        [Tooltip("Main rendering camera reference")]
        public Camera TargetCamera;
        [Tooltip("If you use VR SDK or some auto main camera creation/assign logics, increase this value to for example 3 to let engine do main camera calculations and then assign new camera for optimizers automatically")]
        public int GetCameraAfter = 0;
        private Camera _lastcamera = null;
        private static Camera _mainCam;
        public static Camera MainCamera
        {
            get { if (_mainCam == null) GetMainCamera(); return _mainCam; }
            private set { _mainCam = value; }
        }

        private Vector3 previousCameraPositionMoveTrigger;

        /// <summary> Lists for each FEOptimizingDistance level </summary>
        private List<List<Optimizer_Base>> dynamicLists;

        private static void GenerateOptimizersManager()
        {
            //if (!Application.isPlaying) return;
            OptimizersManager manager = FindObjectOfType<OptimizersManager>();

            if (!manager)
            {
                GameObject managerObject = new GameObject("Generated Optimizers Manager");
                managerObject.transform.SetAsFirstSibling();
                manager = managerObject.AddComponent<OptimizersManager>();
            }

            _get = manager;
            Instance = manager;
            Instance.Init();
        }

        private static void GetMainCamera()
        {
            Camera preCam = _mainCam;
            Camera mainCamera = Camera.main;

            if (mainCamera == null)
            {
                mainCamera = FindObjectOfType<Camera>();

                if (mainCamera)
                    Debug.LogWarning("[OPTIMIZERS] There is no object with 'MainCamera' Tag!");
                else
                    if (FEditor_OneShotLog.CanDrawLog("OptNoCamera", 10)) Debug.LogWarning("[OPTIMIZERS] There is no camera on the scene!");
            }

            _mainCam = mainCamera;
            Instance.TargetCamera = mainCamera;

            if (!preCam != mainCamera) SetNewMainCamera(mainCamera);
        }

        public void SetGet()
        {
            OptimizersManager manager = FindObjectOfType<OptimizersManager>();
            bool destroyed = false;
            if (manager) if (manager != this)
                {

                    if (Application.isPlaying)
                    {
                        Debug.LogWarning("[OPTIMIZERS] There can't be two Optimizers Managers at the same time! I'm removing new one!");
                        Destroy(this);
                        destroyed = true;
                    }
                    else
                    {
                        Debug.LogWarning("[OPTIMIZERS EDITOR] There can't be two Optimizers Managers at the same time! I'm removing previous one!");
                        DestroyImmediate(manager);
                        destroyed = true;
                    }
                }

            if (!destroyed)
            {
                if (_get != null) if (_get != this)
                    {

                        if (Application.isPlaying)
                        {
                            Debug.LogWarning("[OPTIMIZERS] There can't be two Optimizers Managers at the same time! I'm removing new one!");
                            Destroy(this);
                        }
                        else
                        {
                            Debug.LogWarning("[OPTIMIZERS EDITOR] There can't be two Optimizers Managers at the same time! I'm removing previous one!");
                            DestroyImmediate(_get);
                        }

                        return;
                    }
            }
            else
                return;

            Instance = this;
        }

        private bool existThroughScenes = false;
        private bool initialized = false;

        #region Utilities


        /// <summary>
        /// Setting new camera as main for optimizers system.
        /// Method will refresh target camera for all existing optimizers components on scene.
        /// </summary>
        public static void SetNewMainCamera(Camera camera)
        {
            if (camera == null) return;

            MainCamera = camera;
            Instance._lastcamera = MainCamera;

            //foreach (FOptimizer_Base optim in FindObjectsOfType<FOptimizer_Base>()) optim.RefreshCamera(camera);
            foreach (Optimizer_Base optim in Instance.notContainedStaticOptimizers) optim.RefreshCamera(camera);
            foreach (Optimizer_Base optim in Instance.notContainedDynamicOptimizers) optim.RefreshCamera(camera);
            foreach (Optimizer_Base optim in Instance.notContainedEffectiveOptimizers) optim.RefreshCamera(camera);
            foreach (Optimizer_Base optim in Instance.notContainedTriggerOptimizers) optim.RefreshCamera(camera);

            SetNewMainCameraForContainers(camera);
        }


        /// <summary>
        /// Setting new camera only for objects added to culling containers (much quicker, execute if you use only static/dynamic/effective method and basic/detection optimizer)
        /// </summary>
        public static void SetNewMainCameraForContainers(Camera camera)
        {
            MainCamera = camera;

            if (Instance.CullingContainersIDSpecific != null)
                foreach (var item in Instance.CullingContainersIDSpecific)
                {
                    for (int i = 0; i < item.Value.Count; i++)
                    {
                        item.Value[i].SetNewCamera(camera);
                    }
                }

            Instance.InitCameraFrustum();
        }


        /// <summary>
        /// If you need to turn off/on optimizers works.
        /// </summary>
        public static void SwitchOptimizersOnOrOff(bool on = true, bool unhideAll = true)
        {
            if (Instance)
            {
                Instance.enabled = on;

                if (unhideAll)
                {
                    foreach (Optimizer_Base optim in FindObjectsOfType<Optimizer_Base>())
                    {
                        if (optim.CullingGroup != null) optim.CullingGroup.enabled = on;
                        optim.SetLODLevel(0);
                    }
                }
                else
                {
                    foreach (Optimizer_Base optim in FindObjectsOfType<Optimizer_Base>())
                    {
                        if (optim.CullingGroup != null) optim.CullingGroup.enabled = on;
                    }
                }
            }
        }


        private static int GetDistanceTypesCount()
        {
            return System.Enum.GetValues(typeof(EOptimizingDistance)).Length;
        }


        #endregion


        #endregion


        private void Awake() { AppIsQuitting = false; if (!Application.isPlaying) { SetGet(); return; } Init(); }

        private void Start() { Init(); if (GetCameraAfter > 0) StartCoroutine(CGetCamera(GetCameraAfter)); }

        private void Reset()
        {
            GetMainCamera();
            if (MainCamera) WorldScale = (float)System.Math.Round(MainCamera.farClipPlane / 520f, 2);
        }

        public void Init()
        {
            if (initialized) return;

            SetGet();

            if (Application.isPlaying)
            {
                if (ExistThroughScenes) { DontDestroyOnLoad(gameObject); existThroughScenes = true; }

                dynamicLists = new List<List<Optimizer_Base>>();
                CullingContainersIDSpecific = new Dictionary<int, Optimizers_CullingContainersList>();
                //if (MainCamera) previousCameraRotationMoveTrigger = MainCamera.transform.rotation; else previousCameraRotationMoveTrigger = Quaternion.identity;

                initialized = true;

                GenerateClocks();
                RefreshDistances();
                RunDynamicClocks();
            }

            InitCameraFrustum();
            AppIsQuitting = false;

        }

        private void InitCameraFrustum()
        {
            if (MainCamera)
                Instance.CurrentFrustumPlanes = GeometryUtility.CalculateFrustumPlanes(MainCamera);
            else
                Instance.CurrentFrustumPlanes = new Plane[6];
        }

        private void LateUpdate()
        {
            if (!existThroughScenes) if (ExistThroughScenes) DontDestroyOnLoad(gameObject);

            if (TargetCamera == null)
            {
                GetMainCamera();
                SetNewMainCamera(TargetCamera);
                if (TargetCamera != null) Debug.Log("[OPTIMIZERS] New Camera detected and assigned! " + TargetCamera.name);
            }
            else
            {
                if (TargetCamera != MainCamera)
                {
                    SetNewMainCamera(TargetCamera);
                    Debug.Log("[OPTIMIZERS] New Camera detected and assigned! " + TargetCamera.name);
                }

                TransitionsUpdate();
                DynamicUpdate();
            }
        }


        public void OnValidate()
        {
            if (_lastcamera != TargetCamera) SetNewMainCamera(TargetCamera);
            if (TargetCamera != null) if (TargetCamera != MainCamera) MainCamera = TargetCamera;

            if (WorldScale <= 0f) WorldScale = 0.1f;
            if (!Advanced) MoveTreshold = WorldScale / (150f * (1f + UpdateBoost));
            RefreshDistances();
            if (!Advanced) Debugging = false;
            if (GetCameraAfter < 0) GetCameraAfter = 0;

            TargetCamera = MainCamera;

            if (SingleContainerCapacity < 25) SingleContainerCapacity = 25;
            if (SingleContainerCapacity > 10000) SingleContainerCapacity = 10000;
        }

        /// <summary> Wait for main camera with some frames delay </summary>
        private IEnumerator CGetCamera(int framesDelay)
        {
            int elapsed = 0;
            while (elapsed < framesDelay) { yield return null; elapsed++; }
            GetMainCamera();
            yield break;
        }


        public static bool AppIsQuitting = false;
        void OnApplicationQuit()
        {
            AppIsQuitting = true;
        }

        // Rest of the code inside partial classes
    }

}