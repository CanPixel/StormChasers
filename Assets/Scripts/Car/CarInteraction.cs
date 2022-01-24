using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CarInteraction : MonoBehaviour {
    private bool triggered = false;

    public UnityEvent onEnter;

    void OnTriggerEnter(Collider col) {
        if(col.tag == "Player" && !triggered) {
            onEnter.Invoke();
            triggered = true;
        } 
    }
}
