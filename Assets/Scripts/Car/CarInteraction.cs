using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CarInteraction : MonoBehaviour {
    public bool engageMission = false;
    [ConditionalHide("engageMission", true)] public int missionIndex = 0;
    [ReadOnly] public MissionManager.Mission mission;

    private MissionManager missionManager;

    void Start() {
        if(missionManager == null) missionManager = MissionManager.missionManager;
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
                missionManager.ScanMissionCompletion(transform.position);
                missionManager.StartMission(mission);
            }
        }
    }
}
