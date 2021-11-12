using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine.Rendering;
using com.zibra.liquid.SDFObjects;
using com.zibra.liquid.DataStructures;
using com.zibra.liquid.Utilities;
using com.zibra.liquid.Manipulators;

#if UNITY_PIPELINE_HDRP
using UnityEngine.Rendering.HighDefinition;
#endif // UNITY_PIPELINE_HDRP
using AOT;

#if !ZIBRA_LIQUID_PAID_VERSION && !ZIBRA_LIQUID_FREE_VERSION
#error Missing plugin version definition
#endif

namespace com.zibra.liquid.Solver
{
    /// <summary>
    /// Main ZibraFluid solver component
    /// </summary>
    [AddComponentMenu("Zibra/Zibra Liquid")]
    [ExecuteInEditMode] // Careful! This makes script execute in edit mode.
    // Use "EditorApplication.isPlaying" for play mode only check.
    // Encase this check and "using UnityEditor" in "#if UNITY_EDITOR" preprocessor directive to prevent build errors
    public class ZibraLiquid : MonoBehaviour
    {
        /// <summary>
        /// A list of all instances of the ZibraFluid solver
        /// </summary>
        public static List<ZibraLiquid> AllFluids = new List<ZibraLiquid>();

        private const int MPM_THREADS = 256;
        private const int RADIX_THREADS = 256;
        private const int HISTO_WIDTH = 16;
        private const int SCANBLOCKS = 1;
#region PARTICLES

        [StructLayout(LayoutKind.Sequential)]
        private class ParticlesInitValues
        {
            public Vector3 InitPos;
            public int Length;
            public Vector3 InitVel;
            public float InitMass;
            public Vector3 InitScale;
            public int ParticlesCount;
        }

        [StructLayout(LayoutKind.Sequential)]
        private class CameraRender
        {
            public Matrix4x4 View;
            public Matrix4x4 Projection;
            public Matrix4x4 ProjectionInverse;
            public Matrix4x4 ViewProjection;
            public Matrix4x4 ViewProjectionInverse;
            public Vector4 CameraResolution;
            public Vector3 WorldSpaceCameraPos;
            public float Diameter;
            public int CameraID;
        }

        [StructLayout(LayoutKind.Sequential)]
        private class RenderParameters
        {
            public float blurRadius;
            public float bilateralWeight;
        }

        public class CameraResources
        {
            public RenderTexture background;
            public RenderTexture color0;
            public RenderTexture color1;
            public RenderTexture depth;
            public ComputeBuffer atomicGrid;
            public ComputeBuffer JFAGrid0;
            public ComputeBuffer JFAGrid1;
        }

        // List of all cameras we have added a command buffer to
        private readonly Dictionary<Camera, CommandBuffer> cameraCBs = new Dictionary<Camera, CommandBuffer>();
        // Each camera needs its own resources
        public List<Camera> cameras = new List<Camera>();
        public Dictionary<Camera, CameraResources> cameraResources = new Dictionary<Camera, CameraResources>();
        public Dictionary<Camera, IntPtr> camNativeParams = new Dictionary<Camera, IntPtr>();

        [Range(1024, 8388608)]
        public int MaxNumParticles = 262144;
        public ComputeBuffer PositionMass { get; private set; }
        public ComputeBuffer PositionRadius { get; private set; } // in world space
        public ComputeBuffer Velocity { get; private set; }
        public ComputeBuffer[] Affine { get; private set; }
        public ComputeBuffer ParticleNumber { get; private set; }

        public bool isEnabled = true;
        public float particleDiameter = 0.1f;
        public float particleMass = 1.0f;
        public Bounds bounds;

        private bool usingCustomReflectionProbe;

        private Material fluidMaterial;
        private Mesh quad;
        private ParticlesInitValues particlesInitValuesParams;
        private CameraRender cameraRenderParams;
        private RenderParameters renderParams;

#endregion

#region SOLVER

        /// <summary>
        /// Native solver instance ID number
        /// </summary>
        public int CurrentInstanceID;

        [StructLayout(LayoutKind.Sequential)]
        private class FluidParameters
        {
            public Vector3 GridSize;
            public Int32 NumParticles;
            public Vector3 ContainerScale;
            public Int32 NumNodes;
            public Vector3 ContainerPos;
            public Int32 group;
            public Vector3 gravity;
            public Int32 simulation_frame;
            public Vector3 node_delta;
            public Single dt; // time step
            public Vector3 Direction;
            public Single simulation_time;
            public Single velocity_clamp;
            public Single affine_amount;
            public Single boundary_force;
            public Single boundary_friction;
            public Single dynamic_viscosity;
            public Single eos_stiffness;
            public Single eos_power;
            public Single rest_density;
            public Single elastic_mu;
            public Single elastic_lambda;
            public Single density_compensation;
            public Single deformation_decay;
            public Single NormalizationConstant;
            public Int32 SoftBody;
            public Int32 MaxNumParticles;
            public Int32 NumManipulators;
        }

        private const int BlockDim = 8;
        public ComputeBuffer GridData { get; private set; }
        public ComputeBuffer IndexGrid { get; private set; }
        public ComputeBuffer GridBlur0 { get; private set; }
        public ComputeBuffer GridNormal { get; private set; }
        public ComputeBuffer GridSDF { get; private set; }
        public ComputeBuffer GridID { get; private set; }
        public ComputeBuffer SurfaceGridType { get; private set; }

        /// <summary>
        /// Current timestep
        /// </summary>
        public float timestep = 0.0f;

        /// <summary>
        /// Simulation time passed (in simulation time units)
        /// </summary>
        public float simulationInternalTime;

        /// <summary>
        /// Number of simulation iterations done so far
        /// </summary>
        public int simulationInternalFrame;

        public ComputeBuffer ParticleDensity;
        private int numNodes;
        private FluidParameters fluidParameters;
        private ComputeBuffer positionMassCopy;
        private ComputeBuffer gridBlur1;
#if ZIBRA_LIQUID_PAID_VERSION
        private ComputeBuffer forceTorque;
        private ComputeBuffer objPositions;
#endif
        private ComputeBuffer gridNodePositions;
        private ComputeBuffer nodeParticlePairs;
        private ComputeBuffer SortTempBuffer;
        private ComputeBuffer RadixGroupData;
        struct UpdateColliderParams
        {
            public int colliderCount;
            public IntPtr objPos;
        };

