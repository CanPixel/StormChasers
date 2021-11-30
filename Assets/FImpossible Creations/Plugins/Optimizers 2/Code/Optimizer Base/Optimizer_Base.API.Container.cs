using UnityEngine;

namespace FIMSpace.FOptimizing
{
    public abstract partial class Optimizer_Base
    {
        /// <summary> Static ID For CullingGroups container basing on distance ranges values and counts </summary>
        public int ContainerGeneratedID { get; private set; }

        /// <summary> To what culling container belongs this optimizer </summary>
        public Optimizers_CullingContainer OwnerContainer { get; private set; }

        /// <summary> Id of optimizer inside it's container </summary>
        public int ContainerSphereId { get; private set; }

        /// <summary> If using multi-shape </summary>
        public int[] ContainerSphereIds { get; private set; }


        [Tooltip("Adding optimizer to culling container - when used a lot of objects with same distance levels and LOD levels count it can boost performance a lot.")]
        public bool AddToContainer = true;


        internal void AssignToContainer(Optimizers_CullingContainer container, int sphereId, ref BoundingSphere sphere)
        {
            OwnerContainer = container;
            ContainerSphereId = sphereId;
            mainVisibilitySphere = sphere;
        }


        internal void AssignToContainer(Optimizers_CullingContainer container, int[] sphereIds)
        {
            OwnerContainer = container;
            ContainerSphereIds = sphereIds;
        }
    }
}
