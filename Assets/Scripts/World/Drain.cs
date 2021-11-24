using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drain : MonoBehaviour {
    public Transform target;
    public GameObject blastMesh;

    [Header("References")]
    public GameObject zibra;
    public GameObject rubbleParticle;
    public GameObject waterGeyser;
    public Cinemachine.CinemachineImpulseSource impulseSource;

    public void TriggerShark() {
        waterGeyser.SetActive(true);
        rubbleParticle.SetActive(true);
        impulseSource.GenerateImpulseAt(transform.position, Vector3.one * 2);
    }

    public void Detrigger() {
        waterGeyser.SetActive(false);
        target = null;
    }

    void OnTriggerEnter(Collider col) {
        if(col.tag == "CarCivilian") target = col.transform;
    }

    void OnTriggerExit(Collider col) {
        if(col.tag == "CarCivilian") target = null;
    }
}
