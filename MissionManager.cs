using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
    [ConditionalHide("startOnStart", true), ReadOnly] public string missionOnStart;

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
    [Space(10)]
    public AnimationCurve missionTextCurve;
    public float sequenceSpeed = 2f, sequenceDuration = 2f;
    public Color missionMarkerColor, deliverMarkerColor;

    [Space(10)]
    public Image glow;
    public Text missionName;
    public GameObject missionParent;
    public float missionParentScale = 1.2f;
    public Text missionTitle, readyForMark;
    public Transform missionPhotoFinishDisplay;
    public Image missionIcon, missionCross;
    public RatingSystem ratingSystem;
    public UIBob journalSelectControl;
    [SerializeField] private InputActionReference portfolioButton;
    public LockOnSystem lockOnSystem;
    public CameraControl camControl;
    public MissionCriteria[] criteriaSplashes;

    private Mission currentObjective;
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
    }
    [System.Serializable]
    public class MissionObjectWrapper {
        public MissionObject[] missionObjects;
    }

    [System.Serializable]
    public class ObjectiveCriteria {
        public string text = "Objective Criteria";
        public bool optional = false, show = true;
        public bool penalty = false;
        public bool customMarkCompletion = false;

        public bool captureJustOnce = false;

        [ReadOnly] public bool finished = false;
        [Space(5)]
        public string photoItemNameKey;
        public UnityEvent OnPictureDoAction;
        [HideInInspector] public MissionObjectWrapper missionObjects;
    }

    [System.Serializable]
    public class Mission {
        public string name, description;
        public Sprite icon;

        public ObjectiveCriteria[] objectives;
        [HideInInspector] public bool active = false;
        [ReadOnly] public bool cleared = false, hasMarkedPicture = false, delivered = false;

        public bool oneShot = false;
        public bool bypassDelivery = false;

        //public string manuallyEnabledItemKeys;
        [System.Serializable]
        public enum MissionType {
            REGULAR, CHAOS_STACK
        }
        public MissionType objectiveType = MissionType.REGULAR;
        [ConditionalHide("objectiveType", 1)] public StackedTags stackedTags;
        
        [System.Serializable]
        public class StackedTags {
            public StackedTag[] tagDictionary;

            [System.Serializable]
            public class StackedTag {
                public string tag;
                public int value;
            }
        }

        [Space(5)]
        public CarInteraction[] triggerLocations;

        [Space(5)]
        public UnityEvent OnMissionStarted;
        public UnityEvent OnMissionFinished;
        public UnityEvent OnMissionDelivered;

        public void Enable(bool a) {
            foreach(var obj in objectives) {
               var l = MissionManager.EnableBySearch(obj.photoItemNameKey, a);
                obj.missionObjects = new MissionObjectWrapper();
                obj.missionObjects.missionObjects = new MissionObject[l.Count];
                for(int i = 0; i < obj.missionObjects.missionObjects.Length; i++) {
                    obj.missionObjects.missionObjects[i] = new MissionObject();
                    obj.missionObjects.missionObjects[i].objective = l[i];
                }
            }
        }
    }

    private float finishTime = 0;
    private bool afterFinishInvoked = false;

    public void Deliver() {
        if(currentObjective == null) return;
        currentObjective.delivered = true;
        currentObjective.OnMissionDelivered.Invoke();
        camControl.JournalFinish(currentObjective);
    }

    public void ShowReadyForMark(bool i) {
        readyForMark.gameObject.SetActive(i);
    }

    public void MarkForCurrentMission(bool mark) {
        if(currentObjective == null) return;
        currentObjective.hasMarkedPicture = mark;
        if(currentObjective.triggerLocations != null && currentObjective.triggerLocations.Length > 0) foreach(var i in currentObjective.triggerLocations) if(i != null) i.SetDeliveryStage(mark);
    }

    public void DiscardMissionPicture() {
        if(currentObjective == null) return;
        for(int m = 0; m < currentObjective.objectives.Length; m++) MarkCurrentObjective(m, false);
        currentObjective.hasMarkedPicture = false;
        readyForMark.gameObject.SetActive(false);
        foreach(var i in currentObjective.triggerLocations) i.SetDeliveryStage(false);
        currentObjective.cleared = false;
    }

    protected void AutoComplete() {
        if(currentObjective == null || !currentObjective.cleared || !currentObjective.hasMarkedPicture || camControl.missionPicture == null) return;
        camControl.DeliverPicture(currentObjective, null);
    }

    public void ScanMissionCompletion(Transform suckToPos) {
        if(currentObjective == null || !currentObjective.cleared || !currentObjective.hasMarkedPicture || camControl.missionPicture == null) return;
        camControl.DeliverPicture(suckToPos);
        foreach(var i in currentObjective.triggerLocations) i.CompleteMission();
        readyForMark.gameObject.SetActive(false);
    }

    public bool CheckCompletion(CameraControl.PictureScore pic, CameraControl.Screenshot screen) {
        if(activeCriteria == null || currentObjective == null || activeCriteria.Length < 1) return false;

        //Reset
        if(currentObjective.oneShot) for(int m = 0; m < activeCriteria.Length; m++) MarkCurrentObjective(m, false);
        
        //if(pic.item == null) return false;

        //combining all items into one list
/*         List<PhotoBase> items = new List<PhotoBase>();
        items.Add(pic.item);
        foreach(var m in pic.subScore) items.Add(m.item); */

        bool[] missionCleared = new bool[activeCriteria.Length];
        bool completed = true;
        for(int i = 0; i < activeCriteria.Length; i++) {
            bool isPenalty = activeCriteria[i].penalty;
            bool isOptional = activeCriteria[i].optional;
            string key = activeCriteria[i].photoItemNameKey.ToLower().Trim();

            if(activeCriteria[i].captureJustOnce && activeCriteria[i].finished) continue;

            //if(activeCriteria[i].objectiveType == ObjectiveCriteria.ObjectiveType.STACKED) Debug.Log();


          /*   if(ComparePhotoItem(pic.item, key)) {
                missionCleared[i] = true;
                activeCriteria[i].OnPictureDoAction.Invoke();
            }
            else { */
                if(isOptional) {
                    missionCleared[i] = true;
                    foreach(var item in screen.picturedObjects) if(ComparePhotoItem(item.objectReference, key)) {
                        activeCriteria[i].OnPictureDoAction.Invoke();
                        break;
                    }
                }
                else {
                    if(isPenalty) {
                        missionCleared[i] = true;
                      //  foreach(var m in pic.subScore) {
                            foreach(var item in screen.picturedObjects) {
                                if(ComparePhotoItem(item.objectReference, key)) {
                                    missionCleared[i] = false;
                                    break;
                                }
                            }
                        //}
                    } else {
                       // foreach(var m in pic.subScore) {
                        foreach(var item in screen.picturedObjects) {
                            if(ComparePhotoItem(item.objectReference, key)) {
                                activeCriteria[i].OnPictureDoAction.Invoke();
                                missionCleared[i] = true;
                                break;
                            }
                        }
                        //}
                    }
                }
 //           }

            completed = true;
            for(int m = 0; m < missionCleared.Length; m++) if(!missionCleared[m]) {
                completed = false;
                break;
            }

            //Mission Checkmarks
            for(int m = 0; m < activeCriteria.Length; m++) {
                if(!missionCleared[m] || activeCriteria[m].customMarkCompletion) continue;
                MarkCurrentObjective(m, true);
                camControl.JournalMarkObjective(m);
            }

            bool shouldFinishMark = true;
            int customMarkID = -1;
            for(int m = 0; m < missionCleared.Length; m++) {
                if(m >= activeCriteria.Length) break;
                if(activeCriteria[m].customMarkCompletion) customMarkID = m;
                if(!activeCriteria[m].finished && !activeCriteria[m].customMarkCompletion && !activeCriteria[m].optional) shouldFinishMark = false;
            }
            if(shouldFinishMark) {
                if(customMarkID >= 0) {
                    MarkCurrentObjective(customMarkID, true);
                    camControl.JournalMarkObjective(customMarkID);
                }
                completed = true;
            }
        }

        if(completed) {
            currentObjective.OnMissionFinished.Invoke();
            screen.missionReference = currentObjective;
            screen.forMission = true;
            readyForMark.gameObject.SetActive(true);
            if(currentObjective != null) {
                currentObjective.cleared = true;
                journalSelectControl.Bob();
            }
            if(currentObjective.bypassDelivery) CompleteMission(currentObjective);
        }
        return completed;
    }

    public static void CompleteMission(Mission miss) {
        missionManager.MarkForCurrentMission(true);
        missionManager.AutoComplete();
        miss.delivered = true;
        miss.OnMissionDelivered.Invoke();
    }

    /*
    
    public bool CheckCompletion(CameraControl.PictureScore pic, CameraControl.Screenshot screen) {
        if(activeCriteria == null || currentObjective == null || activeCriteria.Length < 1) return false;

        //Reset
        if(currentObjective.oneShot) for(int m = 0; m < activeCriteria.Length; m++) MarkCurrentObjective(m, false);
        

        if(pic.item == null) return false;

        bool[] missionCleared = new bool[activeCriteria.Length];
        bool completed = true;
        for(int i = 0; i < activeCriteria.Length; i++) {
            bool isPenalty = activeCriteria[i].penalty;
            bool isOptional = activeCriteria[i].optional;
            string key = activeCriteria[i].photoItemNameKey.ToLower().Trim();

            if(activeCriteria[i].captureJustOnce && activeCriteria[i].finished) continue;

            //if(activeCriteria[i].objectiveType == ObjectiveCriteria.ObjectiveType.STACKED) Debug.Log();

            if(ComparePhotoItem(pic.item, key)) {
                missionCleared[i] = true;
                activeCriteria[i].OnPictureDoAction.Invoke();
            }
            else {
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
                            if(ComparePhotoItem(m.item, key)) {
                                activeCriteria[i].OnPictureDoAction.Invoke();
                                missionCleared[i] = true;
                                break;
                            }
                        }
                    }
                }
            }

            completed = true;
            for(int m = 0; m < missionCleared.Length; m++) if(!missionCleared[m]) {
                completed = false;
                break;
            }

            //Mission Checkmarks
            for(int m = 0; m < activeCriteria.Length; m++) {
                if(!missionCleared[m] || activeCriteria[m].customMarkCompletion) continue;
                MarkCurrentObjective(m, true);
                camControl.JournalMarkObjective(m);
            }

            bool shouldFinishMark = true;
            int customMarkID = -1;
            for(int m = 0; m < missionCleared.Length; m++) {
                if(m >= activeCriteria.Length) break;
                if(activeCriteria[m].customMarkCompletion) customMarkID = m;
                if(!activeCriteria[m].finished && !activeCriteria[m].customMarkCompletion && !activeCriteria[m].optional) shouldFinishMark = false;
            }
            if(shouldFinishMark) {
                if(customMarkID >= 0) {
                    MarkCurrentObjective(customMarkID, true);
                    camControl.JournalMarkObjective(customMarkID);
                }
                completed = true;
            }
        }
        if(completed) {
            currentObjective.OnMissionFinished.Invoke();
            screen.missionReference = currentObjective;
            screen.forMission = true;
            readyForMark.gameObject.SetActive(true);
            if(currentObjective != null) {
                currentObjective.cleared = true;
                journalSelectControl.Bob();
            }
            if(currentObjective.bypassDelivery) {
                MarkForCurrentMission(true);
                AutoComplete();
                currentObjective.delivered = true;
                currentObjective.OnMissionDelivered.Invoke();
            }
        }
        return completed;
    }

     */

    public bool IsCurrentMissionBypassDelivery() {
        return currentObjective != null && currentObjective.bypassDelivery;
    }

    public void MarkCurrentObjectives() {
        for(int i = 0; i < activeCriteria.Length; i++) MarkCurrentObjective(i, true);
    }

    public void MarkCurrentObjective(int m, bool finished) {
        if(activeCriteria.Length <= m) return;
        activeCriteria[m].finished = finished;
        if(criteriaSplashes.Length - 1 < m) return;

        if(finished) criteriaSplashes[m].Clear();
        else criteriaSplashes[m].Reset();
    }

    protected bool ComparePhotoItem(PhotoBase item, string key) {
        return item.tags.ToLower().Trim() == key.ToLower().Trim() || item.tags.ToLower().Trim().Contains(key.ToLower().Trim());
    }

    public static List<PhotoItem> EnableBySearch(string key, bool b) {
        List<PhotoItem> temp = new List<PhotoItem>();
        foreach(var i in missionManager.lockOnSystem.GetAllTargets()) {
            if(i != null && i.staticTags != null && key.Trim().ToLower().Contains(i.staticTags.Trim().ToLower())) {
                i.active = b;
                temp.Add(i);
            }
        }
        return temp;
    }

    public void ActivateAll(bool j) {
        foreach(var i in missions) i.Enable(j);
        foreach(var i in lockOnSystem.GetAllTargets()) if(i != null) i.active = j;
    }

    public void SetCurrentMission(Mission miss, bool alreadyInJournal = false) {
        camControl.DeliverReset();

/*         foreach(var i in lockOnSystem.GetAllTargets()) if(i != null) i.active = false;
        foreach(var i in missions) i.Enable(false); */

        readyForMark.gameObject.SetActive(false);

        currentObjective = miss;
        currentObjective.delivered = miss.delivered = false;
        if(!alreadyInJournal) startSequence = 0;
        else startSequence = sequenceDuration;
        currentObjective.active = true;
        activeMission = miss;
        miss.OnMissionStarted.Invoke();

        missionIcon.enabled = true;
        missionTitle.text = miss.name;
        if(miss.icon != null) missionIcon.sprite = miss.icon;
        else missionIcon.enabled = false;

        missionCross.fillAmount = 0;

        foreach(var i in criteriaSplashes) i.Disable();
        for(int i = 0; i < miss.objectives.Length; i++) if(miss.objectives[i].show) criteriaSplashes[i].Load(miss.objectives[i]);

        activeCriteria = miss.objectives;
        
        //miss.Enable(true);
        startedMission = true;
    }
    public void StartMission(string name) {
        var i = InvokeMission(name);
        if(i == null || i.cleared /* || currentObjective == i*/ || missionsByName.ContainsKey(i.name.ToLower().Trim())) return;

        missionsByName.Add(i.name.Trim().ToLower(), i);
        camControl.AddMissionToJournal(i);
        camControl.ForceSelect(i);
        camControl.JournalReorient();
    }
    private Mission InvokeMission(string name) {
        foreach(var i in missions) {
            if(i.cleared) continue;
            if(i.name.Trim() == name.Trim()) {
                SetCurrentMission(i, false);
                SoundManager.PlayUnscaledSound("PhotoShoot");
                SoundManager.PlayUnscaledSound("CamMode");
                return i;
            }
        }
        return null;
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
        missionOnStart = missions[missionIndex].name;
    }

    void Start() {
        missionManager = this;
        missionParent.transform.localScale = Vector3.zero;
        currentObjective = null;
        readyForMark.gameObject.SetActive(false);

        if(startOnStart) {
            StartMission(missions[missionIndex], false);
            startedMission = true;

            if(activateAllPhotoItems) ActivateAll(true);
        } else {
            if(activateAllPhotoItems) ActivateAll(true);
            else ActivateAll(false);
        }
    }

    void Update() {
        if(finishTime > 0) finishTime -= Time.deltaTime;
        else if(currentObjective != null) {
            if(!afterFinishInvoked) {
                //currentObjective.AfterMissionDelivered.Invoke();
                afterFinishInvoked = true;
            }
        }

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
        afterFinishInvoked = false;
        finishTime = 0;
    }
}