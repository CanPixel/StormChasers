using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.FOptimizing
{
    public partial class OptimizersManager
    {
        public Plane[] CurrentFrustumPlanes { get; private set; }

        [Header("Dynamic Optimization Parameters")]
        public bool Advanced = false;
        public bool Debugging = false;
        [Tooltip("If camera is not moving or not rotating there will be ignored some of calculations")]
        public bool DetectCameraFreeze = false;
        //[Tooltip("Optimizing count of needed raycasts for obstacle detecions")]
        //public bool RaycastMemory = false;

        internal static int RaycastsInThisFrame = 0;
        internal static int HiddenObjects = 0;

        [Tooltip("When you adding this component, algorithm is adapting this value as MainCamera Far Clipping planes are setted*\n\nAutomatic optimization distance values basing on main character size - Check human scale gizmo in scene view next to camera (It can need other adjustement anyway - depends of project needs)")]
        public float WorldScale = 2.0f;

        [Tooltip("What amount of units should move camera/optimized object in previous frame to trigger checking LOD state (if camera and object doesn't move checking LOD state will be ignored - optimization for system)")]
        public float MoveTreshold = 0.0f;

        [Tooltip("If you want to object checking be even quicker (in some cases can affect a little performance but will reponse much quicker)")]
        [Range(0f, 1f)]
        public float UpdateBoost = 0.0f;

        [Tooltip("You can define in which distances optimized objects should be prioritized lower for checking LOD state")]
        public float[] Distances; // Exposed for advanced setup

        //private Quaternion previousCameraRotationMoveTrigger;

        private Optimizers_DynamicClock[] clocks;

        private long totalTimeConsumption = 0;

        void GenerateClocks()
        {
            if (clocks != null) return;

            clocks = new Optimizers_DynamicClock[GetDistanceTypesCount()];

            for (int i = 0; i < clocks.Length; i++)
            {
                dynamicLists.Add(new List<Optimizer_Base>());
                clocks[i] = new Optimizers_DynamicClock(this, (EOptimizingDistance)i, dynamicLists[i]);
            }
        }

        void RunDynamicClocks()
        {
            StartCoroutine(InitialCall());

            // Starting dynamic clocks
            for (int i = 0; i < clocks.Length; i++)
                StartCoroutine(clocks[i].WatchUpdate());
        }

        void DynamicUpdate()
        {
            RaycastsInThisFrame = 0;

#if UNITY_2017_4_OR_NEWER
            GeometryUtility.CalculateFrustumPlanes(MainCamera, CurrentFrustumPlanes);
#else
            CurrentFrustumPlanes = GeometryUtility.CalculateFrustumPlanes(MainCamera);
#endif

            totalTimeConsumption = 0;
            for (int i = 0; i < clocks.Length; i++)
                totalTimeConsumption += clocks[i].FrameTicksConsumption;
        }


        /// <summary>
        /// To call when doing camera teleports or fast travels in game
        /// </summary>
        public static void CallUpdateAll()
        {
            if (MainCamera)
                for (int i = 0; i < Instance.dynamicLists.Count; i++)
                {
                    for (int c = Instance.dynamicLists[i].Count - 1; c >= 0; c--)
                        Instance.CheckElement(Instance.dynamicLists[i][c], c, false);
                }
        }


        /// <summary>
        /// Adding optimizer component to dynamic update list
        /// </summary>
        public int AddToDynamic(Optimizer_Base optimizer)
        {
            float dist = float.MaxValue;
            if (MainCamera) dist = (optimizer.GetReferencePosition() - MainCamera.transform.position).magnitude;

            EOptimizingDistance category = QualifyDistance(dist);

            int index = -1;
            if (optimizer.CurrentDynamicDistanceCategory != category)
            {
                if (optimizer.CurrentDynamicDistanceCategory != null)
                    dynamicLists[(int)optimizer.CurrentDynamicDistanceCategory].RemoveAt(optimizer.DynamicListIndex);

                dynamicLists[(int)category].Add(optimizer);

                index = dynamicLists[(int)category].Count;
            }
            else
            {
                // Optimizer already existing in lists
                return optimizer.DynamicListIndex;
            }

            if (MainCamera) optimizer.DynamicLODUpdate(category, dist);

            return index;
        }


        /// <summary>
        /// Removing optimizer component from dynamic update list (destroyed?)
        /// </summary>
        public void RemoveFromDynamic(Optimizer_Base optimizer)
        {
            if (optimizer.CurrentDynamicDistanceCategory != null)
                dynamicLists[(int)optimizer.CurrentDynamicDistanceCategory].Remove(optimizer);
        }


        /// <summary>
        /// Checking how far is optimizer or if camera looking away then applying LOD level or culling
        /// </summary>
        public void CheckElement(Optimizer_Base optimizer, int index, bool full = true)
        {
            // Movement / camera rotation treshold optimization
            if (full)
            {
                if (optimizer.TresholdTrigger() == false)
                {
                    return;
                }
            }

            // Qualify current distance of object
            float distance = Vector3.Distance(optimizer.TargetCamera.position, optimizer.GetReferencePosition());
            EOptimizingDistance targetClock = QualifyDistance(distance);

            if (targetClock != optimizer.CurrentDynamicDistanceCategory)
            {
                if (optimizer.CurrentDynamicDistanceCategory != null)
                    dynamicLists[(int)optimizer.CurrentDynamicDistanceCategory].RemoveAt(index);

                dynamicLists[(int)targetClock].Add(optimizer);
            }

            // Updating Optimizer
            optimizer.DynamicLODUpdate(targetClock, distance);
        }


#region Utilities


        private IEnumerator InitialCall()
        {
            yield return null;
            CallUpdateAll();
        }

        /// <summary>
        /// Determinate which distance level catch object from camera position
        /// </summary>
        private EOptimizingDistance QualifyDistance(float distance)
        {
            for (int i = 0; i < Distances.Length; i++)
                if (distance < Distances[i]) return (EOptimizingDistance)i;

            return EOptimizingDistance.Farthest;
        }


        /// <summary>
        /// Refreshing dynamic range distances for manager
        /// </summary>
        public void RefreshDistances()
        {
            if (Advanced)
            {
                if (Distances != null)
                {
                    for (int i = 1; i < Distances.Length; i++)
                        if (Distances[i] < Distances[i - 1] * 1.05f) Distances[i] = Distances[i - 1] * 1.2f;
                }
                else
                    Distances = new float[GetDistanceTypesCount() - 1];

            }
            else
            {
                Distances = new float[GetDistanceTypesCount() - 1];
                for (int i = 0; i < Distances.Length; i++)
                {
                    Distances[i] = Mathf.Lerp(60f * WorldScale, 750f * WorldScale, (float)i / (float)Distances.Length);
                }
            }
        }


        public bool CameraMoved(Vector3 prePos, Quaternion preRot)
        {
            if (!DetectCameraFreeze) return true;

#if UNITY_EDITOR
            if (Time.frameCount < 120) return true;
#endif

            bool cameraMoved = false;

            Vector3 diff = MainCamera.transform.position - prePos;

            if (diff.magnitude > Mathf.Max(0.000001f, Instance.MoveTreshold))
                cameraMoved = true;
            else
                cameraMoved = false;

            if (!cameraMoved)
                if (Quaternion.Angle(preRot, MainCamera.transform.rotation) > 0.1f)
                    cameraMoved = true;

            return cameraMoved;
        }

#endregion
    }
}
