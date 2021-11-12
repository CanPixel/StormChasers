using System.Collections.Generic;
using UnityEngine;

namespace com.zibra.liquid.Manipulators
{
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    public class Manipulator : MonoBehaviour
    {
        [HideInInspector]
        public float[] ConstAdditionalData; // Data to send to a compute buffer once

        [HideInInspector]
        public ManipulatorType TYPE { get; protected set; } = 0;

        [HideInInspector] public Vector4 AdditionalData;

        public static readonly List<Manipulator> AllManipulators = new List<Manipulator>();

        public enum ManipulatorType
        {
            None,
            Emitter,
            Void,
            ForceField,
            NeuralSDF,
            TypeNum
        }
        ;
        protected void OnEnable()
        {
            if (!AllManipulators?.Contains(this) ?? false)
            {
                AllManipulators.Add(this);
            }
        }

        protected void OnDisable()
        {
            if (AllManipulators?.Contains(this) ?? false)
            {
                AllManipulators.Remove(this);
            }
        }
    }

}
