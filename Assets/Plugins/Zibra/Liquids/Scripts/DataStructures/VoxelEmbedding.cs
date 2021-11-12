using System;
using UnityEngine;

namespace com.zibra.liquid.DataStructures
{
    [Serializable]
    public struct VoxelEmbedding
    {
        public Vector3Int coords;
        public float[] embedding;
    }
}