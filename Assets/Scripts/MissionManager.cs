using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MissionManager : MonoBehaviour {
    public AnimationCurve missionTextCurve;
    public Image glow;
    public Text missionName;
    public LockOnSystem lockOnSystem;
    public bool freeRoam = false;
    public float sequenceSpeed = 2f, sequenceDuration = 2f;
    public List<Mission> missions = new List<Mission>();

    [Space(10)]
    public int missionIndex = 0;

    [ReadOnly] public Mission currentObjective;
    private static float startSequence = 0;

    [System.Serializable]
    public class BonusObject {
        public string name = "Bonus";
        public PhotoItem item;
        public int bonus;
    }

    [System.Serializable]
    public class Mission {
        public string name;
        [HideInInspector] public bool active = false;

        public bool centerFrameImportant = true;
        [ConditionalHide("centerFrameImportant", true)] [Range(0, 100)] public int minCenterFrame = 50;

        [Range(0, 100)] public int minMotionStability = 50, minSharpness = 50;
        public PhotoItem focusedObject;
        public PhotoKeyPoint[] keyPoints;
        public int minKeyPointsRequired = 1;
        public BonusObject[] bonusObjects;

        //public float maxDist, minDist;
    }

    public void ActivateAll(bool j) {
        foreach(var i in missions) i.focusedObject.active = i.active = j;
        foreach(var i in lockOnSystem.GetAllTargets()) i.active = j;
    }

    public void SetCurrentMission(Mission miss, bool inEditor = false) {
        if(freeRoam) {
            currentObjective = null;
            startSequence = sequenceDuration + 1;
            return;
        }

        currentObjective = miss;
        startSequence = 0;
        currentObjective.active = true;

        if(inEditor) return;

        foreach(var i in lockOnSystem.GetAllTargets()) i.active = false;
        foreach(var i in missions) i.focusedObject.active = i.active = false;

        foreach(var i in missions) if(miss == i) {
            i.focusedObject.active = i.active = true;
            return;
        }
    }

    void OnValidate() {
        missionIndex = Mathf.Clamp(missionIndex, 0, missions.Count - 1);
        SetCurrentMission(missions[missionIndex], true);

        foreach(var i in missions) {
            if(i.focusedObject.isComposite) i.keyPoints = i.focusedObject.GetKeyPoints();
            else i.keyPoints = null;

            if(i.keyPoints != null && i.minKeyPointsRequired > i.keyPoints.Length) i.minKeyPointsRequired = i.keyPoints.Length;
            if(i.minKeyPointsRequired < 1) i.minKeyPointsRequired = 1;

            i.minCenterFrame = Mathf.Clamp(i.minCenterFrame, 0, 100);
        }
    }

    void Start() {
        if(!freeRoam) SetCurrentMission(missions[missionIndex]);
        else ActivateAll(true);
    }

    void Update() {
        startSequence += Time.unscaledDeltaTime * sequenceSpeed;

        if(startSequence > sequenceDuration) {
            missionName.text = "";
        } else {
            if(currentObjective != null) missionName.text = currentObjective.name;
        }

        missionName.transform.localScale = missionTextCurve.Evaluate(startSequence) / 2f * Vector3.one;
        missionName.color = new Color(missionName.color.r, missionName.color.g, missionName.color.b, Mathf.Lerp(missionName.color.a, startSequence * 2f, Time.deltaTime * 8f * sequenceSpeed));
        glow.color = new Color(glow.color.r, glow.color.g, glow.color.b, missionTextCurve.Evaluate(startSequence));
    }
}
