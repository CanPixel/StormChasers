using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CivilianAI : MonoBehaviour {
    public GameObject roadPathsObject;
    private Transform[] roadPaths;
    private Vector3 targetPos;
    public GameObject parentObj;

    [ReadOnly] public bool flipped = false;

    public Vector2 retargetDuration;

    public float reorientAfter = 10;
    private float reorientTime = 0;

    [Header("References")]
    public PhotoItem photoItem;
    public UnityEngine.AI.NavMeshAgent navigation; 
    public Rigidbody rb;

    private float time = 0, randomDuration;
    private bool flipping = false, chomped = false;

    private bool onScreen = false;

    void Start() {
        roadPaths = roadPathsObject.GetComponentsInChildren<Transform>();
        SetTarget(RandomLocation());
    }

    public Vector3 RandomLocation() {
        return roadPaths[Random.Range(0, roadPaths.Length)].position;
    }

    public void Chomp(Transform shark) {
        navigation.enabled = false;
        rb.useGravity = false;
        transform.SetParent(shark);
        transform.localPosition = Vector3.zero;
        chomped = true;
    }

    void Update() {
        if(chomped) return;

        time += Time.deltaTime;

        flipped = Mathf.Abs(transform.rotation.z) > 0.25f;
        photoItem.OverwriteTag(photoItem.staticTags + (flipped ? " flippedcar " : ""));
        
        //onScreen = LockOnSystem.OnScreen(transform.position);
        if(time > randomDuration /* && !onScreen && navigation.destination == null*/) {
            SetTarget(RandomLocation());
            time = 0;
        }

        if(reorientTime <= 0) {
            navigation.enabled = true;
            if(flipping) {
                if(navigation.destination == null) SetTarget(RandomLocation());
                flipping = false;
            }
        }
        else {
            reorientTime -= Time.deltaTime;
            if(reorientTime < reorientAfter / 2f) {
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, transform.eulerAngles.y, 0), Time.deltaTime * 4f);
                flipping = true;
            }
        }
    }

    public void SetTarget(Transform trans) {
        SetTarget(trans.position);
    }
    public void SetTarget(Vector3 pos) {
        if(navigation == null || !navigation.enabled) return;
        this.targetPos = pos;
        navigation.destination = pos;
        randomDuration = Random.Range(retargetDuration.x, retargetDuration.y);
    }

    void OnTriggerEnter(Collider col) {
        if(col.tag == "CarWaypoint") SetTarget(RandomLocation());
    }

    void OnCollisionEnter(Collision col) {
        if(col.gameObject.tag == "Player") {
            var rb = GetComponent<Rigidbody>();
            navigation.enabled = false;
            reorientTime = reorientAfter;
            rb.useGravity = true;
            rb.isKinematic = false;
            rb.AddForce(col.gameObject.GetComponent<Rigidbody>().velocity * 400f);
        }
    }
}
