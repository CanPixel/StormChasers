using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.zibra.liquid.DataStructures
{
    [Serializable]
    public class ZibraLiquidSolverParameters : MonoBehaviour
    {
        // SIMULATION PARAMETERS
        [Tooltip("The strength and direction of the gravity")]
        public Vector3 Gravity = new Vector3(0.0f, -9.81f, 0.0f);

        [Tooltip("The stiffness of the liquid, recommended 0.1f")]
        [Min(0.0f)]
        public float FluidStiffness = 0.1f;

        [Tooltip("The sharpness of the stiffness, recommended 4.0f")]
        [Range(1.0f, 8.0f)]
        [HideInInspector]
        public float FluidStiffnessPower = 3.0f;

        [Tooltip(
            "The more particles per cell the higher the simulation quality, but is more expensive for the same volume")]
        [Range(0.1f, 10.0f)]
        public float ParticlesPerCell = 1f;

        [Tooltip("The velocity limit of the particles")]
        [Min(0.0f)]
        public float VelocityLimit = 3f;

        [Tooltip("Viscosity of the liquid")]
        [Range(0.0f, 1.0f)]
        public float Viscosity = 0.0f;

        [Tooltip("The strenght of the force acting on a liquid while its touching objects, recommended 3.0")]
        [Range(0.25f, 20.0f)]
        public float BoundaryForce = 3.0f;
    }
}