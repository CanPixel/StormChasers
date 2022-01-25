using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RhinoAI : MonoBehaviour {
    public PhotoItem pi;
    private int baseSensation;
    private string baseName;
    public float sensationBoostDecayAfterKnock = 3;
    private float knockTime = 0;

    public Rigidbody rb;
    public Transform target;
    public float moveSpeed = 20f;
    [Range(0f, 2f)]
    public float turnSpeed = 1f;

    public bool move = true, rotate = true;

    private Vector3 targetPos;

    public Transform neck;
    public Vector3 neckOffset = new Vector3(0, 0, 0);

    void Start() {
        baseSensation = pi.sensation;
        baseName = pi.tag;
    }

    void Update() {
        Vector3 dir = transform.forward;
        if(target != null) dir = (target.position - transform.position);
        else if(targetPos != null) dir = (targetPos - transform.position);

        if(rotate) transform.rotation = Quaternion.LookRotation(GetRotation(dir, turnSpeed));
        if(move) {
            var mov = moveSpeed * transform.forward;
            mov.y = 0;
            rb.velocity += Time.deltaTime * mov;
        }

        if(target != null) {
            neck.LookAt(target);
            neck.rotation *= Quaternion.Euler(neckOffset);
        }

        if(knockTime > 0) {
            knockTime -= Time.deltaTime;
            pi.sensation = baseSensation + SensationScores.scores.rhinoKnockBoostValue;
            pi.tag = baseName + " destroys!";
        } else {
            pi.sensation = baseSensation;
            pi.tag = baseName;
        }
    }

    public Vector3 GetRotation(Vector3 dir, float turnSpeed) {
        var tar = Vector3.RotateTowards(transform.forward, dir, turnSpeed * Time.deltaTime, 0);
        tar.y = 0;
        return tar;
    }

    public void BoostSensation() {
        knockTime = sensationBoostDecayAfterKnock;
    }
}
