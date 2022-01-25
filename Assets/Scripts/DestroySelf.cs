using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroySelf : MonoBehaviour
{
    public float waitForDisable; 
    // Update is called once per frame
    void Update()
    {
        Object.Destroy(this.gameObject, waitForDisable);
    }
}
