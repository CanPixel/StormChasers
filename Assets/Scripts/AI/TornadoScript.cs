using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TornadoScript : MonoBehaviour
{
    [Header ("MOVEMENT")]
    public float currentMoveSpeed;
    public int nodePoint;
    public NodePath path;
    private Transform currentNodeTarget;

    //public Vector3 rotOffs;
    public Vector3 posOffs; 
    public float speed = 100f;//, turnSpeed = 1f;

    [Header("PHYSICS")]
    public float pullStrength;
    public float upForce;
    public float centerRotationSpeed;

    [Header("THROWING")]
    public float throwForce;
    //public float throwCooldown;
    private Transform throwTarget; 
    public Rigidbody pickedThrowTarget;

    public bool canThrow;
    public bool canEatBuilding; 
    public bool isThrowing;
    public bool targetedThrow;
    public bool targetIsPlayer;
    public float maxThrowAmount;
    private float currentObjectsThrown;
    private float waitForThrowTimer;
    [HideInInspector] public bool playerInTornado = false; 
    

    [Header("CONSTRAINTS")]
    public float minHeight = -2f;
    public float maxHeight = 2f;
    public float minCenterDistance = 5f; //pulled object can't come closer then this 
    public float maxCenterDistance = 10f; //pulled object can't go further aw
    public int maxInnerObjects;
    public int currentInnerObjects;
    public bool canPull = true;

    [Header("COMPONENTS")]
    public Transform playerTarget; 
    public Transform centerPoint;
   
    [HideInInspector] public List<Rigidbody> pulledRbList = new List<Rigidbody>(); 
    [HideInInspector] public List<Rigidbody> releasedRbList = new List<Rigidbody>();

    //States 
    private int tornadoState;
    public bool explosionInside; 
    [HideInInspector]
    private enum CurrentState
    {
        IDLE,
        ROAM,
        PULL,
        LAUNCH, 
    }

    private void Start() {

        tornadoState = (int)CurrentState.ROAM;
        SetPos();
        pulledRbList.Clear();

        waitForThrowTimer = Random.Range(5f, 10f);
        maxThrowAmount = Random.Range(3f, 7f);
    }

    private void FixedUpdate() {
        switch (tornadoState) {
            case (int)CurrentState.IDLE:
                break;
            case (int)CurrentState.ROAM:
                MoveTornado();
                RotateCenter();
                CheckPath();
                CheckForThrow();
                break; 
        }
    }

    protected void SetPos() {
        if (path == null) return;

        transform.position = path.pathNodes[nodePoint].position + posOffs;
        var next = nodePoint + 1;
        if (next >= path.pathNodes.Count) next = 0;
        currentNodeTarget = path.pathNodes[next];
    }


    void CheckPath() {
        currentInnerObjects = pulledRbList.Count;
        if(currentNodeTarget == null) return;
        
        float nextObjDistance = Vector3.Distance(transform.gameObject.transform.position, currentNodeTarget.position + posOffs); 
        if(nextObjDistance < 25) SetNextNode(path.pathNodes[nodePoint]);
    }

    protected void SetNextNode(Transform coll) {
        for (int i = 0; i < path.pathNodes.Count; i++) if (path.pathNodes[i] == coll)
            {
                var next = i + 1;
                if (next >= path.pathNodes.Count) next = 0;
                currentNodeTarget = path.pathNodes[next];
                nodePoint = next;
                return;
            }
    }

    void MoveTornado()
    {
        if (currentNodeTarget == null) return;

        var dir = (transform.position - currentNodeTarget.position);
        dir.y = 0;
        transform.position -= dir.normalized * speed * Time.deltaTime;
    }

    void CheckForThrow()
    {
        //No objects in tornado
        if (pulledRbList.Count <= 0)
        {
            canThrow = false; 
            return;
        }

        //Cooldown for throwing 
        if(waitForThrowTimer > 0)
        {
            waitForThrowTimer -= Time.deltaTime; 
        }
        else
        {
            waitForThrowTimer = Random.Range(5f, 10f);
            maxThrowAmount = Random.Range(3f, 7f); 
            canThrow = true; 
        }

        //Throw logic 
        if (canThrow)
        {
            canThrow = false; 
            pickedThrowTarget = pulledRbList[Random.Range(0, pulledRbList.Count)];
            
            SetThrowTarget();
            ThrowObject(throwTarget);        
        }
    }

    void SetThrowTarget()
    {
        if (targetedThrow)
        {
            if (playerTarget)
                throwTarget = playerTarget;           
        }
        else throwTarget = null; 
    }


    void ThrowObject(Transform target)
    {
        pickedThrowTarget.GetComponent<PulledByTornado>().ReleaseObject();


        //Set throwing direction 
        Vector3 throwDir;
        if (target != null)
        {
            throwDir = target.position - pickedThrowTarget.position;
            pickedThrowTarget.velocity = new Vector3(0, 0, 0);
        }
        else
        {
            throwDir = centerPoint.transform.forward;            
        }

        //Add force to picked ojject
        pickedThrowTarget.AddForce(Vector3.up * throwForce / 2, ForceMode.VelocityChange);
        pickedThrowTarget.AddForce(throwDir * throwForce, ForceMode.VelocityChange); 

        //Check if max amount of objects has been thrown 
        currentObjectsThrown++;
        if (currentObjectsThrown < maxThrowAmount) canThrow = true;
        else currentObjectsThrown = 0; 
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