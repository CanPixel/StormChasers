using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TornadoScript : MonoBehaviour
{
    [Header ("MOVEMENT")]
    public float currentMoveSpeed;
    public NodePath path;
    public int nodePoint;

    public Vector3 rotOffs;
    public Vector3 posOffs; 
    public float speed = 100f, turnSpeed = 1f;

    [Header("PHYSICS")]
    public float pullStrength;
    public float upForce;
    public float centerRotationSpeed;

    public Collider colli; 

  

    private Transform currentNodeTarget;
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
    public bool canPull = true;

    [Header("COMPONENTS")]
    public Transform centerPoint;
    public int currentInnerObjects;
    [HideInInspector] public List<Rigidbody> pulledRbList = new List<Rigidbody>(); 
    [HideInInspector] public List<Rigidbody> releasedRbList = new List<Rigidbody>();

    //States 
    private int tornadoState;
    private int fixedControllerState;
    public bool explosionInside; 
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
        SetPos();

    }

    private void FixedUpdate()
    {
        switch (tornadoState)
        {
            case (int)CurrentState.IDLE:
                CheckForObjects();
                //PullObjects();
                RotateCenter(); 
                break;
            case (int)CurrentState.ROAM:

                break; 
             

        }

        if (currentNodeTarget == null) return;

        var dir = (transform.position - currentNodeTarget.position);
        dir.y = 0;
        transform.position -= dir.normalized * speed * Time.deltaTime;
        transform.rotation = Quaternion.Lerp(transform.rotation, GetAngleTo(currentNodeTarget), Time.deltaTime * turnSpeed);
    }

    protected void SetPos()
    {
        if (path == null) return;

        transform.position = path.pathNodes[nodePoint].position + posOffs;
        var next = nodePoint + 1;
        if (next >= path.pathNodes.Count) next = 0;
        currentNodeTarget = path.pathNodes[next];
        transform.rotation = GetAngleTo(currentNodeTarget);
    }

    public Quaternion GetAngleTo(Transform targetPos)
    {
        var dir = (transform.position - targetPos.position);
        dir.y = 0;
        return Quaternion.LookRotation(dir) * Quaternion.Euler(rotOffs);
    }

    void CheckForObjects()
    {
        currentInnerObjects = pulledRbList.Count - 70;

        float nextObjDistance = Vector3.Distance(transform.gameObject.transform.position, currentNodeTarget.position); 

        if(nextObjDistance < 25) SetNextNode(path.pathNodes[nodePoint]);

        Debug.Log(nextObjDistance); 


        //Debug.Log(pulledRbList.Count); 


    }

    /*

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.CompareTag("NodePath"))
        {
            Debug.Log(col.gameObject.name);
            SetNextNode(col.transform);
        }
       
    }
    */



    protected void SetNextNode(Transform coll)
    {
        for (int i = 0; i < path.pathNodes.Count; i++) if (path.pathNodes[i] == coll)
            {
                var next = i + 1;
                if (next >= path.pathNodes.Count) next = 0;
                currentNodeTarget = path.pathNodes[next];
                nodePoint = next;
                return;
            }
    }

    /*

    public void PullObjects()
    {
       


        foreach (Rigidbody pulledRb in pulledRbList)
        {         
            float currentDistance = Vector3.Distance(pulledRb.transform.position, centerPoint.position); 
            float currentHeight = pulledRb.transform.localPosition.y;
            //Rigidbody pulledRb = pulledRb.GetComponent<Rigidbody>();
            //rb.AddTorque(new Vector3(10, 50, 5), ForceMode.Impulse);

            Vector3 dir = centerPoint.position - pulledRb.transform.position;
            if (currentDistance > minCenterDistance) pulledRb.AddForce(dir.normalized * pullStrength * Time.fixedDeltaTime, ForceMode.Acceleration); //Move object towards center 
            else pulledRb.AddForce(-dir.normalized * pullStrength / 2 * Time.fixedDeltaTime, ForceMode.Acceleration); //Keep object away from center if to close 

            if (currentHeight < minHeight) pulledRb.AddForce(Vector3.up * upForce * Time.fixedDeltaTime, ForceMode.Acceleration); //Add force if min height for object is reached  
            if (currentHeight > maxHeight) pulledRb.velocity = new Vector3(pulledRb.velocity.x, pulledRb.velocity.y / 1.06f, pulledRb.velocity.z);  //Reduce velocity if max height for object is reached
            if (currentDistance > maxCenterDistance) pulledRb.velocity = pulledRb.velocity / 1.02f;  //Reduce velocity if max distance from center has been reached 


                    
            
        }
    }
    */

    void MoveAround()
    {

    }



    void Releaseobject()
    {

    }

    void TrackStats()
    {

    }

    void RotateCenter()
    {
        centerPoint.transform.Rotate(0, centerRotationSpeed, 0);
    }

    void OnValidate()
    {
        if (Application.isPlaying) return;

        if (path != null)
        {
            nodePoint = Mathf.Clamp(nodePoint, 0, path.pathNodes.Count - 1);
            SetPos();
        }
    }



}

/*

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatAI : MonoBehaviour
{
    public NodePath path;
    public int nodePoint;

    public Vector3 rotOffs;
    public float speed = 100f, turnSpeed = 1f;

    private Transform currentNodeTarget;

    void OnValidate()
    {
        if (Application.isPlaying) return;

        if (path != null)
        {
            nodePoint = Mathf.Clamp(nodePoint, 0, path.pathNodes.Count - 1);
            SetPos();
        }
    }

    void Start()
    {
        SetPos();
    }

    void FixedUpdate()
    {
        if (currentNodeTarget == null) return;

        var dir = (transform.position - currentNodeTarget.position);
        dir.y = 0;
        transform.position -= dir.normalized * speed * Time.deltaTime;
        transform.rotation = Quaternion.Lerp(transform.rotation, GetAngleTo(currentNodeTarget), Time.deltaTime * turnSpeed);
    }

    protected void SetPos()
    {
        if (path == null) return;

        transform.position = path.pathNodes[nodePoint].position;
        var next = nodePoint + 1;
        if (next >= path.pathNodes.Count) next = 0;
        currentNodeTarget = path.pathNodes[next];
        transform.rotation = GetAngleTo(currentNodeTarget);
    }

    public Quaternion GetAngleTo(Transform targetPos)
    {
        var dir = (transform.position - targetPos.position);
        dir.y = 0;
        return Quaternion.LookRotation(dir) * Quaternion.Euler(rotOffs);
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "NodePath") SetNextNode(col.transform);
    }

    protected void SetNextNode(Transform coll)
    {
        for (int i = 0; i < path.pathNodes.Count; i++) if (path.pathNodes[i] == coll)
            {
                var next = i + 1;
                if (next >= path.pathNodes.Count) next = 0;
                currentNodeTarget = path.pathNodes[next];
                nodePoint = next;
                return;
            }
    }
}
*/