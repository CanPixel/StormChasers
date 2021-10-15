using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionManager : MonoBehaviour {
    public LockOnSystem lockOnSystem;
    [HideInInspector] public Mission currentObjective;
    public List<Mission> missions = new List<Mission>();

    [System.Serializable]
    public struct KeyPointArray {
        public PhotoKeyPoint[] keyList;
    }

    [System.Serializable]
    public class BonusObject {
        public string name = "Bonus";
        public PhotoItem item;
        public int bonus;
    }

    [System.Serializable]
    public class Mission {
        public string name;
        [Range(0, 100)]
        public int minCenterFrame = 50, minMotionStability = 50, minSharpness = 50;
        public bool centerFrameIsImportant = true;
        public bool isCompositeObject = false;
        [ConditionalHide("isCompositeObject", false)] public PhotoItem focusedObject;
        [ConditionalHide("isCompositeObject", true)] public KeyPointArray keyPoints;
        [ConditionalHide("isCompositeObject", true)] public int minKeyPointsRequired = 1;
        public BonusObject[] bonusObjects;

        public float maxDist, minDist;

/*         [System.Serializable]
        public class CriteriaWeight  {
            [HideInInspector] public RatingSystem.Criteria criteria;
            [Range(0, 100)]
            public float minimumScore = 1f;

            public CriteriaWeight(RatingSystem.Criteria criteria) {
                this.criteria = criteria;
            } */
        //}
    }

    void OnValidate() {
        foreach(var i in missions) {
            if(i.minKeyPointsRequired > i.keyPoints.keyList.Length) i.minKeyPointsRequired = i.keyPoints.keyList.Length;
            if(i.minKeyPointsRequired < 1) i.minKeyPointsRequired = 1;
        }
    }

    void Update() {
        
    }
}
