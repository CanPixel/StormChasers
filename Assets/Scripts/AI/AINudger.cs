using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AINudger : MonoBehaviour {
    void OnTriggerEnter(Collider col) {
        if(col.tag == "CarCivilian") {
            var ai = col.GetComponent<CivilianAI>();
            if(ai != null) ai.SetTarget(transform.position);
        }
    }
}
