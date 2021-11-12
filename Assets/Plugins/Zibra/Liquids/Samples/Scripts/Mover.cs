using UnityEngine;
using System;
using UnityEngine.Serialization;

namespace com.zibra.liquid.Samples
{
    public class Mover : MonoBehaviour
    {
        [FormerlySerializedAs("Direction")]
        public Vector3 direction;
        [FormerlySerializedAs("Amplitude")]
        public float amplitude;
        [FormerlySerializedAs("Speed")]
        public float speed;

        protected void Update()
        {
            transform.Translate(Time.deltaTime * amplitude * direction * speed * (float)Math.Sin(speed * Time.time) /
                                (2.0f * (float)Math.PI));
        }
    }
}
