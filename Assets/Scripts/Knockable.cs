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

    private Rigidbody rb;

    void Start() {
        gameObject.tag = "Knockable";
        photoItem = GetComponent<PhotoItem>();
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.constraints = RigidbodyConstraints.FreezeAll;
    }

    void OnCollisionEnter(Collision col) {
        if(col.gameObject.tag == "Player" || col.gameObject.tag == "CarCivilian") {
            rb.useGravity = true;
            rb.constraints = RigidbodyConstraints.None;

            onKnockPlayer.Invoke();
            onKnock.Invoke();
            rb.AddForce(col.rigidbody.velocity * 10f);

            if(addKnockedTag && photoItem != null) {
                photoItem.OverwriteTag("knocked");
                photoItem.sensation = 15;
            }
        }
    }

    void OnTriggerEnter(Collider col) {
        if(col.gameObject.tag == "Shark" && knockableByShark) {
            rb.useGravity = true;
            rb.constraints = RigidbodyConstraints.None;
            onKnockShark.Invoke();
            onKnock.Invoke();
            rb.AddForce(new Vector3(0.05f, 1f, 0) * 15000f);
            rb.AddTorque(new Vector3(10, 50, 5), ForceMode.Impulse);
        }
    }
}
