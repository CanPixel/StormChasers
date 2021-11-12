using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;
using com.zibra.liquid.SDFObjects;

namespace com.zibra.liquid.SDFObjects
{
    // SDF Collider template
    [ExecuteInEditMode] // Careful! This makes script execute in edit mode.
                        // Use "EditorApplication.isPlaying" for play mode only check.
                        // Encase this check and "using UnityEditor" in "#if UNITY_EDITOR" preprocessor directive to
                        // prevent build errors
    [DisallowMultipleComponent]
    public class SDFCollider : MonoBehaviour
    {

#if (UNITY_IOS || UNITY_TVOS || UNITY_WEBGL) && !UNITY_EDITOR
        [DllImport("__Internal")]
#else
        [DllImport("ZibraLiquidNative")]
#endif
        protected static extern void SetColliderParameters(IntPtr ColliderParams, int ColliderID);

        protected int ColliderIndex = 0;
        protected List<int> IsInitialized = new List<int>();

        public static readonly List<SDFCollider> AllColliders = new List<SDFCollider>();

        [StructLayout(LayoutKind.Sequential)]
        public class ColliderParams
        {
            public Vector4 Rotation;
            public Vector3 BBoxMin;
            public Int32 SDFType;
            public Vector3 BBoxMax;
            public Int32 iteration;
            public Vector3 Position;
            public Int32 VoxelNum;
            public Vector3 Scale;
            public Int32 OpType;
            public Int32 Elements;
            public Int32 CurrentID;
            public Int32 colliderIndex;
        };

        protected ColliderParams colliderParams;
        protected IntPtr NativeDataPtr;

        public enum CalculationType
        {
            None,
            Unite,
            Intersect,
            Substract
        }

#if ZIBRA_LIQUID_PAID_VERSION
        public bool ForceInteraction;
#endif

        protected void OnEnable()
        {
            if (!AllColliders?.Contains(this) ?? false)
            {
                AllColliders.Add(this);
            }
        }

        protected void OnDisable()
        {
            if (AllColliders?.Contains(this) ?? false)
            {
                AllColliders.Remove(this);
            }
        }

        /*Compute the SDF and unite it with the SDF in Output*/
        public virtual void ComputeSDF_Unite(int InstanceID, CommandBuffer SolverCommandBuffer,
                                             ComputeBuffer Coordinates, ComputeBuffer Output, ComputeBuffer OutputID,
                                             int ID, int Elements)
        {
        }

        public virtual int GetMemoryFootrpint()
        {
            return 0;
        }
    }
}
