using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpChecker : MonoBehaviour
{

    public bool playerIsJumpingOver;
    public bool sharkCanJump;
    public bool sharkIsJumping;

    public List<GameObject> sharks;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            foreach (GameObject shark in sharks)
            {
                shark.GetComponent<SharkScript>().playerJumpingOverFountain = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            foreach (GameObject shark in sharks)
            {
                shark.GetComponent<SharkScript>().playerJumpingOverFountain = false;
            }
        }
    }

    void Start()
    {
        
    }

  
    void Update()
    {
        
    }
}
