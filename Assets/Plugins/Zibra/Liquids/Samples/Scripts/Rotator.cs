using UnityEngine;
using UnityEngine.Serialization;

namespace com.zibra.liquid.Samples
{
    public class Rotator : MonoBehaviour
    {
        [FormerlySerializedAs("RotationSpeed")]
        public float rotationSpeed = 20.0f;

        protected void Update()
        {
            transform.RotateAround(transform.localPosition, Vector3.up, rotationSpeed * Time.deltaTime);
        }
    }
}
