using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace com.zibra.liquid.Manipulators
{
    public class ZibraManipulatorManager : MonoBehaviour
    {
        [HideInInspector]
        [StructLayout(LayoutKind.Sequential)]
        public struct ManipulatorParam
        {
            public Matrix4x4 Transform;
            public Vector3 Position;
            public Int32 Type;
            public Quaternion Rotation;
            public Vector3 Scale;
            public Int32 Enabled;
            public Vector4 AdditionalData;
        }

        // All data together
        [HideInInspector]
        public int Elements = 0;
        [HideInInspector]
        public List<ManipulatorParam> ManipulatorParams = new List<ManipulatorParam>();

        [HideInInspector]
        public float[] ConstAdditionalData;
        [HideInInspector]
        public List<int> ConstDataID = new List<int>();

        private Vector3 VectorClamp(Vector3 x, Vector3 min, Vector3 max)
        {
            return Vector3.Max(Vector3.Min(x, max), min);
        }

        /// <summary>
        /// Update all arrays and lists with manipulator object data
        /// Should be executed every simulation frame
        /// </summary>
        public void UpdateDynamic(Vector3 containerPos, Vector3 containerSize, List<Manipulator> manipulators)
        {
            int ID = 0;
            ManipulatorParams.Clear();
            // fill arrays
            foreach (var manipulator in manipulators)
            {
                if (manipulator == null)
                    continue;
                ManipulatorParam manip = new ManipulatorParam();

                manip.Transform = manipulator.transform.localToWorldMatrix;
                manip.Type = (int)manipulator.TYPE;
                manip.Rotation = manipulator.transform.rotation;
                manip.Scale = manipulator.transform.lossyScale;
                manip.Position = manipulator.transform.position;

                if (manipulator.TYPE == Manipulator.ManipulatorType.Emitter) // clamp emitter to container edge
                {
                    Vector3 manipMin = manip.Position - manip.Scale * 0.5f;
                    Vector3 manipMax = manip.Position + manip.Scale * 0.5f;

                    Vector3 containerMin = containerPos - containerSize * 0.485f;
                    Vector3 containerMax = containerPos + containerSize * 0.485f;

                    manipMin =
                        VectorClamp(manipMin, containerMin, Vector3.Max(containerMax - manip.Scale, containerMin));
                    manipMax =
                        VectorClamp(manipMax, Vector3.Min(containerMin + manip.Scale, containerMax), containerMax);

                    manip.Scale = manipMax - manipMin;
                    manip.Position = 0.5f * (manipMin + manipMax);
                }

                manip.Enabled = manipulator.enabled ? 1 : 0;
                manip.AdditionalData = manipulator.AdditionalData;

                ManipulatorParams.Add(manip);
                ID++;
            }
            // ManipulatorParams.ToArray();
            Elements = manipulators.Count;
        }

        /// <summary>
        /// Update constant object data
        /// Should be executed once
        /// </summary>
        public void UpdateConst(List<Manipulator> manipulators)
        {
            if (ConstDataID.Count == 0)
            {
                foreach (var manipulator in manipulators)
                {
                    if (manipulator == null)
                        continue;
                    int ID = ConstAdditionalData.Length;
                    ConstDataID.Add(ID);
                    Array.Resize<float>(ref ConstAdditionalData, ID + manipulator.ConstAdditionalData.Length);
                    Array.Copy(manipulator.ConstAdditionalData, 0, ConstAdditionalData, ID,
                               manipulator.ConstAdditionalData.Length);
                }
            }
        }
    }
}
