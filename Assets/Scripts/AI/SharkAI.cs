using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;

public class SharkAI : MonoBehaviour {
    public GameObject sharkPathObject;
    private Vector3 targetPos;
    public bool loopPath = false;
    private List<Transform> wayPointObjects = new List<Transform>();

    [System.Serializable]
    public enum SharkState {
        ROAMING, IDLE, PANIC
    }
    [ReadOnly] public SharkState sharkState = SharkState.ROAMING;

    [Header("Gizmos")]
    public bool showGizmos = true;
    public Gradient colorBlend;
    public float wayPointGizmosScale = 4;

    private int currentTarget = 0;

    [Header("References")]
    public NavMeshAgent navigation; 
    public Animator animator;

    void OnValidate() {
        wayPointGizmosScale = Mathf.Clamp(wayPointGizmosScale, 0, 50);
    }

    void OnDrawGizmos() {
        wayPointObjects.Clear();
        wayPointObjects = sharkPathObject.GetComponentsInChildren<Transform>().ToList();
        wayPointObjects.RemoveAt(0);

        if(!showGizmos) return;
        for(int i = 0; i < wayPointObjects.Count; i++) {
            float progress = ((float)(i+1) / (float)wayPointObjects.Count);
            Gizmos.color = colorBlend.Evaluate(progress);
            Gizmos.DrawSphere(wayPointObjects[i].position + Vector3.up, wayPointGizmosScale);
        }
    }

    void Start() {
        wayPointObjects = sharkPathObject.GetComponentsInChildren<Transform>().ToList();
        wayPointObjects.RemoveAt(0);
        SetWaypoint(0);
    }

    void Update() {
        
    }

    public void SetWaypoint(int i) {
        if(i >= wayPointObjects.Count) {
            sharkState = SharkState.IDLE;
            return;
        }
        SetTarget(wayPointObjects[i].position);
    }

    public void SetTarget(Transform trans) {
        SetTarget(trans.position);
    }
    public void SetTarget(Vector3 pos) {
        this.targetPos = pos;
        navigation.destination = pos;
    }

    void OnTriggerEnter(Collider col) {
        if(col.tag == "SharkWaypoint") {
            if(currentTarget >= wayPointObjects.Count && loopPath) {
                currentTarget = 0;
                SetWaypoint(currentTarget);
            }

            if(currentTarget < wayPointObjects.Count && col.transform == wayPointObjects[currentTarget]) {
                ++currentTarget;
                SetWaypoint(currentTarget);
            } 
        }
    }
}
