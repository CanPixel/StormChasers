using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour {
    [System.Serializable]
    public class CameraSystem {
        public float aim, shoot;
    
        public override string ToString() {
            return "Aim: " + aim + " || Shoot: " + shoot;
        }
    }
    public CameraSystem camSys;

    void Update() {
        Debug.Log(camSys);
    }
}
