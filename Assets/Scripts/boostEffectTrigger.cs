using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class boostEffectTrigger : MonoBehaviour
{
    public bool hasTriggered = false;

    void Start()
    {
        hasTriggered = false;
    }
    void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.CompareTag("Player"))
        {
            hasTriggered = true;
            Debug.Log("TriggerCount");
        }
    }

    void OnTriggerExit(Collider other)
    {

        if (other.gameObject.CompareTag("Player"))
        {
            hasTriggered = false;
        }
    }
}