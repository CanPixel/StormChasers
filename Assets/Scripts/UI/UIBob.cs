using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBob : MonoBehaviour {
    public float bobSpeed = 1f, bobFactor = 1.25f;
    public Vector3 baseScale = new Vector3(1, 1, 1);
    private Vector3 bobScale;

    private bool bob = false;
    private float bobTime = 0;

    public bool onEnable = false;

    void Start() {
        bobScale = baseScale * bobFactor;
    }

    void OnEnable() {
        if(onEnable) Bob();
    }

    void Update() {
        if(bob) {
            bobTime += Time.deltaTime * bobSpeed * 2f;
            transform.localScale = Vector3.Lerp(bobScale, baseScale, bobTime);

            var dist = (transform.localScale - baseScale);
            dist = new Vector3(Mathf.Abs(dist.x), Mathf.Abs(dist.y), Mathf.Abs(dist.z));
            if(Mathf.Abs(dist.magnitude) <= 0) {
                bobTime = 0;
                bob = false;
                transform.localScale = baseScale;
            } 
        }
    }

    public void Bob() {
        bobTime = 0;
        transform.localScale = bobScale;
        bob = true;
    }
}
