using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;
using MoreMountains.Feedbacks;

public class TornadoAI : MonoBehaviour {
    public GameObject pathRoaming;
    private Vector3 targetPos;
    public bool loopPath = false;
    private List<Transform> roamingWayPoints = new List<Transform>();
    private List<SharkAttackPoint> huntingPoints = new List<SharkAttackPoint>();
    
    private CivilianAI civilianCasualty;

    public float TimeUntilRoaming = 1f;

    [Header("Gizmos")]
    public bool showGizmos = true;
    public Gradient colorBlend;
    public float wayPointGizmosScale = 4;

    private int currentTarget = 0;

    [Header("References")]
    public NavMeshAgent navigation; 
    public MMFeedbacks feedback;
    public FollowTargetFromTop markerFollow;
    public MeshRenderer marker;
    public Transform chompingPosition;

    private float wait = 0;

    private float huntDelay = 0;

    void OnValidate() {
        wayPointGizmosScale = Mathf.Clamp(wayPointGizmosScale, 0, 50);
    }

    void OnDrawGizmos() {
        roamingWayPoints.Clear();
        roamingWayPoints = pathRoaming.GetComponentsInChildren<Transform>().ToList();
        roamingWayPoints.RemoveAt(0);

        if(!showGizmos) return;
        for(int i = 0; i < roamingWayPoints.Count; i++) {
            float progress = ((float)(i+1) / (float)roamingWayPoints.Count);
            Gizmos.color = colorBlend.Evaluate(progress);
            Gizmos.DrawSphere(roamingWayPoints[i].position + Vector3.up, wayPointGizmosScale);
        }
    }

    void Start() {
        roamingWayPoints.Clear();
        huntingPoints.Clear();
        roamingWayPoints = pathRoaming.GetComponentsInChildren<Transform>().ToList();
        roamingWayPoints.RemoveAt(0);
        wait = TimeUntilRoaming;
    }

    void Update() {
        if(wait > 0) wait -= Time.deltaTime;
        else {

        }
    }

    public void SetWaypoint(int i) {
        if(i >= roamingWayPoints.Count) {
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

    void OnTriggerStay(Collider col) {
        if(col.tag == "SharkWaypoint") {
            if(currentTarget >= roamingWayPoints.Count && loopPath) {
                currentTarget = 0;
                SetWaypoint(currentTarget);
            }
            if(currentTarget < roamingWayPoints.Count && col.transform == roamingWayPoints[currentTarget]) {
                ++currentTarget;
                SetWaypoint(currentTarget);
            }
        }
    }
}
