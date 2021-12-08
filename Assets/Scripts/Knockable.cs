using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
public class Knockable : MonoBehaviour {
    public bool knockableByShark = false;
    public bool addKnockedTag = true;
    private PhotoItem photoItem;

    public UnityEvent onKnockShark, onKnockPlayer, onKnock;

    void Start() {
        gameObject.tag = "Knockable";
        photoItem = GetComponent<PhotoItem>();
    }

    void OnCollisionEnter(Collision col) {
        if(col.gameObject.tag == "Player" || col.gameObject.tag == "Shark") {
            var rb = GetComponent<Rigidbody>();
            rb.useGravity = true;
            rb.isKinematic = false;
            onKnockPlayer.Invoke();
            onKnock.Invoke();
            rb.AddForce(col.gameObject.GetComponent<Rigidbody>().velocity * 100f);

            if(addKnockedTag && photoItem != null) photoItem.OverwriteTag("knocked");
        }
    }

    void OnTriggerEnter(Collider col) {
        if(col.gameObject.tag == "Shark" && knockableByShark) {
            var rb = GetComponent<Rigidbody>();
            rb.useGravity = true;
            rb.isKinematic = false;
            onKnockShark.Invoke();
            onKnock.Invoke();
            rb.AddForce(new Vector3(0.05f, 1f, 0) * 15000f);
            rb.AddTorque(new Vector3(10, 50, 5), ForceMode.Impulse);
        }
    }
}
