using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EvilDead : MonoBehaviour {
    [Header("Looking")]
    public float LookDistance;
    public float lerpDuration = 2;
    private float targetDistance;
    public Transform leftEyePoint;
    public Transform rightEyePoint;
    public Transform leftEyeOrigin;
    public Transform rightEyeOrigin;
    public Transform hatObject;
    public Transform tilter;

    [Header("Laser")]
    private float laserCooldownTimer;
    public float laserCooldownDuration = 2; 
    public float laserLength = 100f; 
    private bool canLaser;
    public float laserOffset = 2f;
    public float laserLerpSpeed = 4f;

    [ReadOnly] public bool hatIsGone = false;
    public Transform tornadoCenter;
    public GameObject hat;

    [Header("Rotation")]
    public float headTilt = 22;
    public float headRotationSpeed = 0.4f;
    private float headRotationTimer = 0;
    public Vector3 offset = new Vector3(-90, 0, 0);

    [Header ("Components")]
    public LineRenderer leftLaser;
    public LineRenderer rightLaser;
    public Transform target;
    public Transform rotationPoint;
    public Transform leftEye;
    public Transform rightEye;
    public MeshRenderer eyeMeshr;
    public Material searchingEyeMat;
    public Material angryEyeMat;
    public Light leftSpotlight;
    public Light rightSpotlight; 

    private Vector3 lookPos;

    [ReadOnly] public CurrentState headState;

    [HideInInspector]
    public enum CurrentState {
        DISABLED,
        SEARCH,
        LASER,
        IDLE,
        DESTROYED
    }

    private void Start() {
        headState = CurrentState.IDLE;
        leftLaser.positionCount = 2; 
        rightLaser.positionCount = 2;
       
        eyeMeshr.material = searchingEyeMat;
        leftSpotlight.color = rightSpotlight.color = Color.yellow;
    }

    private void Update() {
        switch (headState) {
            case CurrentState.DISABLED:          
                break;
            case CurrentState.IDLE:
                CheckForTarget(); 
                lookPos.x = 0;
                lookPos.y = 0;
                lookPos.z += headRotationSpeed;
                break;
            case CurrentState.LASER:
                CheckForTarget();
                if(canLaser) LaserTarget();
                break; 
        }

        var tiltDest = Quaternion.Euler(0, 0, 0);
        if(headState == CurrentState.LASER) {
            Vector3 direction = target.position - transform.position;
            Quaternion toRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Lerp(transform.rotation, toRotation * Quaternion.Euler(offset), lerpDuration * Time.deltaTime);
        } else {
            tiltDest = Quaternion.Euler(headTilt, 0, 0);
            transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(lookPos), Time.deltaTime * lerpDuration);
        }

        tilter.localRotation = Quaternion.Lerp(tilter.localRotation, tiltDest, Time.deltaTime * lerpDuration);
    }

    public void CheckForTarget() {
        //eye raycasts
        if (Physics.Raycast(leftEyeOrigin.position , leftEye.forward, out RaycastHit hitLeft, laserLength)) {
            if(hitLeft.collider.gameObject.CompareTag("Ground")) leftEyePoint.transform.position = hitLeft.point; 
        }
        if (Physics.Raycast(rightEyeOrigin.position, rightEye.forward, out RaycastHit hitRight, laserLength)) {
            if (hitRight.collider.gameObject.CompareTag("Ground")) rightEyePoint.transform.position = hitRight.point;         
        }

        //Check for targets
        Collider[] hitColliders = Physics.OverlapSphere(rightEyePoint.position, 15f);
        foreach (Collider col in hitColliders) {
            if(col.CompareTag("EvilHead")) continue;

            if (col.CompareTag("Player")) {
                target = col.gameObject.transform;
                eyeMeshr.material = angryEyeMat;
                leftSpotlight.color = rightSpotlight.color = Color.red;
            }
        }
        Collider[] hit2Colliders = Physics.OverlapSphere(leftEyePoint.position, 15f);
        foreach (Collider col in hit2Colliders) {
            if(col.CompareTag("EvilHead")) continue;

            if (col.CompareTag("Player")) {
                target = col.gameObject.transform;
                eyeMeshr.material = angryEyeMat;
                leftSpotlight.color = rightSpotlight.color = Color.red;
            }
        }

        //Tornado steals hat from head
        hatIsGone = (hatObject.parent == null);
        if (target != null)
        {
            eyeMeshr.material = angryEyeMat;
            leftSpotlight.color = rightSpotlight.color = Color.red;
            if(hatIsGone) target = tornadoCenter;
        }

        //If there isn't a target go into idle 
        if (target == null && !canLaser) {
            //Rotate head randomly
            headRotationTimer -= Time.deltaTime;
            if (headRotationTimer <= 0) {
                headRotationSpeed *= -1;
                headRotationTimer = Random.Range(5f, 11f);
            }
        }
        
        //If there is a target
        if (target == null) return;
        targetDistance = Vector3.Distance(target.position, transform.position);

        //Target is player
        if (target.CompareTag("Player")) {
            leftLaser.enabled = true;
            rightLaser.enabled = true;

            laserCooldownTimer += Time.deltaTime;
            headState = CurrentState.LASER;

            if (laserCooldownTimer >= laserCooldownDuration) canLaser = true; 
        }
        
        //Target is out of sight 
        if(target == null || targetDistance > LookDistance) {
            target = null;
            canLaser = false;
            leftLaser.enabled = false;
            rightLaser.enabled = false;
            eyeMeshr.material = searchingEyeMat;
            leftSpotlight.color = rightSpotlight.color = Color.yellow;
            headState = CurrentState.IDLE; 
        }       
    }

    public void LaserTarget() {
        Vector3 laserOffsetV = new Vector3(laserOffset, 0, 0);

        leftEye.LookAt(target);
        rightEye.LookAt(target);

        //Set lasers L/R tornado
        if (hatIsGone) {
            leftLaser.SetPosition(0, leftEye.position);
            leftLaser.SetPosition(1, tornadoCenter.position - laserOffsetV);
            rightLaser.SetPosition(0, rightEye.position);
            rightLaser.SetPosition(1, tornadoCenter.position + laserOffsetV);
        }
        else { //Set lasers L/R 
            leftLaser.SetPosition(0, leftEye.position);
            leftLaser.SetPosition(1, Vector3.Lerp(leftLaser.GetPosition(1), target.position - laserOffsetV, Time.deltaTime * laserLerpSpeed));

            rightLaser.SetPosition(0, rightEye.position);
            rightLaser.SetPosition(1, Vector3.Lerp(rightLaser.GetPosition(1), target.position + laserOffsetV, Time.deltaTime * laserLerpSpeed));
        }
    }
}