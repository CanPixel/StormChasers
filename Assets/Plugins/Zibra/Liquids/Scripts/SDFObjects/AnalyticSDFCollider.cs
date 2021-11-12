using System;
using UnityEngine;
using UnityEngine.Rendering;
using System.Runtime.InteropServices;
using com.zibra.liquid.Solver;

namespace com.zibra.liquid.SDFObjects
{
    /// <summary>
    /// An analytical ZibraFluid SDF collider
    /// </summary>
    [AddComponentMenu("Zibra/Zibra Analytic Collider")]
    public class AnalyticSDFCollider : SDFCollider
    {
        /// <summary>
        /// Types of Analytical SDF's
        /// </summary>
        public enum SDFType
        {
            Sphere,
            Box,
            Bowl
        }
        ;

        /// <summary>
        /// Currently chosen type of SDF collider
        /// </summary>
        public SDFType chosenSDFType = SDFType.Sphere;

        /// <summary>
        /// Initialize the analytical SDF collider in the given solver instance
        /// </summary>
        /// <param name="InstanceID">Istance ID of the solver instance</param>
        public void Initialize(int InstanceID)
        {
            if (IsInitialized.Contains(InstanceID))
                return;

            ColliderIndex = AllColliders.IndexOf(this);
            ZibraLiquidBridge.RegisterAnalyticCollider(InstanceID, ColliderIndex);
            IsInitialized.Add(InstanceID);
        }

        public void Start()
        {
            colliderParams = new ColliderParams();
            NativeDataPtr = Marshal.AllocHGlobal(Marshal.SizeOf(colliderParams));
        }

        private void ComputeSDF_Base(int InstanceID, CommandBuffer SolverCommandBuffer, ComputeBuffer Coordinates,
                                     ComputeBuffer Output, ComputeBuffer OutputID, int ID, int Elements,
                                     CalculationType type)
        {
            Initialize(InstanceID);

            Vector3 position = transform.position;
            Quaternion rotation = transform.rotation;
            Vector3 scale = transform.lossyScale;
            Vector3 newpos = position;

            colliderParams.Position = newpos;
            colliderParams.Rotation = new Vector4(rotation.x, rotation.y, rotation.z, rotation.w);
            colliderParams.Scale = scale;
            colliderParams.Elements = Elements;
            colliderParams.OpType = (int)type;
            colliderParams.CurrentID = ID;
            colliderParams.SDFType = (int)chosenSDFType;
            colliderParams.colliderIndex = ColliderIndex;

            Marshal.StructureToPtr(colliderParams, NativeDataPtr, true);
            SolverCommandBuffer.IssuePluginEventAndData(
                ZibraLiquidBridge.RunSDFShaderWithDataPtr(),
                ZibraLiquidBridge.EventAndInstanceID(ZibraLiquidBridge.EventID.ComputeAnalyticSDF, InstanceID),
                NativeDataPtr);
        }

        /// <summary>
        /// Compute the SDF and unite it with the SDF in Output
        /// </summary>
        /// <param name="InstanceID">Istance ID of the solver instance</param>
        /// <param name="SolverCommandBuffer">Command buffer to add the SDF computation to</param>
        /// <param name="Coordinates">Buffer with input cooditates whre the SDF needs to be sampled</param>
        /// <param name="Output">Output SDF</param>
        /// <param name="OutputID">Output ID of the object at each point</param>
        /// <param name="ID">Istance ID of the SDF collider</param>
        /// <param name="Elements">Number of points to compute the SDF for</param>
        public override void ComputeSDF_Unite(int InstanceID, CommandBuffer SolverCommandBuffer,
                                              ComputeBuffer Coordinates, ComputeBuffer Output, ComputeBuffer OutputID,
                                              int ID, int Elements)
        {
            ComputeSDF_Base(InstanceID, SolverCommandBuffer, Coordinates, Output, OutputID, ID, Elements,
                            CalculationType.Unite);
        }
    }
}