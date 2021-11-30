using System;
using System.Runtime.InteropServices;

namespace com.zibra.liquid.Solver
{
    public static class ZibraLiquidBridge
    {
#if (UNITY_IOS || UNITY_TVOS || UNITY_WEBGL) && !UNITY_EDITOR
        [DllImport("__Internal")]
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        [DllImport("ZibraFluidNative_Mac_x64")]
#elif UNITY_64 || UNITY_EDITOR_64
        [DllImport("ZibraFluidNative_Win_x64")]
#else
        [DllImport("ZibraFluidNative_Win_x86")]
#endif
        public static extern Int32 CreateFluidInstance();

#if (UNITY_IOS || UNITY_TVOS || UNITY_WEBGL) && !UNITY_EDITOR
        [DllImport("__Internal")]
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        [DllImport("ZibraFluidNative_Mac_x64")]
#elif UNITY_64 || UNITY_EDITOR_64
        [DllImport("ZibraFluidNative_Win_x64")]
#else
        [DllImport("ZibraFluidNative_Win_x86")]
#endif
        public static extern void RegisterParticlesBuffers(Int32 InstanceID, IntPtr ParticlesInitValues,
                                                           IntPtr PositionMass, IntPtr Affine0, IntPtr Affine1,
                                                           IntPtr PositionRadius, IntPtr ParticleNumber);

#if (UNITY_IOS || UNITY_TVOS || UNITY_WEBGL) && !UNITY_EDITOR
        [DllImport("__Internal")]
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        [DllImport("ZibraFluidNative_Mac_x64")]
#elif UNITY_64 || UNITY_EDITOR_64
        [DllImport("ZibraFluidNative_Win_x64")]
#else
        [DllImport("ZibraFluidNative_Win_x86")]
#endif
        public static extern void RegisterRenderResources(Int32 InstanceID, Int32 CameraID, IntPtr Depth, IntPtr Color0,
                                                          IntPtr Color1, IntPtr AtomicGrid, IntPtr JFA0, IntPtr JFA1);

#if (UNITY_IOS || UNITY_TVOS || UNITY_WEBGL) && !UNITY_EDITOR
        [DllImport("__Internal")]
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        [DllImport("ZibraFluidNative_Mac_x64")]
#elif UNITY_64 || UNITY_EDITOR_64
        [DllImport("ZibraFluidNative_Win_x64")]
#else
        [DllImport("ZibraFluidNative_Win_x86")]
#endif
        public static extern IntPtr SetCameraParameters(Int32 InstanceID, IntPtr CameraRenderParams);

#if (UNITY_IOS || UNITY_TVOS || UNITY_WEBGL) && !UNITY_EDITOR
        [DllImport("__Internal")]
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        [DllImport("ZibraFluidNative_Mac_x64")]
#elif UNITY_64 || UNITY_EDITOR_64
        [DllImport("ZibraFluidNative_Win_x64")]
#else
        [DllImport("ZibraFluidNative_Win_x86")]
#endif
        public static extern IntPtr SetRenderParameters(Int32 InstanceID, IntPtr RenderParams);

#if (UNITY_IOS || UNITY_TVOS || UNITY_WEBGL) && !UNITY_EDITOR
        [DllImport("__Internal")]
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        [DllImport("ZibraFluidNative_Mac_x64")]
#elif UNITY_64 || UNITY_EDITOR_64
        [DllImport("ZibraFluidNative_Win_x64")]
#else
        [DllImport("ZibraFluidNative_Win_x86")]
#endif
        public static extern void SetCollidersCount(Int32 InstanceID, int count);

#if (UNITY_IOS || UNITY_TVOS || UNITY_WEBGL) && !UNITY_EDITOR
        [DllImport("__Internal")]
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        [DllImport("ZibraFluidNative_Mac_x64")]
#elif UNITY_64 || UNITY_EDITOR_64
        [DllImport("ZibraFluidNative_Win_x64")]
#else
        [DllImport("ZibraFluidNative_Win_x86")]
#endif
        public static extern IntPtr GetRenderEventFunc();

#if (UNITY_IOS || UNITY_TVOS || UNITY_WEBGL) && !UNITY_EDITOR
        [DllImport("__Internal")]
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        [DllImport("ZibraFluidNative_Mac_x64")]
#elif UNITY_64 || UNITY_EDITOR_64
        [DllImport("ZibraFluidNative_Win_x64")]
#else
        [DllImport("ZibraFluidNative_Win_x86")]
#endif
        public static extern IntPtr GetRenderEventWithDataFunc();

#if (UNITY_IOS || UNITY_TVOS || UNITY_WEBGL) && !UNITY_EDITOR
        [DllImport("__Internal")]
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        [DllImport("ZibraFluidNative_Mac_x64")]
#elif UNITY_64 || UNITY_EDITOR_64
        [DllImport("ZibraFluidNative_Win_x64")]
#else
        [DllImport("ZibraFluidNative_Win_x86")]
#endif
        public static extern void RegisterCollidersBuffers(Int32 InstanceID, IntPtr ForceTorque, IntPtr ObjPositions);

#if (UNITY_IOS || UNITY_TVOS || UNITY_WEBGL) && !UNITY_EDITOR
        [DllImport("__Internal")]
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        [DllImport("ZibraFluidNative_Mac_x64")]
#elif UNITY_64 || UNITY_EDITOR_64
        [DllImport("ZibraFluidNative_Win_x64")]
#else
        [DllImport("ZibraFluidNative_Win_x86")]
#endif
        public static extern void RegisterSolverBuffers(Int32 InstanceID, IntPtr FluidParameters,
                                                        IntPtr PositionMassCopy, IntPtr ParticleDensity,
                                                        IntPtr GridData, IntPtr IndexGrid, IntPtr GridBlur0,
                                                        IntPtr GridBlur1, IntPtr GridNormal, IntPtr GridSDF,
                                                        IntPtr GridNodePositions, IntPtr NodeParticlePairs,
                                                        IntPtr GridID, IntPtr SortTempBuf, IntPtr RadixGroupData);

#if (UNITY_IOS || UNITY_TVOS || UNITY_WEBGL) && !UNITY_EDITOR
        [DllImport("__Internal")]
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        [DllImport("ZibraFluidNative_Mac_x64")]
#elif UNITY_64 || UNITY_EDITOR_64
        [DllImport("ZibraFluidNative_Win_x64")]
#else
        [DllImport("ZibraFluidNative_Win_x86")]
#endif
        public static extern void SetFluidParameters(Int32 InstanceID, IntPtr FluidParameters);

#if (UNITY_IOS || UNITY_TVOS || UNITY_WEBGL) && !UNITY_EDITOR
        [DllImport("__Internal")]
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        [DllImport("ZibraFluidNative_Mac_x64")]
#elif UNITY_64 || UNITY_EDITOR_64
        [DllImport("ZibraFluidNative_Win_x64")]
#else
        [DllImport("ZibraFluidNative_Win_x86")]
#endif
        public static extern void RegisterVoxelCollider(Int32 InstanceID, IntPtr VoxelIDGrid1, IntPtr VoxelIDGrid2,
                                                        IntPtr VoxelPositions, IntPtr VoxelEmbeddings, int VoxelNum,
                                                        int colliderNumber);

#if (UNITY_IOS || UNITY_TVOS || UNITY_WEBGL) && !UNITY_EDITOR
        [DllImport("__Internal")]
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        [DllImport("ZibraFluidNative_Mac_x64")]
#elif UNITY_64 || UNITY_EDITOR_64
        [DllImport("ZibraFluidNative_Win_x64")]
#else
        [DllImport("ZibraFluidNative_Win_x86")]
#endif
        public static extern void RegisterManipulators(Int32 InstanceID, Int32 ManipulatorNum,
                                                       IntPtr ManipulatorBufferDynamic, IntPtr ManipulatorBufferConst,
                                                       IntPtr ManipulatorParams, Int32 ConstDataSize,
                                                       IntPtr ConstManipulatorData);

#if (UNITY_IOS || UNITY_TVOS || UNITY_WEBGL) && !UNITY_EDITOR
        [DllImport("__Internal")]
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        [DllImport("ZibraFluidNative_Mac_x64")]
#elif UNITY_64 || UNITY_EDITOR_64
        [DllImport("ZibraFluidNative_Win_x64")]
#else
        [DllImport("ZibraFluidNative_Win_x86")]
#endif
        public static extern void UpdateManipulators(int InstanceID, IntPtr ManipulatorParams);

#if (UNITY_IOS || UNITY_TVOS || UNITY_WEBGL) && !UNITY_EDITOR
        [DllImport("__Internal")]
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        [DllImport("ZibraFluidNative_Mac_x64")]
#elif UNITY_64 || UNITY_EDITOR_64
        [DllImport("ZibraFluidNative_Win_x64")]
#else
        [DllImport("ZibraFluidNative_Win_x86")]
#endif
        public static extern void RegisterAnalyticCollider(Int32 InstanceID, int ColliderIndex);

#if (UNITY_IOS || UNITY_TVOS || UNITY_WEBGL) && !UNITY_EDITOR
        [DllImport("__Internal")]
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        [DllImport("ZibraFluidNative_Mac_x64")]
#elif UNITY_64 || UNITY_EDITOR_64
        [DllImport("ZibraFluidNative_Win_x64")]
#else
        [DllImport("ZibraFluidNative_Win_x86")]
#endif
        public static extern void ReleaseResources(int InstanceID);

#if (UNITY_IOS || UNITY_TVOS || UNITY_WEBGL) && !UNITY_EDITOR
        [DllImport("__Internal")]
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        [DllImport("ZibraFluidNative_Mac_x64")]
#elif UNITY_64 || UNITY_EDITOR_64
        [DllImport("ZibraFluidNative_Win_x64")]
#else
        [DllImport("ZibraFluidNative_Win_x86")]
#endif
        public static extern IntPtr RunSDFShaderWithDataPtr();

#if (UNITY_IOS || UNITY_TVOS || UNITY_WEBGL) && !UNITY_EDITOR
        [DllImport("__Internal")]
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        [DllImport("ZibraFluidNative_Mac_x64")]
#elif UNITY_64 || UNITY_EDITOR_64
        [DllImport("ZibraFluidNative_Win_x64")]
#else
        [DllImport("ZibraFluidNative_Win_x86")]
#endif
        public static extern IntPtr GetCameraUpdateFunction();

#if (UNITY_IOS || UNITY_TVOS || UNITY_WEBGL) && !UNITY_EDITOR
        [DllImport("__Internal")]
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        [DllImport("ZibraFluidNative_Mac_x64")]
#elif UNITY_64 || UNITY_EDITOR_64
        [DllImport("ZibraFluidNative_Win_x64")]
#else
        [DllImport("ZibraFluidNative_Win_x86")]
#endif
        public static extern bool IsPaidVersion();

#if (UNITY_IOS || UNITY_TVOS || UNITY_WEBGL) && !UNITY_EDITOR
        [DllImport("__Internal")]
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        [DllImport("ZibraFluidNative_Mac_x64")]
#elif UNITY_64 || UNITY_EDITOR_64
        [DllImport("ZibraFluidNative_Win_x64")]
#else
        [DllImport("ZibraFluidNative_Win_x86")]
#endif
        public static extern IntPtr GetVersionString();

        public static readonly string version = Marshal.PtrToStringAnsi(GetVersionString());

        public enum EventID : int
        {
            None = 0, // only used for GetCameraUpdateFunction
            InitParticles = 1,
            SortParticles = 2,
            StepPhysics = 3,
            Draw = 4,
            PrepareSDF = 5,
            ComputeNeuralSDF = 6,
            ComputeAnalyticSDF = 7,
            UpdateLiquidParameters = 8,
            UpdateManipulatorParameters = 9,
            UpdateForceInteractionBuffers = 10,
            ClearSDFAndID = 11,
            BeginSimulationFrame = 12
        }

        public static int EventAndInstanceID(EventID eventID, int InstanceID)
        {
            return (int)eventID | (InstanceID << 8);
        }
    }
}