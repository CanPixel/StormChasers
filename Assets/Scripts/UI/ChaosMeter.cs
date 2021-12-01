using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaosMeter : MonoBehaviour {
    public GameObject meter;
    
    void Start() {
        meter.SetActive(false);
    }

    void Update() {
        
    }

    public void StartChaos() {
        meter.SetActive(true);
    }
}
