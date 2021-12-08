using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarCosmetics : MonoBehaviour {
    public GameObject obj;
    public GameObject[] cigarExhaust;

    private bool wear = false;

    void Start() {
        obj.SetActive(false);
        transform.localPosition = new Vector3(0, 0, 4);
        transform.localScale = Vector3.one * 2;
    }

    void Update() {
        if(wear) {
            transform.localPosition = Vector3.Lerp(transform.localPosition, Vector3.zero, Time.deltaTime * 5f);
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one, Time.deltaTime * 7f);
        }
    }

    public void TriggerWearables() {
        obj.SetActive(true);
        wear = true;
        foreach(var i in cigarExhaust) i.SetActive(false);
    }
}
