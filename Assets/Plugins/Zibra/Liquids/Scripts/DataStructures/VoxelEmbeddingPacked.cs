using System;

namespace com.zibra.liquid.DataStructures
{
    [Serializable]
    public struct VoxelEmbeddingPacked
    {
        public int x;
        public int y;
        public int z;
        public string embedding;
    }
}