using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EvilDead : MonoBehaviour
{
    [Header("Looking")]
    public float LookDistance;
    public float lerpDuration = 2;
    private float targetDistance;
    public Transform leftEyePoint;
    public Transform rightEyePoint;
    public Transform tilter;

    [Header("Laser")]
    public float laserDuration;
    public float laserCooldownTimer;
    public float laserCooldownDuration;
    private float laserSpeed;
    public float laserLength = 50f;
    public bool canLaser;

    public bool hatIsGone;
    public Transform tornadoCenter;
    public GameObject hat;

    [Header("Rotation")]
    public float headTilt = 22;
    public float headRotationSpeed = 5;
    private float headRotationTimer;
    public Vector3 offset = new Vector3(0, 0, 0);

    [Header("Components")]
    public LineRenderer leftLaser;
    public LineRenderer rightLaser;
    public Transform target;
    public Transform fakeTarget;
    public Transform rotationPoint;
    //public Transform idleLookAtPoint;
    public Transform leftEye;
    public Transform rightEye;
    public MeshRenderer eyeMeshr;
    public Material searchingEyeMat;
    public Material angryEyeMat;
    public Light leftSpotlight;
    public Light rightSpotlight;

    private Vector3 lookPos;

    private CurrentState headState;

    [HideInInspector]
    private enum CurrentState
    {
        DISABLED,
        SEARCH,
        LASER,
        IDLE,
        DESTROYED,
    }

    private void Start()
    {
        headRotationTimer = Random.Range(8, 12);
        headState = CurrentState.IDLE;
        leftLaser.positionCount = 2;
        rightLaser.positionCount = 2;

        leftEyePoint.transform.position = leftEye.transform.position;
        leftEyePoint.transform.eulerAngles = leftEye.transform.eulerAngles;

        rightEyePoint.transform.position = rightEye.transform.position;
        rightEyePoint.transform.eulerAngles = rightEye.transform.eulerAngles;

        eyeMeshr.material = searchingEyeMat;

        leftSpotlight.color = rightSpotlight.color = Color.yellow;
    }

    private void Update()
    {
        switch (headState)
        {
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
                if (canLaser) LaserTarget(target.position);
                break;
        }

        var tiltDest = Quaternion.Euler(0, 0, 0);
        if (headState == CurrentState.LASER)
        {
            Vector3 direction = target.position - transform.position;
            Quaternion toRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Lerp(transform.rotation, toRotation * Quaternion.Euler(offset), lerpDuration * Time.deltaTime);
        }
        else
        {
            tiltDest = Quaternion.Euler(headTilt, 0, 0);

            transform.localRotation = Quaternion.Lerp(transform.localRotation,
            Quaternion.Euler(lookPos)
            , Time.deltaTime * lerpDuration);
        }

        tilter.localRotation = Quaternion.Lerp(tilter.localRotation, tiltDest, Time.deltaTime * lerpDuration);
    }

    public void CheckForTarget()
    {
        //Left eye raycast
        if (Physics.Raycast(leftEyePoint.position, leftEye.forward, out RaycastHit hitLeft, laserLength))
        {
            if (hitLeft.collider.gameObject.CompareTag("Ground")) leftEyePoint.transform.position = hitLeft.point;
        }

        //Right eye raycast
        if (Physics.Raycast(rightEyePoint.position, rightEye.forward, out RaycastHit hitRight, laserLength))
        {
            if (hitRight.collider.gameObject.CompareTag("Ground")) rightEyePoint.transform.position = hitRight.point;
        }

        //Check for targets
        Collider[] hitColliders = Physics.OverlapSphere(rightEyePoint.position, 15f);
        foreach (Collider col in hitColliders)
        {
            if (col.CompareTag("Player"))
            {
                target = col.gameObject.transform;
                //canLaser = true;
                eyeMeshr.material = angryEyeMat;
                leftSpotlight.color = rightSpotlight.color = Color.red;
            }
        }
        Collider[] hit2Colliders = Physics.OverlapSphere(leftEyePoint.position, 15f);
        foreach (Collider col in hit2Colliders)
        {
            if (col.CompareTag("Player"))
            {
                target = col.gameObject.transform;
                //canLaser = true;
                eyeMeshr.material = angryEyeMat;
                leftSpotlight.color = rightSpotlight.color = Color.red;
            }
        }

        //Tornado steals hat from head
        if (hat.transform.parent.name != "Tilt") hatIsGone = true;
        if (hatIsGone)
        {
            eyeMeshr.material = angryEyeMat;
            leftSpotlight.color = rightSpotlight.color = Color.red;
            target = tornadoCenter;
        }

        //If there isn't a target go into idle 
        if (target == null && !canLaser)
        {
            //Rotate head randomly
            headRotationTimer -= Time.deltaTime;
            if (headRotationTimer <= 0)
            {
                headRotationSpeed *= -1;
                headRotationTimer = Random.Range(4f, 10f);
            }
        }

        //If there is a target
        if (target == null) return;
        targetDistance = Vector3.Distance(target.position, transform.position);

        //Target is player
        if (target.CompareTag("Player") || hatIsGone)
        {
            leftLaser.enabled = true;
            rightLaser.enabled = true;
            //targetRotPos = transform.forward;

            laserCooldownTimer += Time.deltaTime;
            headState = CurrentState.LASER;

            if (laserCooldownTimer >= laserCooldownDuration) canLaser = true;
        }

        //Target is out of sight 
        if (target == null || targetDistance > LookDistance)
        {
            target = null;
            canLaser = false;
            hatIsGone = false;
            leftLaser.enabled = false;
            rightLaser.enabled = false;
            //            transform.eulerAngles = defaultRotation;
            eyeMeshr.material = searchingEyeMat;
            leftSpotlight.color = rightSpotlight.color = Color.yellow;
            headState = CurrentState.IDLE;
            //Debug.Log("Return To idle");
        }
    }

    void OnDrawGizmosSelected()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(rightEyePoint.position, 15);
    }

    /*     public void LookAtTarget(Vector3 targetRotPos) {
            //Lerp target position???????
            if (lerpTimer > lerpDuration && Vector3.Distance(targetRotPos, target.position) > 3)
            {

                //lerpTimer += Time.deltaTime;
            }
            else
            {

            }

            //Rotate head towards player 
            float targetRotation;
            targetRotation = -.3f * targetDistance + 62f;
           // transform.LookAt(targetRotPos); 
          //  transform.eulerAngles = new Vector3(targetRotation, transform.eulerAngles.y, transform.eulerAngles.z) + offset;
        } */

    public void LaserTarget(Vector3 tar)
    {
        Vector3 offset = new Vector3(1, 0, 0);

        //Set eye rotation 
        leftEye.LookAt(target);
        rightEye.LookAt(target);

        //Set lasers L/R

        if (hatIsGone)
        {
            leftLaser.SetPosition(0, leftEye.position);
            leftLaser.SetPosition(1, tornadoCenter.position - offset);

            rightLaser.SetPosition(0, rightEye.position);
            rightLaser.SetPosition(1, tornadoCenter.position + offset);
        }
        else
        {
            leftLaser.SetPosition(0, leftEye.position);
            leftLaser.SetPosition(1, tar - offset);

            rightLaser.SetPosition(0, rightEye.position);
            rightLaser.SetPosition(1, tar + offset);
        }
    }

    private void OnDrawGizmos()
    {

    }

    public void Stunned()
    {

    }

    public void Disabled()
    {

    }
}