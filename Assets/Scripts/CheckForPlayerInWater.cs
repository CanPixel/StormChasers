using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckForPlayerInWater : MonoBehaviour
{
    public List<GameObject> sharks;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            foreach(GameObject shark in sharks)
            {
                shark.GetComponent<SharkScript>().playerInFountain = true; 
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            foreach (GameObject shark in sharks)
            {
                shark.GetComponent<SharkScript>().playerInFountain = false;
            }
        }
    }
}
