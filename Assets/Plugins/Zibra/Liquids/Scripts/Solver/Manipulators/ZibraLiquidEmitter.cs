using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace com.zibra.liquid.Manipulators
{
    [AddComponentMenu("Zibra/Zibra Liquid Emitter")]
    public class ZibraLiquidEmitter : Manipulator
    {
        [Tooltip("Emitted particles per second")]
        [Min(0.0f)]
        public float ParticlesPerSec = 6000.0f;

        [Tooltip("The velocity of the created fluid")]
        [Range(0.001f, 20.0f)]
        public float VelocityMagnitude = 0.001f;

        [HideInInspector]
        public Vector3 Velocity = new Vector3(0.001f, 0, 0);
        private void UpdateVelocity()
        {
            Velocity = transform.rotation * new Vector3(VelocityMagnitude, 0, 0);
        }

        void GismosDrawArrow(Vector3 origin, Vector3 vector, Color color, float arrowHeadLength = 0.25f,
                             float arrowHeadAngle = 20.0f)
        {
            Gizmos.color = color;
            Gizmos.DrawRay(origin, vector);
            arrowHeadLength *= Vector3.Magnitude(vector);

            Vector3 right =
                Quaternion.LookRotation(vector) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Vector3 left =
                Quaternion.LookRotation(vector) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);

            Gizmos.DrawRay(origin + vector, right * arrowHeadLength);
            Gizmos.DrawRay(origin + vector, left * arrowHeadLength);
        }

        void OnDrawGizmosSelected()
        {
            UpdateVelocity();
            float scale = 1.0f;
            GismosDrawArrow(transform.position, scale * Velocity, Color.blue, 0.5f);
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(transform.position, transform.lossyScale);
        }

        void OnDrawGizmos()
        {
            OnDrawGizmosSelected();
        }

        ZibraLiquidEmitter()
        {
            // DataAmount = 4;
            TYPE = ManipulatorType.Emitter;
        }

        private void Update()
        {
            UpdateVelocity();
            AdditionalData.x = Mathf.Floor(ParticlesPerSec * Time.smoothDeltaTime);
            AdditionalData.y = Velocity.x;
            AdditionalData.z = Velocity.y;
            AdditionalData.w = Velocity.z;
        }
    }
}