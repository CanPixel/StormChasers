using System;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.FOptimizing
{
    public partial class OptimizersManager
    {
        /// <summary> Lists of containers, each key can have multiple containers with limited capacity. How many of them will be generated depends on optimizers' different distance ranges or different lod counts. (Optimizing with CullingGroups API) </summary>
        public Dictionary<int, Optimizers_CullingContainersList> CullingContainersIDSpecific { get; private set; }

        /// <summary>
        /// Adding optimizer to culling container for optimal detection.
        /// If there is no contianer for target parameters there is created new.
        /// If there is no slots in containers new one is generated.
        /// </summary>
        internal void AddToContainer(Optimizer_Base optimizer)
        {
            if (optimizer == null) return;

            Optimizers_CullingContainersList containersForIdSpecific;
            Optimizers_CullingContainer container = null;

            // Getting containers list for optimizer's id (from optimizer's distance settings and LOD count)
            if (CullingContainersIDSpecific.TryGetValue(optimizer.ContainerGeneratedID, out containersForIdSpecific))
            {
                if (!optimizer.UseMultiShape)
                {
                    // Searching for container with free slots
                    for (int i = 0; i < containersForIdSpecific.Count; i++)
                        if (containersForIdSpecific[i].HaveFreeSlots)
                        {
                            container = containersForIdSpecific[i];
                            break;
                        }
                }
                else
                {
                    // Searching for container with enough count of free slots
                    for (int i = 0; i < containersForIdSpecific.Count; i++)
                    {
                        if (containersForIdSpecific[i].HaveFreeSlots)
                        {
                            if ((containersForIdSpecific[i].Optimizers.Length - containersForIdSpecific[i].SlotsTaken) > optimizer.Shapes.Count + 1) // +1 for safety
                            {
                                container = containersForIdSpecific[i];
                                break;
                            }
                        }
                    }
                }

                // There is no containers with free slots for this settings
                if (container == null)
                {
                    // Let's generate new one and add to containers list for this settings (stored in id)
                    container = GenerateNewContainer(optimizer);
                    containersForIdSpecific.Add(container);
                }
            }
            else
            {
                // Generating new container list for specified settings (stored in id)
                containersForIdSpecific = new Optimizers_CullingContainersList(optimizer.ContainerGeneratedID);

                // Adding container 
                container = GenerateNewContainer(optimizer);
                containersForIdSpecific.Add(container);

                // Adding containers list to manager data
                CullingContainersIDSpecific.Add(optimizer.ContainerGeneratedID, containersForIdSpecific);
            }

            // Adding optimizer for target container
            container.AddOptimizer(optimizer);
        }


        /// <summary>
        /// Generating container for container list basing on optimizer's parameters
        /// </summary>
        private Optimizers_CullingContainer GenerateNewContainer(Optimizer_Base optimizer)
        {
            Optimizers_CullingContainer container = new Optimizers_CullingContainer(SingleContainerCapacity);
            container.InitializeContainer(optimizer.ContainerGeneratedID, optimizer.GetDistanceMeasures(), TargetCamera);
            return container;
        }


        /// <summary>
        /// Removing optimizer from culling container and clearing it's existence
        /// </summary>
        internal void RemoveFromContainer(Optimizer_Base optimizer)
        {
            if (optimizer == null) return;
            optimizer.OwnerContainer.RemoveOptimizer(optimizer);
        }


        void OnDestroy()
        {
            ClearCullingContainers();
        }

        /// <summary>
        /// Removing culling containers from memory
        /// </summary>
        internal void ClearCullingContainers()
        {
            if (CullingContainersIDSpecific != null)
            {
                foreach (var item in CullingContainersIDSpecific)
                {
                    item.Value.Dispose();
                }

                CullingContainersIDSpecific.Clear();
            }
        }

        public int[] GetContainersIDs()
        {
            int[] containers = new int[CullingContainersIDSpecific.Count];
            int i = 0;

            foreach (var item in CullingContainersIDSpecific)
                containers[i++] = item.Key;

            return containers;
        }
    }
}