        private UpdateColliderParams updateColliderParams;
        private IntPtr updateColliderParamsNative;
        private CommandBuffer solverCommandBuffer;

#if ZIBRA_LIQUID_PAID_VERSION
        /// <summary>
        /// Forces acting on the SDF colliders
        /// </summary>
        private Vector3[] ObjectForces;
        /// <summary>
        /// Torques acting on the SDF colliders
        /// </summary>
        private Vector3[] ObjectTorques;
#endif
#endregion

        /// <summary>
        /// Using a reference mesh renderer to set the container size and position
        /// </summary>
        public MeshRenderer ContainerReference
        {
            set {
                containerReference = value;
                useContainerReference = true;
                SetContainerReference(value);
            }
            get => containerReference;
        }

        public bool IsSimulatingInBackground { get; set; }

        /// <summary>
        /// The grid size of the simulation
        /// </summary>
        public Vector3Int GridSize { get; private set; }

        /// <summary>
        /// Is this instance of the solver currently active
        /// </summary>
        public bool Activated { get; private set; }
        // Currently we do not support HDRP reflection probes

#if UNITY_PIPELINE_HDRP
        [Tooltip("Use a custom reflection probe")] public HDProbe reflectionProbe;
        [Tooltip("Use a custom light")]
        public Light customLight;
#else
        [Tooltip("Use a custom reflection probe")] public ReflectionProbe reflectionProbe;
#endif // UNITY_PIPELINE_HDRP

        [Tooltip("The maximum allowed simulation timestep")]
        [Range(1e-1f, 4.0f)]
        public float timeStepMax = 1.00f;

        [Tooltip("The speed of the simulation, how many simulation time units per second")]
        [Range(1.0f, 100.0f)]
        public float simTimePerSec = 40.0f;

        public int activeParticleNumber { get; private set; } = 262144;

        [Tooltip("The number of solver iterations per frame, in most cases one iteration is sufficient")] [Range(
            1, 10)] public int iterationsPerFrame = 1;

        [Tooltip(
            "Asynchronously update the liquid - rigid body interaction forces (is faster, but might be less stable)")]
        public bool UseAsyncForceUpdate = true;

        public bool RunSimulation = true;

        [Tooltip(
            "Main parameter that regulates the resolution of the simulation. Defines the size of the simulation grid cell in world length units")]
        [Min(1.0e-5f)]
        public float cellSize = 0.1f;

        [Tooltip("Sets the resolution of the largest sid of the grids container equal to this value")]
        [Min(16)]
        public int gridResolution = 128;

        [Range(1e-2f, 16.0f)]
        public float emitterDensity = 1.0f;

        public bool runSimulation = true;

        /// <summary>
        /// Main parameters of the simulation
        /// </summary>
        public ZibraLiquidSolverParameters solverParameters;

        /// <summary>
        /// Main rendering parameters
        /// </summary>
        public ZibraLiquidMaterialParameters materialParameters;

        /// <summary>
        /// Solver container size
        /// </summary>
        public Vector3 containerSize = new Vector3(10, 10, 10);

        /// <summary>
        /// Solver container position
        /// </summary>
        public Vector3 containerPos;

        /// <summary>
        /// Should we use a container reference
        /// </summary>
        public bool useContainerReference;

        /// <summary>
        /// List of all SDF colliders
        /// </summary>
        [SerializeField]
        private List<SDFCollider> allSDFColliders;

        /// <summary>
        /// List of all manipulators
        /// </summary>
        [SerializeField]
        private List<Manipulator> allManipulators;

        /// <summary>
        /// Initial velocity of the fluid
        /// </summary>
        public Vector3 fluidInitialVelocity;

        /// <summary>
        /// Manager for all objects interacting in some way with the simulation
        /// </summary>
        [HideInInspector]
        [SerializeField]
        private ZibraManipulatorManager manipulatorManager;
        private IntPtr NativeManipData;
        private IntPtr NativeFluidData;
        public ComputeBuffer DynamicManipData;
        public ComputeBuffer ConstManipData;
        /// <summary>
        /// Compute buffer with dynamic manipulator data
        /// </summary>
        public ComputeBuffer ManipulatorData { get; private set; }

        /// <summary>
        /// Compute buffer with constant manipulator data
        /// </summary>
        public ComputeBuffer ConstManipulatorData { get; private set; }

        /// <summary>
        /// List of used SDF colliders
        /// </summary>
        public List<SDFCollider> sdfColliders = new List<SDFCollider>();

        /// <summary>
        /// List of used manipulators
        /// </summary>
        public List<Manipulator> manipulators = new List<Manipulator>();

        public int avgFrameRate;
        public float deltaTime;
        public float smoothDeltaTime;

        /// <summary>
        /// Container reference
        /// </summary>
        [SerializeField]
        private MeshRenderer containerReference;

        private bool wasError;
        private bool needForceInteractionUpdate;

        /// <summary>
        /// Is solver initialized
        /// </summary>
        public bool initialized;

#if UNITY_PIPELINE_HDRP
        private FluidHDRPRenderComponent hdrpRenderer;
#endif // UNITY_PIPELINE_HDRP

        /// <summary>
        /// Activate the solver
        /// </summary>
        public void Run()
        {
            runSimulation = true;
        }

        /// <summary>
        /// Stop the solver
        /// </summary>
        public void Stop()
        {
            runSimulation = false;
        }

        void SetupScriptableRenderComponents()
        {
#if UNITY_EDITOR
#if UNITY_PIPELINE_HDRP
            hdrpRenderer = gameObject.GetComponent<FluidHDRPRenderComponent>();
            if (hdrpRenderer == null)
            {
                hdrpRenderer = gameObject.AddComponent<FluidHDRPRenderComponent>();
                hdrpRenderer.injectionPoint = CustomPassInjectionPoint.BeforePostProcess;
                hdrpRenderer.AddPassOfType(typeof(FluidHDRPRenderComponent.FluidHDRPRender));
                FluidHDRPRenderComponent.FluidHDRPRender renderer =
                    hdrpRenderer.customPasses[0] as FluidHDRPRenderComponent.FluidHDRPRender;
                renderer.name = "ZibraLiquidRenderer";
                renderer.liquid = this;
            }
#endif // UNITY_PIPELINE_HDRP
#endif // UNITY_EDITOR
        }

