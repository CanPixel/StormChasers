using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CivilianAI : MonoBehaviour {
    public GameObject roadPathsObject;
    private Transform[] roadPaths;
    private Vector3 targetPos;

    public Vector2 retargetDuration;

    [Header("References")]
    public UnityEngine.AI.NavMeshAgent navigation; 

    private float time = 0, randomDuration;

    void Start() {
        roadPaths = roadPathsObject.GetComponentsInChildren<Transform>();
        SetTarget(RandomLocation());
    }

    public Vector3 RandomLocation() {
        return roadPaths[Random.Range(0, roadPaths.Length)].position;
    }

    void Update() {
        time += Time.deltaTime;
        if(time > randomDuration) {
            SetTarget(RandomLocation());
            time = 0;
        }
    }

    public void SetTarget(Transform trans) {
        SetTarget(trans.position);
    }
    public void SetTarget(Vector3 pos) {
        this.targetPos = pos;
        navigation.destination = pos;
        randomDuration = Random.Range(retargetDuration.x, retargetDuration.y);
    }

    void OnTriggerEnter(Collider col) {
        if(col.tag == "CarWaypoint") SetTarget(RandomLocation());
    }
}
