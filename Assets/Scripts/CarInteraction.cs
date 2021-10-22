using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CarInteraction : MonoBehaviour {
    public UnityEvent onEnter;

    void OnTriggerEnter(Collider col) {
        if(col.tag == "Player") onEnter.Invoke();
    }
}
