using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PadJump : MonoBehaviour
{
    public float padJumpHeight = 50;
    private float defaultJumpHeight;
    private bool hasBeenLaunched = false;
    private bool canLaunch = true; 
    private CarMovement carMovementScript;
    public bool AutoTrigger;


    void Start()
    {
        carMovementScript = FindObjectOfType<CarMovement>();
        defaultJumpHeight = carMovementScript.jumpHeight;
        canLaunch = true; 
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (AutoTrigger && canLaunch)
            {
                canLaunch = false; 
                LaunchPlayer();
            }
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (!AutoTrigger && carMovementScript.jump >= .5f && !hasBeenLaunched)
            {
                hasBeenLaunched = true;
                LaunchPlayer();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (!canLaunch)
                canLaunch = true; 
        }
    }


    void LaunchPlayer()
    {
        carMovementScript.jumpHeight = padJumpHeight;
       // carMovementScript.rb.velocity = new Vector3(carMovementScript.rb.velocity.x, 0, carMovementScript.rb.velocity.z); 
        carMovementScript.Jump();
        carMovementScript.jumpHeight = defaultJumpHeight;
        hasBeenLaunched = false;
    }

    void WaitForRecharge()
    {

    }
}