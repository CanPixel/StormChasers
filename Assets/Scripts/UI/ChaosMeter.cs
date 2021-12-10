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

    public void CalculateChaos(CameraControl.PictureScore pic, CameraControl.Screenshot screen, MissionManager.Mission miss) {
        
    }

    public void StartChaos() {
        meter.SetActive(true);
    }
}
