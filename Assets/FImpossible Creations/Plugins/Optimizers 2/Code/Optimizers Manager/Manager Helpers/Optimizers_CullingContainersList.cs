using System;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.FOptimizing
{
    /// <summary>
    /// FM: Class which is managing CullingGroup bounding spheres and sending culling groups' events to optimizer components
    /// Objects need to have the same distance values to be able to be saved in container
    /// </summary>
    //[Serializable] DEBUG
    public class Optimizers_CullingContainersList : List<Optimizers_CullingContainer>
    {
        public int ID { get; private set; }


        public Optimizers_CullingContainersList(int id)
        {
            ID = id;
        }


        public void Dispose()
        {
            for (int i = 0; i < Count; i++)
            {
                this[i].Dispose();
            }

            Clear();
        }
    }
}
