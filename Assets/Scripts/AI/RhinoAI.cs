using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RhinoAI : MonoBehaviour {
    public Rigidbody rb;
    public Transform target;
    public float moveSpeed = 20f;
    [Range(0f, 2f)]
    public float turnSpeed = 1f;

    public bool move = true, rotate = true;

    private Vector3 targetPos;

    public Transform neck;
    public Vector3 neckOffset = new Vector3(0, 0, 0);

    void Update() {
        Vector3 dir = transform.forward;
        if(target != null) dir = (target.position - transform.position);
        else if(targetPos != null) dir = (targetPos - transform.position);

        if(rotate) transform.rotation = Quaternion.LookRotation(GetRotation(dir, turnSpeed));
        if(move) rb.velocity += Time.deltaTime * moveSpeed * transform.forward;
        ExtrGravity();

        if(target != null) {
            neck.LookAt(target);
            neck.rotation *= Quaternion.Euler(neckOffset);
        }
    }

    private void ExtrGravity() {
        if (rb.velocity.y < 3) rb.velocity -= Vector3.down * Physics.gravity.y * 3f * Time.fixedDeltaTime;
    }

    public Vector3 GetRotation(Vector3 dir, float turnSpeed) {
        var tar = Vector3.RotateTowards(transform.forward, dir, turnSpeed * Time.deltaTime, 0);
        tar.y = 0;
        return tar;
    }
}
