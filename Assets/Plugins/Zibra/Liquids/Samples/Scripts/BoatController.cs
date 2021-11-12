using UnityEngine;

namespace com.zibra.liquid.Samples
{
    public class BoatController : MonoBehaviour
    {
        [SerializeField]
        private Transform lPaddle;
        [SerializeField]
        private Transform rPaddle;
        [SerializeField]
        private float speed;
        private Quaternion lDefaultRotation;
        private Quaternion rDefaultRotation;

        protected void Start()
        {
            lDefaultRotation = lPaddle.rotation;
            rDefaultRotation = rPaddle.rotation;
        }

        // Update is called once per frame
        protected void Update()
        {
            // if (Input.GetKey(KeyCode.W))
            // {
            //     rPaddle.Rotate(new Vector3(speed,0,0));
            //     lPaddle.Rotate(new Vector3(speed,0,0));
            // }
            // if (Input.GetKey(KeyCode.S))
            // {
            //     rPaddle.Rotate(new Vector3(-speed,0,0));
            //     lPaddle.Rotate(new Vector3(-speed,0,0));
            // }

            if (Input.GetKey(KeyCode.D))
            {
                rPaddle.Rotate(new Vector3(speed, 0, 0));
            }
            else
            {
                rPaddle.rotation = Quaternion.Lerp(rPaddle.rotation, rDefaultRotation, 0.2f);
            }

            if (Input.GetKey(KeyCode.A))
            {
                lPaddle.Rotate(new Vector3(speed, 0, 0));
            }
            else
            {
                lPaddle.rotation = Quaternion.Lerp(lPaddle.rotation, lDefaultRotation, 0.2f);
            }
        }
    }
}