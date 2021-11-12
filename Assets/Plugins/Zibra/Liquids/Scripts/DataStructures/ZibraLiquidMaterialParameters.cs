using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.zibra.liquid.DataStructures
{
    [Serializable]
    public class ZibraLiquidMaterialParameters : MonoBehaviour
    {
        [Tooltip("The color of the liquid body")]
        public Color RefractionColor = new Color(.34f, .85f, .92f, 1.0f);

        [Tooltip("The smoothness of the liquid body")]
        [Range(0.0f, 1.0f)]
        public float Smoothness = 0.96f;

        [Tooltip("The metalness of the surface")]
        [Range(0.0f, 1.0f)]
        public float Metal = 0.3f;

        [Tooltip("The opacity of the liquid body")]
        [Range(0.0f, 100.0f)]
        public float Opacity = 3.0f;

        [Tooltip("Fluid depth color shadowing")]
        [Range(0.0f, 1.0f)]
        public float Shadowing = 0.6f;

        [Tooltip("The amount of refraction")]
        [Range(0.0f, 1.0f)]
        public float RefractionDistort = 0.15f;

        [Tooltip("Particle rendering scale compared to the cell size")]
        [Range(0.0f, 4.0f)]
        public float ParticleScale = 1.5f;

        [Tooltip("Amount of foam")]
        [Range(0.0f, 2.0f)]
        public float Foam = 0.8f;

        [Tooltip("Foaming density threshold")]
        [Range(0.0f, 4.0f)]
        public float FoamDensity = 1.0f;

        [Range(0.0001f, 0.1f)]
        public float BlurRadius = 0.04f;

        [HideInInspector]
        [Range(0.0f, 20.0f)]
        public float BilateralWeight = 2.5f;

        [Tooltip("The color of the liquid reflection mod (A - fresnel mod)")]
        public Color ReflectionColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
    }
}