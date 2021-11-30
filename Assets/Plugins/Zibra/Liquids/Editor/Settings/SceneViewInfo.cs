using System;
using UnityEngine;

namespace com.zibra.liquid
{
    [Serializable]
    internal class SceneViewInfo
    {
        public Vector3 Position { get; private set; }
        public Quaternion Rotation { get; private set; }
        public Vector3 Pivot { get; private set; }
        public float Size { get; private set; }
        public bool Is2D { get; private set; }
        public bool IsOrtho { get; private set; }
        public bool IsDefault { get; private set; }

        public SceneViewInfo()
        {
            IsDefault = true;
        }

        public SceneViewInfo(Vector3 position, Vector3 pivot, Quaternion rotation, float size, bool is2D, bool isOrtho)
        {
            Position = position;
            Rotation = rotation;
            Pivot = pivot;
            Size = size;
            Is2D = is2D;
            IsOrtho = isOrtho;
        }
    }
}
