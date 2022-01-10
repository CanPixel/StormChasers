using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
public class Knockable : MonoBehaviour {
    public bool knockableByShark = false;
    public bool addKnockedTag = true;
    private PhotoItem photoItem;
    private bool isStanding = true;
    public Rigidbody rb; 

    public UnityEvent onKnockShark, onKnockPlayer, onKnock;

    void Start() {
        gameObject.tag = "Knockable";
        rb = this.gameObject.GetComponent<Rigidbody>(); 
        photoItem = GetComponent<PhotoItem>();
        rb.constraints = RigidbodyConstraints.FreezeAll;
        rb.useGravity = false;
        rb.isKinematic = false;
    }

    void OnCollisionEnter(Collision col) {
        if(col.gameObject.tag == "Player" || col.gameObject.tag == "Knockable" || col.gameObject.tag == "Shark") {
            rb.constraints = RigidbodyConstraints.None;
            isStanding = false;
            rb.useGravity = true;
            onKnockPlayer.Invoke();
            onKnock.Invoke();


            if(col.gameObject.tag == "Player")rb.AddForce(col.gameObject.GetComponent<Rigidbody>().velocity * 25f);
            //else if(col.gameObject.tag == "Player" && !isStanding) rb.AddForce(col.gameObject.GetComponent<Rigidbody>().velocity * 5f);
            if (col.gameObject.tag == "Knockable" && isStanding) rb.AddForce(col.gameObject.GetComponent<Rigidbody>().velocity * 5f);

            if (addKnockedTag && photoItem != null) photoItem.OverwriteTag("knocked");
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

    private void FixedUpdate()
    {
        //if (isStanding) rb.velocity = new Vector3(0, 0, 0);
        //if(isStanding) rb.velocity = new Vector3(0, 0, 0);
    }

    void StayStatic()
    {
           
    }
}
