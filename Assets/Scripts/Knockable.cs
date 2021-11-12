using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
public class Knockable : MonoBehaviour {
    public UnityEvent onKnockShark, onKnockPlayer;

    void OnCollisionEnter(Collision col) {
        if(col.gameObject.tag == "Player" || col.gameObject.tag == "Shark") {
            var rb = GetComponent<Rigidbody>();
            rb.useGravity = true;
            rb.isKinematic = false;
            onKnockPlayer.Invoke();
            rb.AddForce(col.gameObject.GetComponent<Rigidbody>().velocity * 100f);
        }
    }

    void OnTriggerEnter(Collider col) {
        if(col.gameObject.tag == "Shark") {
            var rb = GetComponent<Rigidbody>();
            rb.useGravity = true;
            rb.isKinematic = false;
            onKnockShark.Invoke();
            rb.AddForce(col.gameObject.GetComponent<Rigidbody>().velocity * 1500f);
        }
    }
}
