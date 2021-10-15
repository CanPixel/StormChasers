using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Knockable : MonoBehaviour {
    void OnCollisionEnter(Collision col) {
        if(col.gameObject.tag == "Player") {
            var rb = GetComponent<Rigidbody>();
            rb.useGravity = true;
            rb.isKinematic = false;
            rb.AddForce(col.gameObject.GetComponent<Rigidbody>().velocity * 100f);
        }
    }
}
