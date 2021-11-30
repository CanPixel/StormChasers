using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Trigger : MonoBehaviour {
    public UnityEvent onEnter, onExit;

    void OnTriggerEnter(Collider col) {
        if(col.tag == "Player") onEnter.Invoke();
    }

    void OnTriggerExit(Collider col) {
        if(col.tag == "Player") onExit.Invoke();
    }
}
