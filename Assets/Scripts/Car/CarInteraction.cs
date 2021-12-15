using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Mission = MissionManager.Mission;

public class CarInteraction : MonoBehaviour {
    public DialogChar character;
    public MeshRenderer missionMarker;
    public Mission mission;

    void Start() {
       //if(missionMarker != null) SetMissionMarkerColor(missionManager.missionMarkerColor);
    }

    public UnityEvent onEnter;

    void OnTriggerEnter(Collider col) {
        if(col.tag == "Player") {
            onEnter.Invoke();
           // if(MissionManager.missionManager.activeMission == mission) MissionManager.missionManager.ScanMissionCompletion(transform);
            MissionManager.missionManager.StartMission(mission);
        } 
    }

/*     protected void SetMissionMarkerColor(Color col) {
        if(mission != null && missionMarker != null) {
            missionMarker.material.SetColor("_Color", col);
            missionMarker.material.SetColor("_EmissionColor", col);
        }
    } */

    /* public void SetDeliveryStage(bool i) {
        if(missionMarker == null || missionMarker.gameObject == null) return;
        if(i) SetMissionMarkerColor(MissionManager.missionManager.deliverMarkerColor);   
        else SetMissionMarkerColor(MissionManager.missionManager.missionMarkerColor);   
    } */

    public void CompleteMission() {
        if(missionMarker != null && missionMarker.gameObject != null) Destroy(missionMarker.gameObject);
    }
}