        protected void OnEnable()
        {
            SetupScriptableRenderComponents();

#if ZIBRA_LIQUID_PAID_VERSION
            if (!ZibraLiquidBridge.IsPaidVersion())
            {
                Debug.LogError(
                    "Free version of native plugin used with paid version of C# plugin. This should never happen.");
            }
#else
            if (ZibraLiquidBridge.IsPaidVersion())
            {
                Debug.LogError(
                    "Paid version of native plugin used with free version of C# plugin. This should never happen.");
            }
#endif

#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlaying)
            {
                return;
            }
#endif

            Init();

            AllFluids?.Add(this);
        }

        private void InitializeParticles()
        {
            fluidParameters = new FluidParameters();

            NativeFluidData = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(FluidParameters)));

            quad = PrimitiveHelper.GetPrimitiveMesh(PrimitiveType.Quad);

            isEnabled = true;
            var numParticlesRounded =
                (int)Math.Ceiling((double)MaxNumParticles / MPM_THREADS) * MPM_THREADS; // round to workgroup size

            GridSize = Vector3Int.CeilToInt(containerSize / cellSize);

            PositionMass = new ComputeBuffer(MaxNumParticles, 4 * sizeof(float));
            PositionRadius = new ComputeBuffer(MaxNumParticles, 4 * sizeof(float));
            Affine = new ComputeBuffer[2];
            Affine[0] = new ComputeBuffer(4 * numParticlesRounded, 2 * sizeof(int));
            Affine[1] = new ComputeBuffer(4 * numParticlesRounded, 2 * sizeof(int));

            ParticleNumber = new ComputeBuffer(128, sizeof(int));

            int[] Pnums = new int[128];
            for (int i = 0; i < 128; i++)
            {
                Pnums[i] = 0;
            }
            ParticleNumber.SetData(Pnums);

            fluidParameters.NumManipulators = 0;

            if (manipulatorManager != null)
            {
                manipulatorManager.UpdateConst(manipulators);
                manipulatorManager.UpdateDynamic(containerPos, containerSize, manipulators);

                if (manipulatorManager.Elements > 0)
                {
                    fluidParameters.NumManipulators = manipulatorManager.Elements;

                    int ManipSize = Marshal.SizeOf(typeof(ZibraManipulatorManager.ManipulatorParam));

                    NativeManipData = Marshal.AllocHGlobal(manipulatorManager.Elements * ManipSize);
                    DynamicManipData = new ComputeBuffer(manipulatorManager.Elements, ManipSize);
                    ConstManipData = new ComputeBuffer(manipulatorManager.Elements, sizeof(int));
                    int ConstDataSize = manipulatorManager.Elements * sizeof(int);

                    var gcparamBuffer0 =
                        GCHandle.Alloc(manipulatorManager.ManipulatorParams.ToArray(), GCHandleType.Pinned);
                    var gcparamBuffer1 = GCHandle.Alloc(manipulatorManager.ConstAdditionalData, GCHandleType.Pinned);

                    ZibraLiquidBridge.RegisterManipulators(
                        CurrentInstanceID, manipulatorManager.Elements, DynamicManipData.GetNativeBufferPtr(),
                        ConstManipData.GetNativeBufferPtr(), gcparamBuffer0.AddrOfPinnedObject(), ConstDataSize,
                        gcparamBuffer1.AddrOfPinnedObject());

                    gcparamBuffer0.Free();
                    gcparamBuffer1.Free();
                }
            }
            else
            {
                Debug.LogWarning("No manipulator manipulatorManager has been set");
            }

            fluidMaterial = new Material(Resources.Load<Shader>("Shaders/FluidShader"));

            particlesInitValuesParams = new ParticlesInitValues();
            cameraRenderParams = new CameraRender();
            renderParams = new RenderParameters();

            particlesInitValuesParams.InitMass = particleMass;
            particlesInitValuesParams.ParticlesCount = MaxNumParticles;

            GCHandle gcparamBuffer = GCHandle.Alloc(particlesInitValuesParams, GCHandleType.Pinned);

