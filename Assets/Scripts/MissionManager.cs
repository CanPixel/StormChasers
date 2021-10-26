using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MissionManager : MonoBehaviour {
    [ReadOnly] public ObjectiveCriteria[] activeMission;
    [Space(10)]
    public List<Mission> missions = new List<Mission>();
    public bool freeRoam = false, startOnStart = false;
    public int missionIndex = 0;

    private static MissionManager missionManager;

    [System.Serializable]
    public class MissionCriteria {
        [SerializeField] private GameObject parent;
        [SerializeField] private Text description;
        [SerializeField] private Image checkmark;
        public bool finished = false;

        public void Load(ObjectiveCriteria crit) {
            finished = false;
            checkmark.enabled = false;
            parent.SetActive(true);
            description.text = crit.text;
        }

        public void Disable() {
            description.text = "";
            checkmark.enabled = false;
            finished = false;
            parent.SetActive(false);
        }

        public void Clear() {
            checkmark.enabled = true;
            finished = true;
        }
    }

    private bool startedMission = false;
    public AnimationCurve missionTextCurve;
    public float sequenceSpeed = 2f, sequenceDuration = 2f;

    [Space(10)]
    public Image glow;
    public Text missionName;
    public GameObject missionParent;
    public Text missionTitle;
    public Image missionIcon, missionCross;
    public RatingSystem ratingSystem;
    public LockOnSystem lockOnSystem;
    public MissionCriteria[] criteriaSplashes;

    private Mission currentObjective;
    private static float startSequence = 0;

    [System.Serializable]
    public class BonusObject {
        public string name = "Bonus";
        public PhotoItem item;
        public int bonus;
    }

    [System.Serializable]
    public class MissionObject {
        public string name;
        public PhotoItem objective;
        [Range(0, 1)]
        public float weight = 1;
    }
    [System.Serializable]
    public class MissionObjectWrapper {
        public MissionObject[] missionObjects;
    }

    [System.Serializable]
    public class ObjectiveCriteria {
        public string text = "Objective Criteria";
        public bool mainSubject = false;
        [ConditionalHide("mainSubject", false)] public bool optional = false;
        public bool penalty = false;
        [ReadOnly] public bool finished = false;
        [Space(5)]
        public bool editPhotoItemsManually = false;
        [ConditionalHide("editPhotoItemsManually", false)] public string photoItemNameKey;
        [ConditionalHide("editPhotoItemsManually", true)] public MissionObjectWrapper missionObjects;
    }

    [System.Serializable]
    public class Mission {
        public string name;
        public Sprite icon;

        public ObjectiveCriteria[] objectives;
        [HideInInspector] public bool active = false;
        [ReadOnly] public bool cleared = false;
        [ReadOnly] public bool marked = false;

        [Header("Criteria")]
        public bool centerFrameImportant = true;
        [ConditionalHide("centerFrameImportant", true)] [Range(0, 100)] public int minCenterFrame = 50;

        [Range(0, 100)] public int minMotionStability = 50, minSharpness = 50;
        public int minKeyPointsRequired = 1;
        public BonusObject[] bonuses;
        public BonusObject[] penalties;
        //public float maxDist, minDist;

        public void Enable(bool a) {
            foreach(var obj in objectives) {
                if(obj.editPhotoItemsManually) foreach(var i in obj.missionObjects.missionObjects) i.objective.active = active = a;
                else {
                    var l = MissionManager.EnableBySearch(obj.photoItemNameKey, a);
                    obj.missionObjects = new MissionObjectWrapper();
                    obj.missionObjects.missionObjects = new MissionObject[l.Count];
                    for(int i = 0; i < obj.missionObjects.missionObjects.Length; i++) {
                        obj.missionObjects.missionObjects[i] = new MissionObject();
                        obj.missionObjects.missionObjects[i].weight = 1f;
                        obj.missionObjects.missionObjects[i].objective = l[i];
                    }
                }
            }
            foreach(var i in bonuses) {
                i.item.active = true;
            }
        }
    }

    public void ScanMissionCompletion() {
        
    }

    public void CheckCompletion(CameraControl.PictureScore pic, CameraControl.Screenshot screen) {
        if(activeMission == null || activeMission.Length < 1 || pic.item == null) return;

        string str = "[" + pic.item.tags + "]";
        foreach(var i in pic.subScore) str += ", " + i.item.tags;

        bool[] missionCleared = new bool[activeMission.Length];
        for(int i = 0; i < activeMission.Length; i++) {
            bool isPenalty = activeMission[i].penalty;
            bool isOptional = activeMission[i].optional;
            string key = activeMission[i].photoItemNameKey.ToLower().Trim();

            if(activeMission[i].mainSubject && ComparePhotoItem(pic.item, key)) {
                missionCleared[i] = true;
                continue;
            }

            if(!activeMission[i].mainSubject) {
                if(isOptional) missionCleared[i] = true;
                else {
                    if(!isPenalty) {
                        foreach(var m in pic.subScore) {
                            if(ComparePhotoItem(m.item, key)) {
                                missionCleared[i] = true;
                                break;
                            }
                        }
                    } else {
                        missionCleared[i] = true;
                        foreach(var m in pic.subScore) {
                            if(ComparePhotoItem(m.item, key)) {
                                missionCleared[i] = false;
                                break;
                            }
                        }
                    }
                }
            }

            var end = "Result: ";
            bool finished = true;
            foreach(var m in missionCleared) {
                end += m + " ";
                if(!m) {
                    finished = false;
                    break;
                }
            }

          //  Debug.Log(end);
            if(finished) {
                for(int m = 0; m < activeMission.Length; m++) {
                    activeMission[m].finished = true;
                    criteriaSplashes[m].Clear();
                }
                screen.forMission = true;
                currentObjective.cleared = true;
            }
        }
    }

    public void MarkCurrentObjective() {
        currentObjective.marked = true;
    }

    protected bool ComparePhotoItem(PhotoBase item1, string key) {
        return item1.tags.ToLower().Trim().Contains(key.ToLower().Trim());
    }

    public static List<PhotoItem> EnableBySearch(string key, bool b) {
        List<PhotoItem> temp = new List<PhotoItem>();
        foreach(var i in missionManager.lockOnSystem.GetAllTargets()) {
            if(i.tags.Trim().ToLower().Contains(key.Trim().ToLower())) {
                i.active = b;
                temp.Add(i);
            }
        }
        return temp;
    }

    public void ActivateAll(bool j) {
        foreach(var i in missions) i.Enable(j);
        foreach(var i in lockOnSystem.GetAllTargets()) i.active = j;
    }

    private void SetCurrentMission(Mission miss) {
/*         if(freeRoam) {
            currentObjective = null;
            startSequence = sequenceDuration + 1;
            return;
        } */

        foreach(var i in lockOnSystem.GetAllTargets()) i.active = false;
        foreach(var i in missions) i.Enable(false);

        currentObjective = miss;
        startSequence = 0;
        currentObjective.active = true;

        missionIcon.enabled = true;
        missionTitle.text = miss.name;
        if(miss.icon != null) missionIcon.sprite = miss.icon;
        else missionIcon.enabled = false;

        missionCross.fillAmount = 0;

        for(int i = 0; i < criteriaSplashes.Length; i++) {
            if(miss.objectives.Length <= i) {
                criteriaSplashes[i].Disable();
                break;
            }
            criteriaSplashes[i].Load(miss.objectives[i]);
        }

        activeMission = miss.objectives;
        
        miss.Enable(true);
        startedMission = true;
    }
    public void StartMission(string name) {
        foreach(var i in missions) {
            if(i.cleared) continue;
            if(i.name.Trim() == name.Trim()) {
                SetCurrentMission(i);
                SoundManager.PlayUnscaledSound("PhotoShoot");
                SoundManager.PlayUnscaledSound("CamMode");
                return;
            }
        }
    }

    void OnValidate() {
        missionIndex = Mathf.Clamp(missionIndex, 0, missions.Count - 1);

        foreach(var i in missions) {
            //if(i..GetKeyPoints() != null && i.minKeyPointsRequired > i.focusedObject.GetKeyPoints().Length) i.minKeyPointsRequired = i.focusedObject.GetKeyPoints().Length;
            if(i.minKeyPointsRequired < 1) i.minKeyPointsRequired = 1;

            i.minCenterFrame = Mathf.Clamp(i.minCenterFrame, 0, 100);
        }
    }

    void Start() {
        missionManager = this;
        missionParent.transform.localScale = Vector3.zero;
        currentObjective = null;

        if(!freeRoam) {
            if(startOnStart) {
                SetCurrentMission(missions[missionIndex]);
                startedMission = true;
            } else ActivateAll(false);
        }
        else ActivateAll(true);
    }

    void Update() {
        if(startedMission) {
            startSequence += Time.unscaledDeltaTime * sequenceSpeed;

            if(startSequence > sequenceDuration) missionParent.transform.localScale = Vector3.Lerp(missionParent.transform.localScale, Vector3.one, Time.unscaledDeltaTime * 5f);
        }


        if(startSequence > sequenceDuration) {
            missionName.text = "";
        } else {
            if(currentObjective != null) missionName.text = currentObjective.name;
        }

        missionName.transform.localScale = missionTextCurve.Evaluate(startSequence) / 2f * Vector3.one;
        missionName.color = new Color(missionName.color.r, missionName.color.g, missionName.color.b, Mathf.Lerp(missionName.color.a, startSequence * 2f, Time.deltaTime * 8f * sequenceSpeed));
        glow.color = new Color(glow.color.r, glow.color.g, glow.color.b, missionTextCurve.Evaluate(startSequence));
    
        //Mission Cleared anim
        if(currentObjective != null && currentObjective.cleared) {
            missionCross.fillAmount = Mathf.Lerp(missionCross.fillAmount, (currentObjective.marked) ? 1 : 0, Time.unscaledDeltaTime * 4f);
        }
    }
}