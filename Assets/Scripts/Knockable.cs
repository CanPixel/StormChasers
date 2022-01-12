using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
public class Knockable : MonoBehaviour {
    public bool knockableByShark = false;
    public bool addKnockedTag = true;
    public bool canbeExploded = true; 
    private PhotoItem photoItem;
    private bool isStanding = true;
    private GameObject collidedObject; 
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

    private void FixedUpdate()
    {
        ExtrGravity(); 
    }

    void OnCollisionEnter(Collision col) {
        if(col.gameObject.tag == "Player" || col.gameObject.tag == "Knockable" || col.gameObject.tag == "Shark") {

            collidedObject = col.gameObject; 
            LaunchKnockAble(); 
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

    void ExtrGravity()
    {
        if(rb.velocity.y < 0) rb.velocity -= Vector3.down * Physics.gravity.y * (4 - 1) * Time.fixedDeltaTime;
       // rb.velocity -= new Vector3(rb.velocity.x, -5, rb.velocity.z); 
    
    }

    public void LaunchKnockAble()
    {
        rb.constraints = RigidbodyConstraints.None;
        isStanding = false;
        rb.useGravity = true;
        onKnockPlayer.Invoke();
        onKnock.Invoke();

        if (collidedObject != null)
        {
            if (collidedObject.gameObject.tag == "Player") rb.AddForce(collidedObject.gameObject.GetComponent<Rigidbody>().velocity * 25f);
            if (collidedObject.gameObject.tag == "Knockable" && isStanding) rb.AddForce(collidedObject.gameObject.GetComponent<Rigidbody>().velocity * 5f);
        }

        if (addKnockedTag && photoItem != null) photoItem.OverwriteTag("knocked");

        collidedObject = null; 
    }

    

    void StayStatic()
    {
           
    }
}
