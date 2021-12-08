using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floods : MonoBehaviour {
    public float startAtY = -9;

    public GameObject water;
    private float timer = 0;
    
    void Start() {
        water.SetActive(false);
        water.transform.localPosition = new Vector3(0, startAtY, 0);
    }

    void Update() {
        if(timer > 0) {
            timer += Time.deltaTime;
            if(timer > 4) water.transform.localPosition = Vector3.Lerp(water.transform.localPosition, Vector3.zero, Time.deltaTime * 0.5f);
        }
    }

    public void Flood() {
        water.SetActive(true);
        timer = 0.1f;
    }
}
