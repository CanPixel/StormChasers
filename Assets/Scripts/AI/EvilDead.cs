using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EvilDead : MonoBehaviour
{

    [Header("Looking")]
    public float LookDistance;
    public float lerpTimer;
    public float lerpDuration;
    private float targetDistance;
    public Transform leftEyePoint;
    public Transform rightEyePoint;
    private Vector3 defaultRotation; 

    [Header("Laser")]
    public float laserDuration;
    public float laserCooldownTimer;
    public float laserCooldownDuration; 
    private float laserSpeed;
    public float laserLength = 50f; 
    public bool canLaser;

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
    public MeshRenderer eyeMeshr;
    public Material searchingEyeMat;
    public Material angryEyeMat;
    public Light leftSpotlight;
    public Light rightSpotlight; 

    private int headState;

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
        headRotationTimer = Random.Range(1, 4);
        headState = (int)CurrentState.IDLE;
        leftLaser.positionCount = 2; 
        rightLaser.positionCount = 2;
       
        //  lerpTimer = lerpDuration; 
        defaultRotation = transform.eulerAngles; 
        
        leftEyePoint.transform.position = leftEye.transform.position;
        leftEyePoint.transform.eulerAngles = leftEye.transform.eulerAngles;

        rightEyePoint.transform.position = rightEye.transform.position;
        rightEyePoint.transform.eulerAngles = rightEye.transform.eulerAngles;

        //eyeMeshr = GetComponent<MeshRenderer>();
        eyeMeshr.material = searchingEyeMat;

        //leftSpotlight = gameObject.GetComponentInChildren<Light>();
       // rightSpotlight = gameObject.GetComponentInChildren<Light>();

        leftSpotlight.color = rightSpotlight.color = Color.yellow;
        

    }

    private void Update()
    {
       

        switch (headState)
        {
            case (int)CurrentState.DISABLED:          
                break;
            case (int)CurrentState.IDLE:
                CheckForTarget(); 
                break;
               
            case (int)CurrentState.LASER:
                CheckForTarget();
                Vector3 lookPos;
                lookPos = target.position - transform.position;
                var angle = Mathf.Atan2(lookPos.y, lookPos.x) * Mathf.Rad2Deg; 
                if(canLaser) LaserTarget(lookPos);


                float targetRotation = -.3f * targetDistance + 62f;
              //  lookPos.x -= targetRotation; 
                // transform.LookAt(targetRotPos); 
                //  transform.eulerAngles = new Vector3(targetRotation, transform.eulerAngles.y, transform.eulerAngles.z) + offset;


                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(offset + new Vector3(0, angle, 0)), Time.deltaTime * lerpDuration);
                
                // LookAtTarget();

                break; 
        }

     


    }


    public void CheckForTarget()
    {
        

        //Left eye raycast
        if (Physics.Raycast(leftEyePoint.position , leftEye.forward, out RaycastHit hitLeft, laserLength))
        {
            if(hitLeft.collider.gameObject.CompareTag("Ground")) leftEyePoint.transform.position = hitLeft.point; 
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

            //Tilting
            transform.localEulerAngles = new Vector3(headTilt, transform.localEulerAngles.y, transform.localEulerAngles.z);
            rotationPoint.transform.localEulerAngles += new Vector3(0, 0, headRotationSpeed);
        }
        

        //If there is a target
        if (target == null) return;

        targetDistance = Vector3.Distance(target.position, transform.position);

        //Target is player
        if (target.CompareTag("Player"))
        {
            leftLaser.enabled = true;
            rightLaser.enabled = true;
            //targetRotPos = transform.forward;

            laserCooldownTimer += Time.deltaTime;

            headState = (int)CurrentState.LASER;

            if (laserCooldownTimer >= laserCooldownDuration)
            {
                canLaser = true; 
            }
            
        }
        
        //Target is out of sight 
        if(target == null || targetDistance > LookDistance)
        {
            target = null;
            canLaser = false;
            leftLaser.enabled = false;
            rightLaser.enabled = false;
            transform.eulerAngles = defaultRotation;
            eyeMeshr.material = searchingEyeMat;
            leftSpotlight.color = rightSpotlight.color = Color.yellow;
            headState = (int)CurrentState.IDLE; 
            Debug.Log("Return To idle");
        }       
        
    }

    void OnDrawGizmosSelected()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(rightEyePoint.position, 15);
    }

    public void LookAtTarget(Vector3 targetRotPos)
    {
        

        //Lerp target position???????
        /*
        if (lerpTimer > lerpDuration && Vector3.Distance(targetRotPos, target.position) > 3)
        {
           
            //lerpTimer += Time.deltaTime;
        }
        else
        {
           
        }
        */

            
        
        //Rotate head towards player 
        float targetRotation;
        targetRotation = -.3f * targetDistance + 62f;
       // transform.LookAt(targetRotPos); 
      //  transform.eulerAngles = new Vector3(targetRotation, transform.eulerAngles.y, transform.eulerAngles.z) + offset;

        
        
    }

    public void LaserTarget(Vector3 tar)
    {
        Vector3 offset = new Vector3(1, 0, 0);

        //Set eye rotation 
        leftEye.LookAt(target);
        rightEye.LookAt(target);

        //Set lasers L/R
        
        leftLaser.SetPosition(0, leftEye.position);
        leftLaser.SetPosition(1, tar - offset);
     
        rightLaser.SetPosition(0, rightEye.position);
        rightLaser.SetPosition(1, tar + offset);        
 
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
