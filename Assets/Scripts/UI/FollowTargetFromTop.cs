using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTargetFromTop : MonoBehaviour {
    public Transform target;
    public Vector3 offset, rotation;

    public float freq = 5, amp = 1;

    void Update() {
        if(target == null) return;
        transform.position = target.position + offset + new Vector3(0, Mathf.Sin(Time.time * freq)  * amp, 0);
        transform.rotation = Quaternion.Euler(rotation);
    }
}