            ZibraLiquidBridge.RegisterParticlesBuffers(
                CurrentInstanceID, gcparamBuffer.AddrOfPinnedObject(), PositionMass.GetNativeBufferPtr(),
                Affine[0].GetNativeBufferPtr(), Affine[1].GetNativeBufferPtr(), PositionRadius.GetNativeBufferPtr(),
                ParticleNumber.GetNativeBufferPtr());
            gcparamBuffer.Free();
        }

        public int GetParticleCountRounded()
        {
            return (int)Math.Ceiling((double)MaxNumParticles / MPM_THREADS) * MPM_THREADS; // round to workgroup size;
        }

        public int GetParticleCountFootprint()
        {
            int result = 0;
            int particleCountRounded = GetParticleCountRounded();
            result += MaxNumParticles * 4 * sizeof(float);            // PositionMass
            result += MaxNumParticles * 4 * sizeof(float);            // PositionRadius
            result += 2 * 4 * particleCountRounded * 2 * sizeof(int); // Affine
            result += particleCountRounded * sizeof(float);           // ParticleDensity
            result += particleCountRounded * 4 * sizeof(float);       // positionMassCopy
            result += particleCountRounded * 2 * sizeof(int);         // nodeParticlePairs

            int RadixWorkGroups = (int)Math.Ceiling((float)MaxNumParticles / (float)(RADIX_THREADS * SCANBLOCKS));
            result += particleCountRounded * 2 * sizeof(int);                 // SortTempBuffer
            result += (RadixWorkGroups + 1) * 3 * HISTO_WIDTH * sizeof(uint); // RadixGroupData

            return result;
        }

        public int GetCollidersFootprint()
        {
            int result = 0;

            foreach (var collider in sdfColliders)
            {
                result += collider.GetMemoryFootrpint();
            }

            int ManipSize = Marshal.SizeOf(typeof(ZibraManipulatorManager.ManipulatorParam));

            result += manipulators.Count * ManipSize;   // DynamicManipData
            result += manipulators.Count * sizeof(int); // ConstManipData

            return result;
        }

        public int GetGridFootprint()
        {
            int result = 0;

            GridSize = Vector3Int.CeilToInt(containerSize / cellSize);
            numNodes = GridSize[0] * GridSize[1] * GridSize[2];

            result += numNodes * 4 * sizeof(int);   // GridData
            result += numNodes * 4 * sizeof(float); // GridNormal
            result += numNodes * 4 * sizeof(int);   // GridBlur0
            result += numNodes * 4 * sizeof(int);   // gridBlur1
            result += numNodes * sizeof(float);     // GridSDF
            result += numNodes * sizeof(int);       // GridID
            result += numNodes * 4 * sizeof(float); // gridNodePositions
            result += numNodes * 2 * sizeof(int);   // IndexGrid

            return result;
        }

        private void InitializeSolver()
        {
            simulationInternalTime = 0.0f;
            simulationInternalFrame = 0;
            numNodes = GridSize[0] * GridSize[1] * GridSize[2];
            GridData = new ComputeBuffer(numNodes, 4 * sizeof(int));
            GridNormal = new ComputeBuffer(numNodes, 4 * sizeof(float));
            GridBlur0 = new ComputeBuffer(numNodes, 4 * sizeof(int));
            gridBlur1 = new ComputeBuffer(numNodes, 4 * sizeof(int));
            GridSDF = new ComputeBuffer(numNodes, sizeof(float));
            GridID = new ComputeBuffer(numNodes, sizeof(int));
            gridNodePositions = new ComputeBuffer(numNodes, 4 * sizeof(float));

            IndexGrid = new ComputeBuffer(numNodes, 2 * sizeof(int));

            int NumParticlesRounded = GetParticleCountRounded();

            ParticleDensity = new ComputeBuffer(NumParticlesRounded, sizeof(float));
            positionMassCopy = new ComputeBuffer(NumParticlesRounded, 4 * sizeof(float));
            nodeParticlePairs = new ComputeBuffer(NumParticlesRounded, 2 * sizeof(int));

            int RadixWorkGroups = (int)Math.Ceiling((float)MaxNumParticles / (float)(RADIX_THREADS * SCANBLOCKS));
            SortTempBuffer = new ComputeBuffer(NumParticlesRounded, 2 * sizeof(int));
            RadixGroupData = new ComputeBuffer((RadixWorkGroups + 1) * 3 * HISTO_WIDTH, sizeof(uint));

#if ZIBRA_LIQUID_PAID_VERSION
            ObjectForces = new Vector3[sdfColliders.Count];
            ObjectTorques = new Vector3[sdfColliders.Count];

            int i = 0;
            foreach (var collider in sdfColliders)
            {
                if (collider != null && collider.enabled)
                {
                    if (collider.ForceInteraction)
                    {
                        needForceInteractionUpdate = true;
                    }

                    ObjectForces[i] = new Vector3(0, 0, 0);
                    ObjectTorques[i] = new Vector3(0, 0, 0);
                }
                i++;
            }
#endif

            SetFluidParameters();

            var gcparamBuffer = GCHandle.Alloc(fluidParameters, GCHandleType.Pinned);

            ZibraLiquidBridge.RegisterSolverBuffers(
                CurrentInstanceID, gcparamBuffer.AddrOfPinnedObject(), positionMassCopy.GetNativeBufferPtr(),
                ParticleDensity.GetNativeBufferPtr(), GridData.GetNativeBufferPtr(), IndexGrid.GetNativeBufferPtr(),
                GridBlur0.GetNativeBufferPtr(), gridBlur1.GetNativeBufferPtr(), GridNormal.GetNativeBufferPtr(),
                GridSDF.GetNativeBufferPtr(), gridNodePositions.GetNativeBufferPtr(),
                nodeParticlePairs.GetNativeBufferPtr(), GridID.GetNativeBufferPtr(),
                SortTempBuffer.GetNativeBufferPtr(), RadixGroupData.GetNativeBufferPtr());

            gcparamBuffer.Free();

#if ZIBRA_LIQUID_PAID_VERSION
            if (sdfColliders.Count > 0)
            {
                forceTorque = new ComputeBuffer(6 * sdfColliders.Count, sizeof(int));
                objPositions = new ComputeBuffer(sdfColliders.Count, 4 * sizeof(float));
                updateColliderParamsNative = Marshal.AllocHGlobal(Marshal.SizeOf(updateColliderParams));

                ZibraLiquidBridge.RegisterCollidersBuffers(CurrentInstanceID, forceTorque.GetNativeBufferPtr(),
                                                           objPositions.GetNativeBufferPtr());
            }
#endif

            solverCommandBuffer.IssuePluginEvent(
                ZibraLiquidBridge.GetRenderEventFunc(),
                ZibraLiquidBridge.EventAndInstanceID(ZibraLiquidBridge.EventID.SortParticles, CurrentInstanceID));

            Graphics.ExecuteCommandBuffer(solverCommandBuffer);
        }

        /// <summary>
        /// Initializes a new instance of ZibraFluid
        /// </summary>
        public void Init()
        {
            if (initialized)
            {
                return;
            }

            initialized = true;

            try
            {
                Camera.onPreRender += RenderCallBack;

                solverCommandBuffer = new CommandBuffer { name = "ZibraLiquid.Solver" };
                CurrentInstanceID = ZibraLiquidBridge.CreateFluidInstance();

                InitializeParticles();
                InitializeSolver();

                ZibraLiquidBridge.SetCollidersCount(CurrentInstanceID, FindObjectsOfType<SDFCollider>().Length);

                Activated = true;
            }
            catch
            {
                wasError = true;
                Debug.LogError("Fatal error, Zibra Liquid not initialized");
                Activated = false;
                throw;
            }
        }

        /// <summary>
        /// Sets the container reference
        /// </summary>
        /// <param name="referenceRenderer">Reference renderer</param>
        private void SetContainerReference(Renderer referenceRenderer = null)
        {
            if (referenceRenderer != null)
            {
                containerPos = transform.InverseTransformPoint(referenceRenderer.bounds.center);
                containerSize = referenceRenderer.bounds.size;
            }
        }

        protected void Update()
        {
            allSDFColliders = SDFCollider.AllColliders;
            allManipulators = Manipulator.AllManipulators;

            if (useContainerReference)
                SetContainerReference();

#if UNITY_EDITOR
            if (gameObject.GetComponent<ZibraLiquidMaterialParameters>() ==
                null) // if no component found then add one to this object
            {
                gameObject.AddComponent(typeof(ZibraLiquidMaterialParameters));
            }
            materialParameters = gameObject.GetComponent<ZibraLiquidMaterialParameters>();

            if (GetComponent<ZibraLiquidSolverParameters>() == null) // if no component found then add one to this
                                                                     // object
            {
                gameObject.AddComponent(typeof(ZibraLiquidSolverParameters));
            }
            solverParameters = gameObject.GetComponent<ZibraLiquidSolverParameters>();

            if (GetComponent<ZibraManipulatorManager>() == null) // if no component found then add one to this object
            {
                gameObject.AddComponent(typeof(ZibraManipulatorManager));
            }
            manipulatorManager = gameObject.GetComponent<ZibraManipulatorManager>();

            if (!UnityEditor.EditorApplication.isPlaying)
                return;
#endif

            if (!initialized || wasError)
                return;

            deltaTime = Time.deltaTime;
            smoothDeltaTime = smoothDeltaTime * 0.98f + deltaTime * 0.02f;

            var timeStep = Math.Min(simTimePerSec * smoothDeltaTime / (float)iterationsPerFrame, timeStepMax);

            if (runSimulation && timeStep > 0.0f)
            {
                solverCommandBuffer.Clear();

                solverCommandBuffer.IssuePluginEvent(
                    ZibraLiquidBridge.GetRenderEventFunc(),
                    ZibraLiquidBridge.EventAndInstanceID(ZibraLiquidBridge.EventID.BeginSimulationFrame,
                                                         CurrentInstanceID));

                for (var i = 0; i < iterationsPerFrame; i++)
                {
                    StepPhysics(timeStep);
                }

                Graphics.ExecuteCommandBuffer(solverCommandBuffer);

                var ReadbackRequest = AsyncGPUReadback.Request(ParticleNumber, request => {
                    var Data = request.GetData<int>();
                    activeParticleNumber = Data[0];
                });

#if ZIBRA_LIQUID_PAID_VERSION
                ApplyForceInteraction();
#endif
            }

            particleDiameter = materialParameters.ParticleScale * cellSize /
                               (float)Math.Pow(solverParameters.ParticlesPerCell, 0.333f);
            particleMass = 1.0f;
        }

        /// <summary>
        /// Update the material parameters
        /// </summary>
        public void SetMaterialParams()
        {
#if UNITY_PIPELINE_HDRP
            if (customLight == null)
                Debug.LogError(
                    "No Custom Light set in Zibra Liquids settings. Set Custom Light to render liquid correctly.");
            else
                fluidMaterial.SetVector("WorldSpaceLightPos", customLight.transform.position);

            if (reflectionProbe == null)
                Debug.LogError("No reflection probe added to Zibra Liquids");
#else
            if (reflectionProbe != null) // custom reflection probe
            {
                usingCustomReflectionProbe = true;
                fluidMaterial.SetTexture("ReflectionProbe", reflectionProbe.texture);
                fluidMaterial.SetVector("ReflectionProbe_HDR", reflectionProbe.textureHDRDecodeValues);
                fluidMaterial.SetVector("ReflectionProbe_BoxMax", reflectionProbe.bounds.max);
                fluidMaterial.SetVector("ReflectionProbe_BoxMin", reflectionProbe.bounds.min);
                fluidMaterial.SetVector("ReflectionProbe_ProbePosition", reflectionProbe.transform.position);
            }
            else
            {
                usingCustomReflectionProbe = false;
            }
#endif // UNITY_PIPELINE_HDRP

            fluidMaterial.SetFloat("Opacity", materialParameters.Opacity);
            fluidMaterial.SetFloat("Metal", materialParameters.Metal);
            fluidMaterial.SetFloat("RefractionDistortion", materialParameters.RefractionDistort);
            fluidMaterial.SetFloat("Shadowing", materialParameters.Shadowing);
            fluidMaterial.SetFloat("Smoothness", materialParameters.Smoothness);
            fluidMaterial.SetVector("RefactionColor", materialParameters.RefractionColor);
            fluidMaterial.SetVector("ReflectionColor", materialParameters.ReflectionColor);

            fluidMaterial.SetVector("ContainerScale", containerSize);
            fluidMaterial.SetVector("ContainerPosition", containerPos);
            fluidMaterial.SetVector("GridSize", (Vector3)GridSize);
            fluidMaterial.SetFloat("Foam", materialParameters.Foam);
            fluidMaterial.SetFloat("FoamDensity", materialParameters.FoamDensity * solverParameters.ParticlesPerCell);
            fluidMaterial.SetFloat("ParticleDiameter", particleDiameter);
        }

        private bool CreateTexture(ref RenderTexture texture, Camera cam, int depth, RenderTextureFormat format,
                                   bool enableRandomWrite = false)
        {
            int width = cam.pixelWidth;
            int height = cam.pixelHeight;
            if (texture == null || texture.width != width || texture.height != height)
            {
                if (texture != null)
                {
                    texture.Release();
                    DestroyImmediate(texture);
                }
                texture = new RenderTexture(width, height, depth, format);
                texture.enableRandomWrite = enableRandomWrite;
                texture.filterMode = FilterMode.Point; // make sure its not interpolating
                texture.Create();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Update Native textures for a given camera
        /// </summary>
        /// <param name="cam">Camera</param>
        public bool UpdateNativeTextures(Camera cam)
        {
            int width = cam.pixelWidth;
            int height = cam.pixelHeight;

            if (!cameras.Contains(cam))
            {
                // add camera to list
                cameras.Add(cam);
            }

            int CameraID = cameras.IndexOf(cam);

            if (!cameraResources.ContainsKey(cam))
            {
                cameraResources[cam] = new CameraResources();
            }

            bool isDirty = false;

#if UNITY_PIPELINE_HDRP
            isDirty =
                CreateTexture(ref cameraResources[cam].background, cam, 0, RenderTextureFormat.ARGBHalf) || isDirty;
#else
            isDirty = CreateTexture(ref cameraResources[cam].background, cam, 0, RenderTextureFormat.ARGB32) || isDirty;
#endif
            isDirty = CreateTexture(ref cameraResources[cam].depth, cam, 16, RenderTextureFormat.Depth) || isDirty;
            isDirty =
                CreateTexture(ref cameraResources[cam].color0, cam, 0, RenderTextureFormat.ARGBHalf, true) || isDirty;
            isDirty =
                CreateTexture(ref cameraResources[cam].color1, cam, 0, RenderTextureFormat.ARGBHalf, true) || isDirty;

            if (isDirty)
            {
                cameraResources[cam].atomicGrid?.Release();
                cameraResources[cam].JFAGrid0?.Release();
                cameraResources[cam].JFAGrid1?.Release();

                cameraResources[cam].atomicGrid = new ComputeBuffer(cam.pixelWidth * cam.pixelHeight, sizeof(uint) * 2);
                cameraResources[cam].JFAGrid0 = new ComputeBuffer(cam.pixelWidth * cam.pixelHeight, sizeof(uint));
                cameraResources[cam].JFAGrid1 = new ComputeBuffer(cam.pixelWidth * cam.pixelHeight, sizeof(uint));

                ZibraLiquidBridge.RegisterRenderResources(CurrentInstanceID, CameraID,
                                                          cameraResources[cam].depth.GetNativeTexturePtr(),
                                                          cameraResources[cam].color0.GetNativeTexturePtr(),
                                                          cameraResources[cam].color1.GetNativeTexturePtr(),
                                                          cameraResources[cam].atomicGrid.GetNativeBufferPtr(),
                                                          cameraResources[cam].JFAGrid0.GetNativeBufferPtr(),
                                                          cameraResources[cam].JFAGrid1.GetNativeBufferPtr());
            }

            return isDirty;
        }

        /// <summary>
        /// Render the particles from the native plugin
        /// </summary>
        /// <param name="cmdBuffer">Command Buffer to add the rendering commands to</param>
        public void RenderParticelsNative(CommandBuffer cmdBuffer)
        {
            cmdBuffer.IssuePluginEvent(
                ZibraLiquidBridge.GetRenderEventFunc(),
                ZibraLiquidBridge.EventAndInstanceID(ZibraLiquidBridge.EventID.Draw, CurrentInstanceID));
        }

        /// <summary>
        /// Render the fluid surface
        /// </summary>
        /// <param name="cmdBuffer">Command Buffer to add the rendering commands to</param>
        /// <param name="cam">Camera</param>
        public void RenderFluid(CommandBuffer cmdBuffer, Camera cam)
        {
            cmdBuffer.SetGlobalTexture("Background", cameraResources[cam].background);
            cmdBuffer.SetGlobalTexture("FluidColor", cameraResources[cam].color0);
            fluidMaterial.SetBuffer("GridNormal", GridNormal);
#if UNITY_PIPELINE_HDRP
            cmdBuffer.SetGlobalTexture("ReflectionProbe", reflectionProbe.texture);
            cmdBuffer.SetGlobalVector("ReflectionProbe_HDR", new Vector4(0.01f, 1.0f));
            cmdBuffer.SetGlobalVector("ReflectionProbe_BoxMax", reflectionProbe.bounds.max);
            cmdBuffer.SetGlobalVector("ReflectionProbe_BoxMin", reflectionProbe.bounds.min);
            cmdBuffer.SetGlobalVector("ReflectionProbe_ProbePosition", reflectionProbe.transform.position);
            cmdBuffer.EnableShaderKeyword("HDRP_RENDER");
#else
            if (usingCustomReflectionProbe)
                cmdBuffer.EnableShaderKeyword("CUSTOM_REFLECTION_PROBE");
            else
                cmdBuffer.DisableShaderKeyword("CUSTOM_REFLECTION_PROBE");
#endif

            cmdBuffer.DrawMesh(quad, transform.localToWorldMatrix, fluidMaterial, 0, 0);
        }

        /// <summary>
        /// Update the camera parameters for the particle renderer
        /// </summary>
        /// <param name="cam">Camera</param>
        ///
        public void UpdateCamera(Camera cam)
        {
            Matrix4x4 Projection = GL.GetGPUProjectionMatrix(cam.projectionMatrix, true);
            Matrix4x4 ProjectionInverse = Projection.inverse;
            Matrix4x4 View = cam.worldToCameraMatrix;
            Matrix4x4 ViewInverse = cam.cameraToWorldMatrix;
            Matrix4x4 ViewProjection = Projection * View;

            cameraRenderParams.View = cam.worldToCameraMatrix;
            cameraRenderParams.Projection = GL.GetGPUProjectionMatrix(cam.projectionMatrix, true);
            cameraRenderParams.ProjectionInverse = cameraRenderParams.Projection.inverse;
            cameraRenderParams.ViewProjection = cameraRenderParams.Projection * cameraRenderParams.View;
            cameraRenderParams.ViewProjectionInverse = cameraRenderParams.ViewProjection.inverse;
            cameraRenderParams.Diameter = particleDiameter;
            cameraRenderParams.WorldSpaceCameraPos = cam.transform.position;
            cameraRenderParams.CameraResolution = new Vector4(cam.pixelWidth, cam.pixelHeight, 0.0f, 0.0f);
            cameraRenderParams.CameraID = cameras.IndexOf(cam);

            fluidMaterial.SetMatrix("ProjectionInverse", GL.GetGPUProjectionMatrix(cam.projectionMatrix, true).inverse);

            // update the data at the pointer
            Marshal.StructureToPtr(cameraRenderParams, camNativeParams[cam], true);
        }

        /// <summary>
        /// Update render parameters for a given camera
        /// </summary>
        /// <param name="cam">Camera</param>
        public void UpdateNativeRenderParams(Camera cam)
        {
            if (!camNativeParams.ContainsKey(cam))
            {
                // allocate memory for camera parameters
                camNativeParams[cam] = Marshal.AllocHGlobal(Marshal.SizeOf(cameraRenderParams));
                // update parameters
                UpdateCamera(cam);
                // set initial parameters in the native plugin
                ZibraLiquidBridge.SetCameraParameters(CurrentInstanceID, camNativeParams[cam]);
            }

            renderParams.blurRadius = materialParameters.BlurRadius;
            renderParams.bilateralWeight = materialParameters.BilateralWeight;

            GCHandle gcparamBuffer = GCHandle.Alloc(renderParams, GCHandleType.Pinned);
            ZibraLiquidBridge.SetRenderParameters(CurrentInstanceID, gcparamBuffer.AddrOfPinnedObject());
            gcparamBuffer.Free();
        }

        /// <summary>
        /// Rendering callback which is called by every camera in the scene
        /// </summary>
        /// <param name="cam">Camera</param>
        public void RenderCallBack(Camera cam)
        {
            if (!isEnabled || fluidMaterial == null)
                return;

#if !UNITY_PIPELINE_HDRP && !UNITY_PIPELINE_URP
            if ((cam.cullingMask & (1 << this.gameObject.layer)) ==
                0) // fluid gameobject layer is not in the culling mask of the camera
                return;
#endif

            SetMaterialParams();
            bool isDirty = UpdateNativeTextures(cam);
            UpdateNativeRenderParams(cam);
            UpdateCamera(cam);

#if UNITY_PIPELINE_HDRP || UNITY_PIPELINE_URP
            // upload camera parameters
            ZibraLiquidBridge.SetCameraParameters(CurrentInstanceID, camNativeParams[cam]);
#endif

#if !UNITY_PIPELINE_HDRP && !UNITY_PIPELINE_URP
            if (!cameraCBs.ContainsKey(cam) || isDirty)
            {
                CameraEvent cameraEvent = (cam.actualRenderingPath == RenderingPath.Forward)
                                              ? CameraEvent.BeforeForwardAlpha
                                              : CameraEvent.AfterLighting;
                CommandBuffer renderCommandBuffer;
                if (isDirty && cameraCBs.ContainsKey(cam))
                {
                    renderCommandBuffer = cameraCBs[cam];
                    renderCommandBuffer.Clear();
                }
                else
                {
                    // Create render command buffer
                    renderCommandBuffer = new CommandBuffer { name = "ZibraLiquid.Render" };
                    // add command buffer to camera
                    cam.AddCommandBuffer(cameraEvent, renderCommandBuffer);
                    // add camera to the list
                    cameraCBs[cam] = renderCommandBuffer;
                }

                // enable depth texture
                cam.depthTextureMode = DepthTextureMode.Depth;

                // update native camera parameters
                renderCommandBuffer.IssuePluginEventAndData(
                    ZibraLiquidBridge.GetCameraUpdateFunction(),
                    ZibraLiquidBridge.EventAndInstanceID(ZibraLiquidBridge.EventID.None, CurrentInstanceID),
                    camNativeParams[cam]);

                renderCommandBuffer.Blit(BuiltinRenderTextureType.CurrentActive, cameraResources[cam].background);
                RenderParticelsNative(renderCommandBuffer);
                renderCommandBuffer.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
                RenderFluid(renderCommandBuffer, cam);
            }
#endif
        }

        private void StepPhysics(float dt)
        {
            timestep = dt;

            solverCommandBuffer.IssuePluginEvent(
                ZibraLiquidBridge.GetRenderEventFunc(),
                ZibraLiquidBridge.EventAndInstanceID(ZibraLiquidBridge.EventID.ClearSDFAndID, CurrentInstanceID));

#if ZIBRA_LIQUID_PAID_VERSION
            UpdateForceInteractionBuffers();
#endif
            SetFluidParameters();
            AddColliderSDFs();

            manipulatorManager.UpdateDynamic(containerPos, containerSize, manipulators);

            // Update fluid parameters
            Marshal.StructureToPtr(fluidParameters, NativeFluidData, true);
            solverCommandBuffer.IssuePluginEventAndData(
                ZibraLiquidBridge.GetRenderEventWithDataFunc(),
                ZibraLiquidBridge.EventAndInstanceID(ZibraLiquidBridge.EventID.UpdateLiquidParameters,
                                                     CurrentInstanceID),
                NativeFluidData);

            if (manipulatorManager.Elements > 0)
            {
                // Update manipulator parameters

                // Interop magic
                long LongPtr = NativeManipData.ToInt64(); // Must work both on x86 and x64
                for (int I = 0; I < manipulatorManager.ManipulatorParams.Count; I++)
                {
                    IntPtr Ptr = new IntPtr(LongPtr);
                    Marshal.StructureToPtr(manipulatorManager.ManipulatorParams[I], Ptr, true);
                    LongPtr += Marshal.SizeOf(typeof(Manipulators.ZibraManipulatorManager.ManipulatorParam));
                }

                solverCommandBuffer.IssuePluginEventAndData(
                    ZibraLiquidBridge.GetRenderEventWithDataFunc(),
                    ZibraLiquidBridge.EventAndInstanceID(ZibraLiquidBridge.EventID.UpdateManipulatorParameters,
                                                         CurrentInstanceID),
                    NativeManipData);
            }

            // execute simulation
            solverCommandBuffer.IssuePluginEvent(
                ZibraLiquidBridge.GetRenderEventFunc(),
                ZibraLiquidBridge.EventAndInstanceID(ZibraLiquidBridge.EventID.StepPhysics, CurrentInstanceID));

            // update internal time
            simulationInternalTime += timestep;
            simulationInternalFrame++;
        }

#if ZIBRA_LIQUID_PAID_VERSION
        void UpdateForceInteractionBuffers()
        {
            if (!needForceInteractionUpdate || sdfColliders.Count == 0)
                return;
            var objPos = new float[4 * sdfColliders.Count];

            for (var i = 0; i < sdfColliders.Count; i++)
            {
                if (sdfColliders[i] != null && sdfColliders[i].enabled)
                {
                    Vector3 pos = sdfColliders[i].transform.position;
                    if (sdfColliders[i].ForceInteraction &&
                        sdfColliders[i].gameObject.GetComponent<Rigidbody>() != null)
                        pos = sdfColliders[i].GetComponent<Rigidbody>().worldCenterOfMass;

                    objPos[4 * i + 0] = pos.x;
                    objPos[4 * i + 1] = pos.y;
                    objPos[4 * i + 2] = pos.z;
                    objPos[4 * i + 3] = 0.0f;
                }
            }

            GCHandle gcparamBuffer = GCHandle.Alloc(objPos, GCHandleType.Pinned);

            updateColliderParams.colliderCount = sdfColliders.Count;
            updateColliderParams.objPos = gcparamBuffer.AddrOfPinnedObject();
            Marshal.StructureToPtr(updateColliderParams, updateColliderParamsNative, true);

            // We are using eventID as ColliderNum parameter
            solverCommandBuffer.IssuePluginEventAndData(
                ZibraLiquidBridge.GetRenderEventWithDataFunc(),
                ZibraLiquidBridge.EventAndInstanceID(ZibraLiquidBridge.EventID.UpdateForceInteractionBuffers,
                                                     CurrentInstanceID),
                updateColliderParamsNative);
            gcparamBuffer.Free();
        }
        private void ApplyForceInteraction()
        {
            if (!needForceInteractionUpdate || sdfColliders.Count == 0)
                return;
            // load forces and torques asynchronously
            int id = 0;
            var ReadbackRequest = AsyncGPUReadback.Request(forceTorque, request => {
                id = 0;
                var FT = request.GetData<int>();

                foreach (var collider in sdfColliders)
                {
                    if (collider != null && collider.enabled)
                    {
                        Vector3 force =
                            new Vector3(INT2Float(FT[id * 6]), INT2Float(FT[id * 6 + 1]), INT2Float(FT[id * 6 + 2]));
                        Vector3 torque = new Vector3(INT2Float(FT[id * 6 + 3]), INT2Float(FT[id * 6 + 4]),
                                                     INT2Float(FT[id * 6 + 5]));
                        ObjectForces[id] = force;
                        ObjectTorques[id] = torque;
                    }

                    id++;
                }
            });

            if (!UseAsyncForceUpdate)
                ReadbackRequest.WaitForCompletion();

            // apply forces and torques every frame
            int i = 0;
            foreach (var collider in sdfColliders)
            {
                if (collider != null && collider.enabled && collider.ForceInteraction)
                {
                    if (collider.gameObject.GetComponent<Rigidbody>() != null)
                    {
                        collider.gameObject.GetComponent<Rigidbody>().AddForce(ObjectForces[i], ForceMode.Force);
                        collider.gameObject.GetComponent<Rigidbody>().AddTorque(ObjectTorques[i], ForceMode.Force);
                    }
                    else
                        Debug.LogWarning(
                            "GameObject has no rigid body component but has force interaction activated, please add a rigid body component");
                }
                i++;
            }
        }
#endif

        private void AddColliderSDFs()
        {
            var id = 0;
            foreach (var sdfCollider in sdfColliders.Where(sdfCollider => sdfCollider != null && sdfCollider.enabled))
            {
                if (sdfCollider != null && sdfCollider.enabled)
                {
                    // compute the SDF and unite the sdfs with the given buffer
                    // first argument is the positions and the second is the buffer to write to
                    sdfCollider.ComputeSDF_Unite(CurrentInstanceID, solverCommandBuffer, gridNodePositions, GridSDF,
                                                 GridID, id, numNodes);
                }
                id++;
            }
        }

        private static float INT2Float(int a)
        {
            const float MAX_INT = 2147483647.0f;
            const float F2I_MAX_VALUE = 5000.0f;
            const float F2I_SCALE = (MAX_INT / F2I_MAX_VALUE);

            return a / F2I_SCALE;
        }

        private void SetFluidParameters()
        {
            fluidParameters.affine_amount = 4.0f * (1.0f - solverParameters.Viscosity);
            fluidParameters.boundary_force = solverParameters.BoundaryForce;
            fluidParameters.boundary_friction = 0.1f;
            fluidParameters.ContainerPos = containerPos;
            fluidParameters.ContainerScale = containerSize;
            fluidParameters.deformation_decay = 0.0f;
            fluidParameters.density_compensation = 0.0f;
            fluidParameters.Direction = Vector3.zero;
            fluidParameters.dt = timestep;
            fluidParameters.dynamic_viscosity = 0.0f;
            fluidParameters.elastic_lambda = 0.0f;
            fluidParameters.elastic_mu = 0.0f;
            fluidParameters.eos_power = solverParameters.FluidStiffnessPower;
            fluidParameters.eos_stiffness = solverParameters.FluidStiffness;
            fluidParameters.gravity = solverParameters.Gravity / 100.0f;
            fluidParameters.GridSize = GridSize;
            fluidParameters.group = 0;
            fluidParameters.node_delta = Vector3.zero;
            fluidParameters.NumNodes = numNodes;
            fluidParameters.NumParticles = MaxNumParticles;
            fluidParameters.rest_density = solverParameters.ParticlesPerCell;
            fluidParameters.simulation_frame = simulationInternalFrame;
            fluidParameters.simulation_time = simulationInternalTime;
            fluidParameters.velocity_clamp = solverParameters.VelocityLimit;
            fluidParameters.SoftBody = 0;
            fluidParameters.MaxNumParticles = MaxNumParticles;
        }

        /// <summary>
        /// Disable fluid render for a given camera
        /// </summary>
        public void DisableForCamera(Camera cam)
        {
            CameraEvent cameraEvent =
                cam.actualRenderingPath == RenderingPath.Forward ? CameraEvent.AfterSkybox : CameraEvent.AfterLighting;
            cam.RemoveCommandBuffer(cameraEvent, cameraCBs[cam]);
            cameraCBs[cam].Dispose();
            cameraCBs.Remove(cam);
        }

        protected void ClearRendering()
        {
            Camera.onPreRender -= RenderCallBack;

            foreach (var cam in cameraCBs)
            {
                if (cam.Key != null)
                {
                    cam.Value.Clear();
                }
            }

            cameraCBs.Clear();
            cameras.Clear();

            // free allocated memory
            foreach (var data in camNativeParams)
            {
                Marshal.FreeHGlobal(data.Value);
            }

            foreach (var resource in cameraResources)
            {
                resource.Value.atomicGrid?.Release();
                resource.Value.background?.Release();
                resource.Value.color0?.Release();
                resource.Value.color1?.Release();
                resource.Value.depth?.Release();
                resource.Value.JFAGrid0?.Release();
                resource.Value.JFAGrid1?.Release();
            }
            cameraResources.Clear();
            camNativeParams.Clear();
        }

        protected void ClearSolver()
        {
            ZibraLiquidBridge.ReleaseResources(CurrentInstanceID);

            solverCommandBuffer?.Release();
            PositionMass?.Release();
            PositionRadius?.Release();
            Affine[0]?.Release();
            Affine[1]?.Release();
            GridData?.Release();
            IndexGrid?.Release();
            ParticleDensity?.Release();
            nodeParticlePairs?.Release();
            positionMassCopy?.Release();
            GridNormal?.Release();
            GridBlur0?.Release();
            gridBlur1?.Release();
            GridSDF?.Release();
            GridID?.Release();
#if ZIBRA_LIQUID_PAID_VERSION
            forceTorque?.Release();
            objPositions?.Release();
#endif
            gridNodePositions?.Release();

            ParticleNumber?.Release();
            DynamicManipData?.Release();
            ConstManipData?.Release();

            Marshal.FreeHGlobal(NativeManipData);
            Marshal.FreeHGlobal(NativeFluidData);

            initialized = false;

            // DO NOT USE AllFluids.Remove(this)
            // This will not result in equivalent code
            // ZibraLiquid::Equals is overriden and don't have correct implementation

            if (AllFluids != null)
            {
                for (int i = 0; i < AllFluids.Count; i++)
                {
                    var fluid = AllFluids[i];
                    if (ReferenceEquals(fluid, this))
                    {
                        AllFluids.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        protected void OnApplicationQuit()
        {
            ClearRendering();
        }

        // dispose the objects
        protected void OnDisable()
        {
#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlaying)
            {
                return;
            }
#endif

            if (!initialized)
            {
                return;
            }

            if (Activated)
            {
                ClearRendering();
                ClearSolver();
                Activated = false;
                isEnabled = false;
            }
        }
    }
}