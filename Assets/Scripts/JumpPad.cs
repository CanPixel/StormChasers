using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpPad : MonoBehaviour
{
    public float padJumpHeight = 50;
    private float defaultJumpHeight;
    private bool hasBeenLaunched = false;
    private CarMovement carMovementScript;
    public bool AutoTrigger;

    void Start()
    {
        carMovementScript = FindObjectOfType<CarMovement>();
        defaultJumpHeight = carMovementScript.jumpHeight;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (AutoTrigger) LaunchPlayer();
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


    void LaunchPlayer()
    {
        carMovementScript.jumpHeight = padJumpHeight;
        carMovementScript.Jump();
        carMovementScript.jumpHeight = defaultJumpHeight;
        hasBeenLaunched = false;
    }
}