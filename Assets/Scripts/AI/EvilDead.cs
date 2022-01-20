using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EvilDead : MonoBehaviour
{

    [Header("Looking")]
    public float checkDistance;
    public float laserDuration;
    public float laserSpeed;   
    public float LookDistance;
    public float lerpTimer;
    public float lerpDuration; 

    [Header("Rotation")]
    public float headTilt;
    public float headRotationSpeed = 5;
    private float headRotationTimer;
    public Vector3 offset = new Vector3(0, 20, 0);
    Vector3 targetRotPos; 

    [Header ("Components")]
    public LineRenderer leftLaser;
    public LineRenderer rightLaser;
    public Transform target;
    public Transform rotationPoint;
    public Transform idleLookAtPoint;
    public Transform leftEye;
    public Transform rightEye;
    public Light eyeLights;


    public bool canLaser;

    private int headState;

    [HideInInspector]
    private enum CurrentState
    {
        DISABLED,
        SEARCH,
        LASER,
        DESTROYED,
    }





    private void Start()
    {
        headRotationTimer = Random.Range(1, 4);
        headState = (int)CurrentState.SEARCH;
        leftLaser.positionCount = 2; 
        rightLaser.positionCount = 2;
      //  lerpTimer = lerpDuration; 
    }

    private void Update()
    {
    

        switch (headState)
        {
            case (int)CurrentState.DISABLED:
              
                break;
            case (int)CurrentState.SEARCH:
                CheckForTarget(); 
                
                break;
        }
    }


    public void CheckForTarget()
    {
        //Rotate head randomly
        headRotationTimer -= Time.deltaTime; 
        if(headRotationTimer <= 0)
        {
            headRotationSpeed *= -1;
            headRotationTimer = Random.Range(4f, 10f);  
        }

        if(target == null)
        {
            transform.localEulerAngles = new Vector3(headTilt, transform.localEulerAngles.y, transform.localEulerAngles.z); 
            rotationPoint.transform.localEulerAngles += new Vector3(0, 0, headRotationSpeed); 

        }
        else if (target.CompareTag("Player"))
        {
            leftEye.LookAt(target);
            rightEye.LookAt(target);
            LaserTarget();
            LookAtTarget(); 
        }

       
     
    }

    public void LookAtTarget()
    {
        //   rotationPoint.transform.localEulerAngles = target.transform.position; 

        //   transform.LookAt(target.position + offset);

     
        float targetDistance = Vector3.Distance(target.position, transform.position);
        float targetRotation;

        //  lerpTimer = lerpDuration;
        if (lerpTimer < lerpDuration)
        {
     
            targetRotPos = Vector3.Lerp(targetRotPos, target.position, lerpTimer / lerpDuration);
            lerpTimer += Time.deltaTime;
        }

        Debug.Log(targetRotPos + " | " + target.position); 


        if (targetDistance < 200)
        {
            //transform.eulerAngles = new Vector3(, 0,0)
            //x = currentdistance - 3
            targetRotation = -.3f * targetDistance + 62f;
            transform.LookAt(targetRotPos); 
            transform.eulerAngles = new Vector3(targetRotation, transform.eulerAngles.y, transform.eulerAngles.z) + offset; 
        }
  
        //42

        //65 min distance 

        //200 max distance
    }

    public void LaserTarget()
    {
        Vector3 offset = new Vector3(1, 0, 0);
        Vector3 targetRighPos;
        Vector3 targetLeftPos;
        //leftLaser.SetPosition(0, leftEye.position);
        //leftLaser.SetPosition(0, target.position);
        //leftLaser.SetPosition(1, leftEye.forward * 20);

        
        //targetLeftPos = target.position - offset; 
       

        leftLaser.SetPosition(0, leftEye.position);

        leftLaser.SetPosition(1, target.position - offset);

       
        
        rightLaser.SetPosition(0, rightEye.position);
        rightLaser.SetPosition(1, target.position + offset);



        Ray ray;

        if (Physics.Raycast(transform.position, transform.forward, 20))
        {

        }

        Debug.DrawRay(rightEye.position, transform.forward, Color.green); 

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
