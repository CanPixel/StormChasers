using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SharkScript : MonoBehaviour
{

    [HideInInspector] public enum currentState { IDLE, CHASING, ATTACKING, SEARCHING, INVESTIGATE, DYING, RAGDOLLING, CHECKSTATUS, JUMPING, RETURNTOWATER }
    private int sharkState;

    public bool playerInFountain;
    public bool playerJumpingOverFountain; 
    private bool targetInSight;
    public float currentFovAngle;
    public float currentLookDistance = 20f;



    Vector3 newDirection;
    Vector3 targetPos;
    Vector3 lastKnownPos;

    public float idleMoveSpeed = 2f;
    public float chaseMoveSpeed = 10f;
    public float attackMoveSpeed = 20f;
    public float jumpSpeed = 20f; 

    public float rotateSpeed = 20f;
   // public float waterRotateSpeed = 
    public float attackDistance = 4;
    private float targetDistance;

    [Header("Components")]
    public Transform target;
    public Transform respawnPoint; 
    public NavMeshAgent agent;
    private Rigidbody mainRb;
    public Animator anim;
    public GameObject mouth;

    private void Start()
    {
        sharkState = (int)currentState.IDLE;
        agent.stoppingDistance = attackDistance;

    }

    void Update()
    {
        //Debug.Log(targetDistance);
        

        switch (sharkState)
        {
            case (int)currentState.IDLE:
                CheckForTarget();
                IdleSwimming();
                //LookAtTarget(); 
                break;

            case (int)currentState.CHASING:
                CheckForTarget();
                ChaseTarget();
                LookAtTarget();
                break;

            case (int)currentState.ATTACKING:
                CheckForTarget();
                LookAtTarget();
                AttackPlayer();
                break;

            case (int)currentState.JUMPING:
                LookAtTarget();
                ChaseTarget(); 
                //JumpTowardsPlayer(); 
                break;

            case (int)currentState.RETURNTOWATER:
                ReturnToWater(); 
                //LookAtTarget();
                //JumpTowardsPlayer();
                break;

        }
    }

   

    void CheckForTarget()
    {


        if (playerInFountain)
        {
            playerJumpingOverFountain = false;
         
            sharkState = (int)currentState.CHASING;
        }
        else if (playerJumpingOverFountain) sharkState = (int)currentState.JUMPING;
        else sharkState = (int)currentState.IDLE;
    }
    void ReturnToWater()
    {

    }

    void IdleSwimming()
    {
        agent.speed = 0f;
    }



    void LookAtTarget()
    {
        if (playerInFountain || playerJumpingOverFountain)
        {

            Vector3 targetDirection = (target.transform.position - transform.position).normalized;
            
            float singleStep = 50f * Time.deltaTime; //step distance per call
            newDirection = Vector3.RotateTowards(transform.forward, targetDirection, singleStep, 0.0f); //determin new rotation direcition
            transform.rotation = Quaternion.LookRotation(newDirection); //Set rotation 
            if(playerInFountain) transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, transform.eulerAngles.z);    
        }
    }


    void ChaseTarget()
    {
        targetDistance = Vector3.Distance(target.transform.position, mouth.transform.position);

        if (playerInFountain)
        {
            //Set player pos to target pos and move towards target pos 
            targetPos = target.transform.position;
            agent.enabled = true; 

            //Shark is chasing the player
            if (targetDistance > attackDistance)
            {
                agent.speed = chaseMoveSpeed;
                anim.SetBool("Attack", false);
                anim.SetBool("Moving", true);
                agent.SetDestination(targetPos);
            }
            //Shark is close enough to attac
            else
            {
                agent.speed = attackMoveSpeed;
                anim.SetBool("Attack", true);
                AttackPlayer();
            }
        }
        else if (playerJumpingOverFountain)
        {
            agent.enabled = false;
            float step = jumpSpeed * Time.deltaTime; // calculate distance to move  
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(target.position.x, target.position.y + 6, target.position.z + 5), step);

            Debug.Log("JumpTowardsPlayer");

            if (targetDistance < .5)
            {
                Debug.Log("Reached Player"); 
                anim.SetBool("Attack", true);
                anim.SetBool("Attack", false); 
                sharkState = (int)currentState.RETURNTOWATER;
            }
        }
    }


    void AttackPlayer()
    {
        //Check if the player is within attack range 

        Collider[] playerInMouth = Physics.OverlapSphere(mouth.transform.position, 1.5f);

        foreach (Collider player in playerInMouth)
        {
            if (player.gameObject.name == "PLAYER")
            {
                Debug.Log("Player eaten");
                target = player.transform;
                StartCoroutine("RespawnPlayer"); 
                //player.gameObject.SetActive(false);              
            }
        }
    }

    IEnumerator RespawnPlayer()
    {
        target.gameObject.SetActive(false);

        yield return new WaitForSeconds(3f);

        target.gameObject.SetActive(true);
        target.transform.position = respawnPoint.transform.position;
        playerInFountain = false;
    }


    /*
    if (targetDistance < attackDistance)
    {
        anim.SetBool("Attack", false);
        sharkState = (int)currentState.CHASING;

    }
    /


}



/*
else
{
    //Track the player a bit longer when line of sight is broken        
    if (lineOfSightTimer < maxTimeOutOfSight)
    {
        lineOfSightTimer += Time.deltaTime;
        targetPos = player.transform.position;
        lastKnownPos = targetPos;
        //if (on) Debug.Log("Line of sight broken");
    }

    //Move towards the last known location of the target, if reached start searching 
    if (agent.destination != lastKnownPos)
    {
        //transform.position = Vector3.MoveTowards(transform.position, lastKnownPos, chaseSpeed * Time.deltaTime);
        agent.SetDestination(lastKnownPos);
        //if (on) Debug.Log("Move to lk pos");
    }
    else
    {
        enemyState = (int)currentState.SEARCHING;
        agent.speed = 0f;
        //if(on)Debug.Log("Searching"); 
    }




}
*/

}


