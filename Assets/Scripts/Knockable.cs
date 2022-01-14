using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
public class Knockable : MonoBehaviour {
    public bool knockableByShark = false;
    public bool addKnockedTag = true;
    private PhotoItem photoItem;
    
    public bool canbeExploded = true; 
    private bool isStanding = true;
    private GameObject collidedObject;

    public UnityEvent onKnockShark, onKnockPlayer, onKnock;

    public Rigidbody rb;

    void Start() {
        gameObject.tag = "Knockable";
        photoItem = GetComponent<PhotoItem>();
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
	    rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeAll;
    }

    private void FixedUpdate() {
	    ExtrGravity();
    }

    void OnCollisionEnter(Collision col) {
        if(col.gameObject.tag == "Player" || col.gameObject.tag == "CarCivilian" || col.gameObject.tag == "Knockable") {
            collidedObject = col.gameObject;
            LaunchKnockAble();
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

    private void ExtrGravity() {
        if(rb.velocity.y < 0) rb.velocity -= Vector3.down * Physics.gravity.y * (4 - 1) * Time.fixedDeltaTime;
       // rb.velocity -= new Vector3(rb.velocity.x, -5, rb.velocity.z); 
    }

    public void LaunchKnockAble() {
        rb.constraints = RigidbodyConstraints.None;
        isStanding = false;
        rb.useGravity = true;
        onKnockPlayer.Invoke();
        onKnock.Invoke();

        if (collidedObject != null) {
            if (collidedObject.gameObject.tag == "Player") rb.AddForce(collidedObject.gameObject.GetComponent<Rigidbody>().velocity * 25f);
            if (collidedObject.gameObject.tag == "Knockable" && isStanding) rb.AddForce(collidedObject.gameObject.GetComponent<Rigidbody>().velocity * 5f);
        }

        if(addKnockedTag && photoItem != null) {
            photoItem.OverwriteTag("knocked");
            photoItem.sensation = 15;
        }

        collidedObject = null; 
    }
}