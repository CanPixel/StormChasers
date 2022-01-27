using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
public class Knockable : MonoBehaviour {
    public bool knockableByShark = false;
    public bool addKnockedTag = true;
    private PhotoItem photoItem;
    private string baseName;
    private int baseSensation;
    
    public bool canbeExploded = true;
    public bool isInTornado = false; 
    private bool isStanding = true;
    private GameObject collidedObject;

    public UnityEvent onKnockShark, onKnockPlayer, onKnock;

    [HideInInspector] public Rigidbody rb;

    void Start() {
        gameObject.tag = "Knockable";
        photoItem = GetComponent<PhotoItem>();
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
	    rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeAll;
        isInTornado = false; 
        
        if(photoItem == null) return;
        baseSensation = photoItem.sensation;
        baseName = photoItem.tag;
    }

    private void FixedUpdate() {
        if(transform.position.y > -50f) ExtrGravity();
        DestroySelf(); 
        if (rb.useGravity) isInTornado = false; 
    }

    private void DestroySelf()
    {
        if(transform.position.y < -50f && !rb.isKinematic)
        {
            
            rb.velocity = new Vector3(0, 0, 0);        
            rb.isKinematic = true;       
        }
    }

    void OnCollisionEnter(Collision col) {
        if (!isInTornado) {
            if (col.gameObject.tag == "Player" || col.gameObject.tag == "CarCivilian" || col.gameObject.tag == "Knockable" || col.gameObject.tag == "Rhino") {
                if (col.gameObject.GetComponent<Rigidbody>().velocity.magnitude > 5f) {
                    collidedObject = col.gameObject;
                    LaunchKnockAble();

                    if(col.gameObject.tag == "Rhino") col.gameObject.GetComponent<RhinoAI>().BoostSensation();
                }
            }
        }
    }

    void OnTriggerEnter(Collider col) {
        if(col.gameObject.tag == "Shark" && knockableByShark && !isInTornado) {
            rb.useGravity = true;
            rb.constraints = RigidbodyConstraints.None;
            onKnockShark.Invoke();
            onKnock.Invoke();
            rb.AddForce(new Vector3(0.05f, 1f, 0) * 15000f);
            rb.AddTorque(new Vector3(10, 50, 5), ForceMode.Impulse);
        }
    }

    private void ExtrGravity() {
        if (!isStanding && rb.velocity.y < 3) rb.velocity -= Vector3.down * Physics.gravity.y * 3f * Time.fixedDeltaTime;
        // rb.velocity -= new Vector3(rb.velocity.x, -5, rb.velocity.z); 
    }

    public void LaunchKnockAble() {
        rb.constraints = RigidbodyConstraints.None;
        isStanding = false;
        isInTornado = false; 
        rb.useGravity = true;
        onKnockPlayer.Invoke();
        onKnock.Invoke();

        if (collidedObject != null) {
            if (collidedObject.gameObject.tag == "Player") rb.AddForce(collidedObject.gameObject.GetComponent<Rigidbody>().velocity * 25f);
            if (collidedObject.gameObject.tag == "Knockable" && isStanding) rb.AddForce(collidedObject.gameObject.GetComponent<Rigidbody>().velocity * 5f);
        }

        if(addKnockedTag && photoItem != null) {
            photoItem.tag = "Knocked " + baseName;
            photoItem.sensation = baseSensation + SensationScores.scores.knockableValue;
        }
        collidedObject = null; 
    }

    public void Explode() {
        if(photoItem == null) return;
        photoItem.tag = "Blasted tree!";
        photoItem.sensation = SensationScores.scores.knockableValue + 20;
    }
}