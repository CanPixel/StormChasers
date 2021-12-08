using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;
using MoreMountains.Feedbacks;

public class SharkAI : MonoBehaviour {
    public GameObject sharkPathRoaming, sharkPathHunting;
    private Vector3 targetPos;
    public bool loopPath = false;
    private List<Transform> roamingWayPoints = new List<Transform>();
    private List<SharkAttackPoint> huntingPoints = new List<SharkAttackPoint>();
    
    public bool talkingShark = false;
    public float talkInterval = 0.5f;

    private GameObject drain;
    private Drain lastDrain;
    private CivilianAI civilianCasualty;

    public float TimeUntilRoaming = 1f;

    [Header("Hunting State")]
    public float huntInterval = 5;
    private float huntTimer = 0;
    private bool isHunting = false, isChomping = false;
    public AnimationCurve sharkLungeY;
    public AnimationCurve sharkBarrelRoll;
    public float chompSpeed = 1f;
    private float chompTimer = 0;
    public float drainJumpFactor = 9, sharkJumpFactor = 1;

    [System.Serializable]
    public enum SharkState {
        IDLE = 0, ROAMING = 1, HUNTING = 2, CHASING = 3
    }
    [ReadOnly] public SharkState sharkState = SharkState.IDLE;

    [Header("Gizmos")]
    public bool showGizmos = true;
    public Gradient colorBlend;
    public float wayPointGizmosScale = 4;

    private int currentTarget = 0;

    [Header("References")]
    public NavMeshAgent navigation; 
    public Animator huntingSharkAnim;
    public MMFeedbacks feedback;
    public FollowTargetFromTop markerFollow;
    public MeshRenderer marker;
    public Transform chompingPosition;

    private float wait = 0;
    private float baseY;

    private float huntDelay = 0;

    void OnValidate() {
        wayPointGizmosScale = Mathf.Clamp(wayPointGizmosScale, 0, 50);
    }

    void OnDrawGizmos() {
        roamingWayPoints.Clear();
        huntingPoints.Clear();
        roamingWayPoints = sharkPathRoaming.GetComponentsInChildren<Transform>().ToList();
        huntingPoints = sharkPathHunting.GetComponentsInChildren<SharkAttackPoint>().ToList();
        roamingWayPoints.RemoveAt(0);

        if(!showGizmos) return;
        for(int i = 0; i < roamingWayPoints.Count; i++) {
            float progress = ((float)(i+1) / (float)roamingWayPoints.Count);
            Gizmos.color = colorBlend.Evaluate(progress);
            Gizmos.DrawSphere(roamingWayPoints[i].position + Vector3.up, wayPointGizmosScale);
        }

        for(int i = 0; i < huntingPoints.Count; i++) {
            float progress = ((float)(i+1) / (float)huntingPoints.Count);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(huntingPoints[i].transform.position + Vector3.up, wayPointGizmosScale * 1.5f);
        }
    }

    void Start() {
        roamingWayPoints.Clear();
        huntingPoints.Clear();
        roamingWayPoints = sharkPathRoaming.GetComponentsInChildren<Transform>().ToList();
        huntingPoints = sharkPathHunting.GetComponentsInChildren<SharkAttackPoint>().ToList();
        roamingWayPoints.RemoveAt(0);
        baseY = huntingSharkAnim.transform.localPosition.y;
        wait = TimeUntilRoaming;
    }

    void Update() {
        if(wait > 0) wait -= Time.deltaTime;
        else {
            switch(sharkState) {
                case SharkState.IDLE: 
                    SetWaypoint(0);
                    sharkState = SharkState.ROAMING;
                    break;
                case SharkState.ROAMING: 
                    RoamingUpdate();
                    break;
                case SharkState.HUNTING: 
                    HuntingUpdate();
                    break;
                case SharkState.CHASING: 
                    ChasingUpdate();
                    break;
            }
        }
    }

