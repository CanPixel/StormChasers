using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TornadoScript : MonoBehaviour
{
    [Header ("MOVEMENT")]
    public float currentMoveSpeed;

    [Header("PHYSICS")]
    public float pullStrength;
    public float upForce;
    public float centerRotationSpeed;
    //public float downForce; 

    // public float throwStength;
    //  public float pullRadius;

    // public float currentSize;
    // public float maxSize;
    // public float minSize; 

    [Header("CONSTRAINTS")]
    public float minHeight = -2f;
    public float maxHeight = 2f;
    public float minCenterDistance = 5f; //pulled object can't come closer then this 
    public float maxCenterDistance = 10f; //pulled object can't go further aw
    public int maxInnerObjects;
    [HideInInspector] public bool canPull;

    [Header("COMPONENTS")]
    public Transform centerPoint;
    [HideInInspector] public int currentInnerObjects;
    [HideInInspector] public List<Rigidbody> pulledRbList = new List<Rigidbody>();

    //States 
    private int tornadoState;
    private int fixedControllerState;
    [HideInInspector]
    private enum CurrentState
    {
        IDLE,
        ROAM,
        PULL,
        LAUNCH, 

    }

    private void Start()
    {
        tornadoState = (int)CurrentState.IDLE; 
    }

    private void FixedUpdate()
    {
        switch (tornadoState)
        {
            case (int)CurrentState.IDLE:
                CheckForObjects();
                PullObjects();
                RotateCenter(); 
                break;
            case (int)CurrentState.ROAM:

                break; 
             

        }
    }



    void CheckForObjects()
    {
           currentInnerObjects = pulledRbList.Count; 
    
    }

    void PullObjects()
    {  
        foreach (Rigidbody pulledRb in pulledRbList)
        {         
            float currentDistance = Vector3.Distance(pulledRb.transform.position, centerPoint.position); 
            float currentHeight = pulledRb.transform.localPosition.y; 
            //Rigidbody pulledRb = pulledRb.GetComponent<Rigidbody>();

            Vector3 dir = centerPoint.position - pulledRb.transform.position;
            if (currentDistance > minCenterDistance) pulledRb.AddForce(dir.normalized * pullStrength * Time.fixedDeltaTime, ForceMode.Acceleration); //Move object towards center 
            else pulledRb.AddForce(-dir.normalized * pullStrength / 2 * Time.fixedDeltaTime, ForceMode.Acceleration); //Keep object away from center if to close 

            if (currentHeight < minHeight) pulledRb.AddForce(Vector3.up * upForce * Time.fixedDeltaTime, ForceMode.Acceleration); //Add force if min height for object is reached  
            if (currentHeight > maxHeight) pulledRb.velocity = new Vector3(pulledRb.velocity.x, pulledRb.velocity.y / 1.06f, pulledRb.velocity.z);  //Reduce velocity if max height for object is reached
            if (currentDistance > maxCenterDistance) pulledRb.velocity = pulledRb.velocity / 1.02f;  //Reduce velocity if max distance from center has been reached 
            
        }
    }

    void MoveAround()
    {

    }

    void LaunchObject()
    {

    }

    void TrackStats()
    {

    }

    void RotateCenter()
    {
        centerPoint.transform.Rotate(0, centerRotationSpeed, 0);
    }
}
