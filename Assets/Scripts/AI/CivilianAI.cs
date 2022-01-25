using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CivilianAI : MonoBehaviour {
    public GameObject roadPathsObject;
    private Transform[] roadPaths;
    private GameObject collidedObj;
    [HideInInspector] public bool isInTornado = false; 

    [ReadOnly] public bool flipped = false;

    private Vector2 retargetDuration = new Vector2(16, 30);

    public float reorientAfter = 10;
    private float reorientTime = 0;

    [Header("References")]
    public PhotoItem photoItem;
    private string baseName;
    private int baseSensation;
    public UnityEngine.AI.NavMeshAgent navigation; 
    public Rigidbody rb;

    private float time = 0, randomDuration;
    private bool flipping = false, chomped = false;

    private bool onScreen = false;

    void Start() {
        roadPaths = roadPathsObject.GetComponentsInChildren<Transform>();
        SetTarget(RandomLocation());
        baseSensation = photoItem.sensation;
        baseName = photoItem.tag;
    }

    public Vector3 RandomLocation() {
        return roadPaths[Random.Range(0, roadPaths.Length - 1)].position;
    }

    public void Chomp(Transform shark) {
        navigation.enabled = false;
        rb.useGravity = false;
        transform.SetParent(shark);
        transform.localPosition = Vector3.zero;
        chomped = true;
    }

    void OnDrawGizmosSelected() {
        if(navigation.destination != null) {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(navigation.destination, 15);
        }
    }

    void Update() {
        if(chomped || isInTornado) {
            photoItem.tag = "Car sucked into tornado!";
            photoItem.sensation = (baseSensation + SensationScores.scores.tornadoCarValue);
            return;
        }

        time += Time.deltaTime;

        flipped = Mathf.Abs(transform.rotation.z) > 0.25f;
        photoItem.tag = ((flipped ? "Flipped car! " : baseName));
        photoItem.sensation = flipped ? (baseSensation + SensationScores.scores.flippedCarValue) : baseSensation;
        
        if(time > randomDuration) {
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
        navigation.destination = pos;
        randomDuration = Random.Range(retargetDuration.x, retargetDuration.y);
    }

    void OnTriggerEnter(Collider col) {
        if(col.tag == "CarWaypoint") SetTarget(RandomLocation());
    }

    void OnCollisionEnter(Collision col) {
        if(col.gameObject.tag == "Player") {
            collidedObj = col.gameObject; 
            LaunchCivilian(); 
        }
    }

    public void LaunchCivilian()
    {
        var rb = GetComponent<Rigidbody>();
        navigation.enabled = false;
        isInTornado = false; 
        reorientTime = reorientAfter;
        rb.useGravity = true;
        rb.isKinematic = false;
        if(collidedObj != null)rb.AddForce(collidedObj.gameObject.GetComponent<Rigidbody>().velocity * 300f);
        collidedObj = null; 
    }
}
