using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour {
    public Vector3 dir;
    
    void Update() {
        transform.rotation *= Quaternion.Euler(dir.x * Time.deltaTime, dir.y * Time.deltaTime, dir.z * Time.deltaTime);
    }
}
