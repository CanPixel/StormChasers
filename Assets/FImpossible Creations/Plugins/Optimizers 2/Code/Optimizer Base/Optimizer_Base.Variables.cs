using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.FOptimizing
{
    public abstract partial class Optimizer_Base
    {
        public virtual int GetToOptimizeCount() { return 0; }


        #region Culling Detection Public Variables

        [Range(1, 8)]
        [Tooltip("Level of detail (LOD) steps to configure optimization levels")]
        public int LODLevels = 2;

        [SerializeField]
        [HideInInspector]
        protected int preLODLevels = 1;

        [Tooltip("Max distance from main camera.\nWhen exceed object will be culled")]
        public float MaxDistance = 100f;

        [Tooltip("[Static] - For models which aren't moving far from initial position or just stays in one place (method is using only CullingGroups - Very Effective for 'Cull if not see')\n\n[Dynamic] - For objects which are moving in scene's world. If object is moving very fast, use 'UpdateBost' slider in Optimizers Manager but using EFFECTIVE method more recommended in such situtation. Dynamic method can response with some delay when there are thousands of active objects to optimize.\n\n[EFFECTIVE] - Connecting features of static method and dynamic, the most resposible method when you have very mobile objects and you need quick detection if object is seen by camera\n\n[Trigger Based] Using trigger colliders to define distance levels (experimental)")]
        public EOptimizingMethod OptimizingMethod = EOptimizingMethod.Effective;

        [FPD_DrawTexture("FIMSpace/Optimizers 2/Opt_CullHelp", 128, 20, 120, 165)]
        [Tooltip("[Toggled] Changing LOD state to cull (or hidden) if camera is looking away from detection sphere/bounds\n\n[Untoggled] Only max distance will cull this object")]
        public bool CullIfNotSee = true;
        [Tooltip("CullIfNotSee: Radius of detecting object visibility for camera view (frustum - CullingGroups)")]
        public float DetectionRadius = 3f;
        [Tooltip("CullIfNotSee: Bounding Box for detecting object visibility for camera view (frustum)")]
        public Vector3 DetectionBounds = Vector3.one;
        [HideInInspector]
        /// <summary> Set it to true at override void Reset() if you coding custom class and want to have "Hidden" LOD option always visible </summary>
        public bool Hideable = false;

        [Tooltip("Offsetting center of detection sphere/bounds")]
        public Vector3 DetectionOffset = Vector3.zero;

        [Range(0f, 1f)]
        [Tooltip("Alpha for debug spheres etc. visible in scene view when object with Optimizer is selected and Optimizer is unfolded")]
        public float GizmosAlpha = 1f;
        public bool DrawGizmos = true;

        [Range(0f, 3f)]
        [Tooltip("How long (in seconds) should take transition between LOD levels (if transitioning for optimized component is supported)")]
        public float FadeDuration = 0f;
        [Tooltip("If you want to use transition when object goes out of camera view (camera frustum) and not fade just when distance ranges are changed.")]
        public bool FadeViewVisibility = false;

        //[Tooltip("Displaying options to assign shared settings files to components LODs.\n(Untoggling will not disable using shared settings, just viewing them)")]
        //public bool DrawSharedSettingsOptions = false;

        [Tooltip("If at 'Culled' LOD state game object should be deactivated (after transition)\n\nWARNING: Deactivating whole game object is highly time comsuming for unity when you do it on multiple objects during one game frame\nif you use optimizers on many objects and experience lags during rotating camera then try not deactivating game object but just components inside 'To Optimize' list!")]
        public bool DeactivateObject = false;

        #endregion


        #region LOD Range Calculations Variables

        [HideInInspector]
        public List<float> LODPercent;

        /// <summary> Reference position for editor to calculate helper distance from camera to this object </summary>
        protected Vector3 distancePoint = Vector3.zero;

        [HideInInspector]
        public bool AutoDistance = false;
        public float AutoDistanceFactor = 0f;
        [HideInInspector]
        public bool DrawAutoDistanceToggle = true;

        [HideInInspector]
        public int HiddenCullAt = -1;

        [HideInInspector]
        public int LimitLODLevels = 0;

        protected bool drawDetectionSphere = true;
        protected float moveTreshold;

        #endregion


        #region LOD and Cull Variables

        [HideInInspector]
        public bool UnlockFirstLOD = false;

        public bool OutOfDistance { get; protected set; }
        public bool OutOfCameraView { get; protected set; }
        public float[] DistanceLevels { get; protected set; }
        public int CurrentLODLevel { get; protected set; }
        public int CurrentDistanceLODLevel { get; protected set; }
        public bool IsCulled { get; protected set; }
        public bool IsHidden { get; protected set; }
        public bool FarAway { get; protected set; }

        protected bool WasOutOfCameraView;
        protected bool WasHidden;

        protected bool doFirstCull = true; // Initial cull state change - no transitioning flag
        public Transform TargetCamera { get; protected set; }

        #endregion


        #region Transition Info

        public int TransitionNextLOD { get; internal set; }
        public float TransitionPercent { get; internal set; }

        [HideInInspector]
        public bool DrawGeneratedPrefabInfo = false;

        [HideInInspector]
        public bool DrawDeactivateToggle = true;

        #endregion


    }
}
