using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CarInteraction : MonoBehaviour {
    public bool engageMission = false;
    [ConditionalHide("engageMission", true)] public int missionIndex = 0;
    [ConditionalHide("engageMission", true)] public MeshRenderer missionMarker;
    [ConditionalHide("engageMission", true)] public GameObject[] destroyOnMissionComplete;
    [ReadOnly] public MissionManager.Mission mission;

    private MissionManager missionManager;

    void Start() {
        if(missionManager == null) missionManager = MissionManager.missionManager;

        SetMissionMarkerColor(missionManager.missionMarkerColor);
    }

    void OnValidate() {
        if(missionManager == null) missionManager = GameObject.FindGameObjectWithTag("MissionManager").GetComponent<MissionManager>();
        var missions = missionManager.missions;
        missionIndex = Mathf.Clamp(missionIndex, 0, missions.Count - 1);
        mission = missions[missionIndex];
    }

    public UnityEvent onEnter;

    void OnTriggerEnter(Collider col) {
        if(col.tag == "Player") {
            onEnter.Invoke();
            if(engageMission) {
                if(missionManager.activeMission == mission) missionManager.ScanMissionCompletion(transform.position);
                missionManager.StartMission(mission);
            }
        }
    }

    protected void SetMissionMarkerColor(Color col) {
        if(mission != null && missionMarker != null) {
            missionMarker.material.SetColor("_Color", col);
            missionMarker.material.SetColor("_EmissionColor", col);
        }
    }

    public void SetDeliveryStage(bool i) {
        if(missionMarker == null || missionMarker.gameObject == null) return;
        if(i) SetMissionMarkerColor(missionManager.deliverMarkerColor);   
        else SetMissionMarkerColor(missionManager.missionMarkerColor);   
    }

    public void CompleteMission() {
        foreach(var i in destroyOnMissionComplete) Destroy(i.gameObject);
        if(missionMarker != null && missionMarker.gameObject != null) Destroy(missionMarker.gameObject);
    }
}