    private float talkTime = 0;
    private bool talkToggle = false;
    protected void RoamingUpdate() {
        if(talkingShark) {
            huntingSharkAnim.Play("Babbel");

            talkTime += Time.deltaTime;
            if(talkTime > talkInterval) {
                talkToggle = !talkToggle;
                huntingSharkAnim.SetBool("MouthOpen", talkToggle);
                huntingSharkAnim.SetBool("MouthClosed", !talkToggle);
                talkTime = 0;
            }
        }
    }
    protected void HuntingUpdate() {
        if(huntDelay > 0) huntDelay -= Time.deltaTime;

        marker.enabled = (!isChomping);

        if(!isHunting) huntTimer += Time.deltaTime;
        if(huntTimer > huntInterval) {
            isHunting = true;
            var atP = huntingPoints[Random.Range(0, huntingPoints.Count)];
            markerFollow.target = atP.transform;
            SetTarget(atP.transform);
            huntTimer = 0;
        }

        if(isChomping) {
            chompTimer += Time.deltaTime * chompSpeed;

            //Exit State
            if(chompTimer >= 1) {
                isChomping = false;
                huntTimer = 0;
                huntingSharkAnim.SetBool("MouthOpen", false); 
                huntingSharkAnim.SetBool("MouthClosed", true); //
                drain.transform.localPosition = Vector3.zero;
                drain.transform.localRotation = Quaternion.Euler(-90, 0, 0);
                feedback.enabled = false;
                isHunting = false;
                lastDrain.Detrigger();
                
                // Respawning cars is essential !!!!!!!!!!!!!!!!!1
                if(civilianCasualty != null) {
                    if(civilianCasualty.parentObj != null) Destroy(civilianCasualty.parentObj);
                    Destroy(civilianCasualty.gameObject);
                }
                //


                civilianCasualty = null;
                lastDrain = null;
                drain = null;
            }

            huntingSharkAnim.SetBool("MouthOpen", chompTimer <= 0.8f); //
            huntingSharkAnim.SetBool("MouthClosed", chompTimer > 0.8f);

            huntingSharkAnim.transform.localPosition = new Vector3(huntingSharkAnim.transform.localPosition.x, baseY + sharkLungeY.Evaluate(chompTimer) * sharkJumpFactor, huntingSharkAnim.transform.localPosition.z);
            huntingSharkAnim.transform.rotation = Quaternion.Lerp(huntingSharkAnim.transform.rotation, Quaternion.Euler((chompTimer > 0.5f) ? 90 : -90, sharkBarrelRoll.Evaluate(chompTimer) * 360f, 0), Time.deltaTime * 6f);

            if(drain != null) {
                if(chompTimer < 0.7f) drain.transform.Rotate(200f * Time.deltaTime, 0, 0);
                else drain.transform.localRotation = Quaternion.Lerp(drain.transform.localRotation, Quaternion.Euler(-90, 0, 0), Time.deltaTime * 12f);
                drain.transform.localPosition = new Vector3(0, Mathf.Clamp(sharkLungeY.Evaluate(chompTimer) * drainJumpFactor, 0, float.MaxValue), 0);
            }
        }
    }
    protected void ChasingUpdate() {
    }

    public void SetState(string sharkoState) {
        foreach(SharkState state in System.Enum.GetValues(typeof(SharkState))) if(state.ToString().ToLower().Trim() == sharkoState.Trim().ToLower()) {
            sharkState = state;
            return;
        }
    }

    public void SetWaypoint(int i) {
        if(i >= roamingWayPoints.Count) {
            sharkState = SharkState.IDLE;
            return;
        }
        SetTarget(roamingWayPoints[i].position);
    }

    public void SetTarget(Transform trans) {
        SetTarget(trans.position);
    }
    public void SetTarget(Vector3 pos) {
        this.targetPos = pos;
        navigation.destination = pos;
    }

    void OnTriggerExit(Collider col) {
         switch(sharkState) {
                case SharkState.HUNTING:
                    var sap = col.GetComponent<SharkAttackPoint>();
                    if(sap != null && sap.drain != null) sap.drain.shouldNudgeAI = false;
                    break;
         }
    }

    void OnTriggerStay(Collider col) {
        if(col.tag == "SharkWaypoint") {
            switch(sharkState) {
                case SharkState.ROAMING:
                    if(currentTarget >= roamingWayPoints.Count && loopPath) {
                        currentTarget = 0;
                        SetWaypoint(currentTarget);
                    }
                    if(currentTarget < roamingWayPoints.Count && col.transform == roamingWayPoints[currentTarget]) {
                        ++currentTarget;
                        SetWaypoint(currentTarget);
                    }
                    break;

                case SharkState.HUNTING:
                    if(isChomping) break;
                    var sap = col.GetComponent<SharkAttackPoint>();
                    if(sap != null && sap.drain != null) sap.drain.shouldNudgeAI = true;

                    if(sap != null && sap.drain != null && sap.drain.target != null && huntDelay <= 0) {
                        drain = sap.drain.blastMesh;
                        lastDrain = sap.drain;
                        huntDelay = 2;
                        chompTimer = 0;
                        civilianCasualty = sap.drain.target.GetComponent<CivilianAI>();
                        if(civilianCasualty != null) {
                            civilianCasualty.photoItem.OverwriteTag("sharkvictim");
                            civilianCasualty.Chomp(chompingPosition);
                            sap.drain.TriggerShark();
                            feedback.enabled = true;
                            huntingSharkAnim.SetBool("MouthOpen", true);
                            huntingSharkAnim.transform.position = sap.drain.transform.position + new Vector3(0, -1.5f, 0);
                            huntingSharkAnim.transform.rotation = Quaternion.Euler(-90, 0, 0);
                            isChomping = true;
                        }
                    }
                    break;
            }
        }
    }
}
