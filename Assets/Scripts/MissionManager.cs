using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class MissionManager : MonoBehaviour {
    [ReadOnly] public Mission activeMission; 
    private ObjectiveCriteria[] activeCriteria;
    [Space(10)]
    public List<Mission> missions = new List<Mission>();
    private Dictionary<string, Mission> missionsByName = new Dictionary<string, Mission>();
    public bool startOnStart = false, activateAllPhotoItems = false;
    public int missionIndex = 0;

    public static MissionManager missionManager;

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

        public void Reset() {
            checkmark.enabled = false;
            finished = false;
        }
    }

    private bool startedMission = false;
    public AnimationCurve missionTextCurve;
    public float sequenceSpeed = 2f, sequenceDuration = 2f;
    public Color missionMarkerColor, deliverMarkerColor;

    [Space(10)]
    public Image glow;
    public Text missionName;
    public GameObject missionParent;
    public float missionParentScale = 1.2f;
    public Text missionTitle, readyForMark;
    public Image missionIcon, missionCross;
    public RatingSystem ratingSystem;
    [SerializeField] private InputActionReference portfolioButton;
    public LockOnSystem lockOnSystem;
    public CameraControl camControl;
    public MissionCriteria[] criteriaSplashes;

    private Mission currentObjective;
    private bool currentFinished = false;
    private static float startSequence = 0;

    private float postDeliveredTime = 0;

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
        //public bool mainSubject = false;
        //[ConditionalHide("mainSubject", false)] 
        public bool optional = false;
        public bool penalty = false;
        [ReadOnly] public bool finished = false;
        [Space(5)]
        public bool editPhotoItemsManually = false;
        [ConditionalHide("editPhotoItemsManually", false)] public string photoItemNameKey;
        [ConditionalHide("editPhotoItemsManually", true)] public MissionObjectWrapper missionObjects;
    }

    [System.Serializable]
    public class Mission {
        public string name, description;
        public Sprite icon;

        public ObjectiveCriteria[] objectives;
        [HideInInspector] public bool active = false;
        [ReadOnly] public bool cleared = false, hasMarkedPicture = false, delivered = false;

        [Space(5)]
        public CarInteraction[] triggerLocations;

        [Header("Criteria")]
        public bool centerFrameImportant = true;
        [ConditionalHide("centerFrameImportant", true)] [Range(0, 100)] public int minCenterFrame = 50;

        [Range(0, 100)] public int minMotionStability = 50, minSharpness = 50;
        public int minKeyPointsRequired = 1;
        public BonusObject[] bonuses;
        public BonusObject[] penalties;

        [Space(5)]
        public UnityEvent OnMissionFinished;

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
            foreach(var i in bonuses) i.item.active = true;
        }
    }

    public void Deliver() {
        if(currentObjective == null) return;
        currentObjective.delivered = true;
        currentObjective.OnMissionFinished.Invoke();
        camControl.JournalFinish();
    }

    public void ShowReadyForMark(bool i) {
        readyForMark.gameObject.SetActive(i);
    }

    public void MarkForCurrentMission(bool mark) {
        if(currentObjective == null) return;
        currentObjective.hasMarkedPicture = mark;
        if(currentObjective.triggerLocations != null && currentObjective.triggerLocations.Length > 0) foreach(var i in currentObjective.triggerLocations) i.SetDeliveryStage(mark);
    }

    public void DiscardMissionPicture() {
        if(currentObjective == null) return;
        for(int m = 0; m < currentObjective.objectives.Length; m++) MarkCurrentObjective(m, false);
        currentObjective.hasMarkedPicture = false;
        readyForMark.gameObject.SetActive(false);
        foreach(var i in currentObjective.triggerLocations) i.SetDeliveryStage(false);
        currentObjective.cleared = false;
    }

    public void ScanMissionCompletion(Vector3 suckToPos) {
        if(currentObjective == null || !currentObjective.cleared || !currentObjective.hasMarkedPicture || camControl.missionPicture == null) return;
        camControl.DeliverPicture(suckToPos);
        foreach(var i in currentObjective.triggerLocations) i.CompleteMission();
        readyForMark.gameObject.SetActive(false);
    }

    public bool CheckCompletion(CameraControl.PictureScore pic, CameraControl.Screenshot screen) {
        if(activeCriteria == null || activeCriteria.Length < 1) return false;

        //Reset
        if(!currentFinished) for(int m = 0; m < activeCriteria.Length; m++) MarkCurrentObjective(m, false);

        if(pic.item == null) return false;

        bool fullCompletion = false;

        //List<PhotoBase> items = new List<PhotoBase>();
        //items.Add(pic.item);
        //string str = "[" + pic.item.tags + "]";
        //foreach(var i in pic.subScore) items.Add(i.item);
        //Debug.Log(items.Count);

        bool[] missionCleared = new bool[activeCriteria.Length];
        for(int i = 0; i < activeCriteria.Length; i++) {
            bool isPenalty = activeCriteria[i].penalty;
            bool isOptional = activeCriteria[i].optional;
            string key = activeCriteria[i].photoItemNameKey.ToLower().Trim();

            if(/* activeCriteria[i].mainSubject &&*/ ComparePhotoItem(pic.item, key)) {
                missionCleared[i] = true;
            } else {
            // if(!activeMission[i].mainSubject) {
                if(isOptional) missionCleared[i] = true;
                else {
                        if(isPenalty) {
                        missionCleared[i] = true;
                        foreach(var m in pic.subScore) {
                            if(ComparePhotoItem(m.item, key) || ComparePhotoItem(pic.item, key)) {
                                missionCleared[i] = false;
                                break;
                            }
                        }
                    } else {
                        foreach(var m in pic.subScore) {
                            Debug.Log(m.item.name);
                            if(ComparePhotoItem(m.item, key)) {
                                missionCleared[i] = true;
                                break;
                            }
                        }
                    }
                }
            }
    //    }

            bool completed = true;
            foreach(var m in missionCleared) if(!m) completed = false;

            //Mission Checkmarks
            for(int m = 0; m < activeCriteria.Length; m++) {
                if(!missionCleared[m]) continue;
          /*       activeMission[m].finished = true;
                criteriaSplashes[m].Clear(); */
                MarkCurrentObjective(m, true);
                camControl.JournalMarkObjective(m);
            }

            if(completed) {
                fullCompletion = true;
                currentFinished = true;
                screen.missionReference = currentObjective;
                screen.forMission = true;
                readyForMark.gameObject.SetActive(true);
                if(currentObjective != null) currentObjective.cleared = true;
            } //else for(int m = 0; m < activeMission.Length; m++) MarkCurrentObjective(m, false);
        }
        return fullCompletion;
    }

    public void MarkCurrentObjective(int m, bool finished) {
        activeCriteria[m].finished = finished;
        if(finished) criteriaSplashes[m].Clear();
        else criteriaSplashes[m].Reset();
    }

    protected bool ComparePhotoItem(PhotoBase item, string key) {
        return item.tags.ToLower().Trim() == key.ToLower().Trim() || item.tags.ToLower().Trim().Contains(key.ToLower().Trim());
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

    public void SetCurrentMission(Mission miss, bool alreadyInJournal = false) {
        foreach(var i in lockOnSystem.GetAllTargets()) i.active = false;
        foreach(var i in missions) i.Enable(false);

        readyForMark.gameObject.SetActive(false);

        currentObjective = miss;
        if(!alreadyInJournal) startSequence = 0;
        else startSequence = sequenceDuration;
        currentFinished = false;
        currentObjective.active = true;
        activeMission = miss;

        missionIcon.enabled = true;
        missionTitle.text = miss.name;
        if(miss.icon != null) missionIcon.sprite = miss.icon;
        else missionIcon.enabled = false;

        missionCross.fillAmount = 0;

        for(int i = 0; i < criteriaSplashes.Length; i++) {
            if(miss.objectives.Length <= i) {
                criteriaSplashes[i].Disable();
                continue;
            }
            criteriaSplashes[i].Load(miss.objectives[i]);
        }

        activeCriteria = miss.objectives;
        
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
    public void StartMission(Mission i, bool sounds = true) {
        if(i.cleared || currentObjective == i || missionsByName.ContainsKey(i.name.ToLower().Trim())) return;
        SetCurrentMission(i);

        if(sounds) {
            SoundManager.PlayUnscaledSound("PhotoShoot");
            SoundManager.PlayUnscaledSound("CamMode");
        }

        missionsByName.Add(i.name.Trim().ToLower(), i);
        camControl.AddMissionToJournal(i);
        camControl.ForceSelect(i);
    }

    void OnValidate() {
        missionIndex = Mathf.Clamp(missionIndex, 0, missions.Count - 1);

        foreach(var i in missions) {
            if(i.minKeyPointsRequired < 1) i.minKeyPointsRequired = 1;
            i.minCenterFrame = Mathf.Clamp(i.minCenterFrame, 0, 100);
        }
    }

    void Start() {
        missionManager = this;
        missionParent.transform.localScale = Vector3.zero;
        currentObjective = null;
        readyForMark.gameObject.SetActive(false);

        if(startOnStart) {
            StartMission(missions[missionIndex], false);
            startedMission = true;
        } else {
            if(activateAllPhotoItems) ActivateAll(true);
            else ActivateAll(false);
        }
    }

    void Update() {
        if(startedMission) {
            startSequence += Time.unscaledDeltaTime * sequenceSpeed;
            if(startSequence > sequenceDuration) missionParent.transform.localScale = Vector3.Lerp(missionParent.transform.localScale, Vector3.one * ((currentObjective != null && currentObjective.cleared) ? (missionParentScale + 0.2f) : missionParentScale), Time.unscaledDeltaTime * 5f);
        } else missionParent.transform.localScale = Vector3.Lerp(missionParent.transform.localScale, Vector3.zero, Time.unscaledDeltaTime * 5f);

        if(startSequence > sequenceDuration) missionName.text = "";
        else if(currentObjective != null) missionName.text = "<color='#ff0000'>" + currentObjective.name + "</color>\nAdded to journal!";

        missionName.transform.localScale = missionTextCurve.Evaluate(startSequence) / 2f * Vector3.one;
        missionName.color = new Color(missionName.color.r, missionName.color.g, missionName.color.b, Mathf.Lerp(missionName.color.a, startSequence * 2f, Time.deltaTime * 8f * sequenceSpeed));
        glow.color = new Color(glow.color.r, glow.color.g, glow.color.b, missionTextCurve.Evaluate(startSequence));
    
        //Mission Cleared anim
         if(currentObjective != null && currentObjective.cleared) {
            missionCross.fillAmount = Mathf.Lerp(missionCross.fillAmount, (currentObjective.delivered) ? 1.1f : 0, Time.unscaledDeltaTime * 4f);

            if(currentObjective.delivered) {
                postDeliveredTime += Time.unscaledDeltaTime;
                if(postDeliveredTime > 3) HideMission();
            }
         }
    }

    protected void HideMission() {
        currentObjective.active = false;
        currentObjective = null;
        startSequence = 0;
        activeMission = null;
        startedMission = false;
    }
}