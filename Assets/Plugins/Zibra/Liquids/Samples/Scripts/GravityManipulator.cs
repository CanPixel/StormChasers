using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.zibra.liquid.Samples
{
    public class GravityManipulator : MonoBehaviour
    {
        Solver.ZibraLiquid liquid;

        // Start is called before the first frame update
        void Start()
        {
            liquid = GetComponent<Solver.ZibraLiquid>();
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKey(KeyCode.UpArrow))
            {
                liquid.solverParameters.Gravity.y = 9.81f;
                liquid.solverParameters.Gravity.x = 0.0f;
            }

            if (Input.GetKey(KeyCode.DownArrow))
            {
                liquid.solverParameters.Gravity.y = -9.81f;
                liquid.solverParameters.Gravity.x = 0.0f;
            }

            if (Input.GetKey(KeyCode.RightArrow))
            {
                liquid.solverParameters.Gravity.y = 0.0f;
                liquid.solverParameters.Gravity.x = 9.81f;
            }

            if (Input.GetKey(KeyCode.LeftArrow))
            {
                liquid.solverParameters.Gravity.x = -9.81f;
                liquid.solverParameters.Gravity.y = 0.0f;
            }

            if (Input.GetKey(KeyCode.O))
            {
                liquid.solverParameters.Gravity.x = 0.0f;
                liquid.solverParameters.Gravity.y = 0.0f;
            }

            if (Input.GetKey(KeyCode.LeftShift))
            {
                liquid.solverParameters.Gravity *= 1.02f;
            }
            if (Input.GetKey(KeyCode.LeftControl))
            {
                liquid.solverParameters.Gravity *= 0.98f;
            }
        }
    }
}
