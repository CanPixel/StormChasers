using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RhinoAI : MonoBehaviour {
    public Rigidbody rb;
    public Transform target;
    public float speed;

    private Vector3 targetPos;

    void FixedUpdate() {
        Vector3 dir = Vector3.zero;
        if(target != null) dir = (transform.position - target.position).normalized;
        else if(targetPos != null) dir = (transform.position - targetPos).normalized;
        rb.velocity += Time.deltaTime * speed * dir;
    }
}
