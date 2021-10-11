using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FakeParent : MonoBehaviour {
    public Transform target;
    public bool followPosition, followRotation;

    void Update() {
        if(followPosition) transform.position = target.position;
        if(followRotation) transform.rotation = target.rotation;
    }
}
