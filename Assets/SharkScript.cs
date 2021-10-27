using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SharkScript : MonoBehaviour
{

    [HideInInspector] public enum currentState { IDLE, CHASING, ATTACKING, SEARCHING, INVESTIGATE, DYING, RAGDOLLING, CHECKSTATUS, JUMPING, RETURNTOWATER }
    private int sharkState;

    [Header("Checks")]
    public bool playerInFountain;
    public bool playerJumpingOverFountain;
    public bool jumpingTowardsPlayer;
    private bool checkCurrentPos = false;
    private bool playerHasBeenEaten;
    public bool canJump;

    //Return to water
    private float ReturnToIdleTimer;
    private float IdleMoveSpeed = .65f;
    private float idleHeight;
    private Vector3 idlePosition;
    private Vector3 currentPosition;
    private Vector3 idleRotation;
    private Vector3 currentRotation;
    private float currentHeight;



    [Header("Movement")]
    public float idleRotationSpeed = 2f;
    private float extraBiteSpeed = 1.3f;
    public float chaseMoveSpeed = 10f;
    public float attackMoveSpeed = 20f;
    public float jumpSpeed = 20f;
    public float rotateSpeed = 20f;

    public float attackDistance = 4;
    public float mouthOpenDistance = 12;
    private float targetDistance;
    private float startingHeight;
    private float startingDistance;

    private Vector3 startingPosition;
    private Vector3 startingDirection;
    private Vector3 newDirection;
    private Vector3 targetPos;
    private Vector3 lastKnownPos;

    [Header("Components")]
    public Transform target;
    public Transform player;
    public GameObject pickUpPointObj;
    public PickUpPoint pickUpPoint;
    public Transform respawnPoint;
    public NavMeshAgent agent;
    private Rigidbody mainRb;
    public Animator anim;
    public GameObject mouth;
    public Transform sharkRotationPoint;

    private void Start()
    {
        sharkState = (int)currentState.IDLE;
        agent.stoppingDistance = attackDistance;
        startingHeight = transform.position.y;

        startingDistance = Vector3.Distance(transform.position, sharkRotationPoint.position);
        startingDirection = (sharkRotationPoint.position - transform.position);
        startingPosition = transform.position + startingDirection * 2;
        Debug.Log(startingPosition);
    }

    void Update()
    {

        switch (sharkState)
        {
            case (int)currentState.IDLE:
                CheckForTarget();
                IdleSwimming();

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
                CheckForTarget();
                break;

            case (int)currentState.RETURNTOWATER:
                ReturnToIdle();
                break;

        }
    }

    void CheckForFood()
    {

    }

    void CheckForTarget()
    {

        if (pickUpPoint.hasPickUp && !playerInFountain)
        {
            canJump = true;
            target = pickUpPoint.transform;
        }
        else
        {
            canJump = false;
            target = player;
        }


        if (playerInFountain)
        {
            Debug.Log("PlayerInFountain");
            playerJumpingOverFountain = false;
            jumpingTowardsPlayer = false;
            target = player;

            sharkState = (int)currentState.CHASING;
        }
        else if (playerJumpingOverFountain && canJump)
        {
            Debug.Log("SharkJumpingToPlayer");
            sharkState = (int)currentState.JUMPING;
        }
        else if (!playerJumpingOverFountain && jumpingTowardsPlayer && !playerInFountain || playerHasBeenEaten)
        {
            Debug.Log("ReturnToTank");

            playerHasBeenEaten = false;
            jumpingTowardsPlayer = false;
            //currentHeight = transform.position.y; 

            sharkState = (int)currentState.RETURNTOWATER;
        }
        else
        {
            Debug.Log("IdleState");
            sharkState = (int)currentState.IDLE;
        }
    }

    void ReturnToIdle()
    {
        if (!checkCurrentPos)
        {
            //currentHeight = transform.position.y;
            currentPosition = transform.position;
            currentRotation = transform.eulerAngles;
            anim.SetBool("Attack", false);
            checkCurrentPos = true;
        }

        //Return shark to idle pos
        if (ReturnToIdleTimer < IdleMoveSpeed)
        {
            idlePosition = Vector3.Lerp(currentPosition, new Vector3(startingPosition.x, startingHeight, startingPosition.z), ReturnToIdleTimer / IdleMoveSpeed);
            idleRotation = Vector3.Lerp(currentRotation, new Vector3(0, 37, transform.eulerAngles.z), ReturnToIdleTimer / IdleMoveSpeed);

            ReturnToIdleTimer += Time.deltaTime;
            transform.eulerAngles = idleRotation;
            transform.position = idlePosition;
        }
        else
        {
            ReturnToIdleTimer = 0f;
            checkCurrentPos = false;
            sharkState = (int)currentState.IDLE;
        }
    }


    void IdleSwimming()
    {
        Vector3 speed = new Vector3(0, idleRotationSpeed, 0);
        sharkRotationPoint.Rotate(speed * Time.deltaTime);

        anim.SetBool("Moving", true);
        anim.SetBool("MouthOpen", false);
        anim.SetBool("MouthClosed", false);
    }


    void LookAtTarget()
    {
        if (playerInFountain || playerJumpingOverFountain)
        {

            Vector3 targetDirection = (target.transform.position - transform.position).normalized;

            float singleStep = 50f * Time.deltaTime; //step distance per call
            newDirection = Vector3.RotateTowards(transform.forward, targetDirection, singleStep, 0.0f); //determin new rotation direcition
            transform.rotation = Quaternion.LookRotation(newDirection); //Set rotation 
                                                                        //  if(playerInFountain) transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, transform.eulerAngles.z);    
        }
    }


    void ChaseTarget()
    {
        targetDistance = Vector3.Distance(target.transform.position, mouth.transform.position);
        targetPos = target.transform.position;

        if (playerInFountain)
        {
            float step = (chaseMoveSpeed * extraBiteSpeed) * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(targetPos.x, targetPos.y + 4, targetPos.z), step);

            //Chase the player 
            if (targetDistance < mouthOpenDistance - 5)
            {
                if (targetDistance > attackDistance)
                    anim.SetBool("MouthOpen", true);
                else
                {

                    anim.SetBool("MouthOpen", false);
                    anim.SetBool("MouthClosed", true);
                    AttackPlayer();
                }

            }
        }
        else if (playerJumpingOverFountain)
        {
            jumpingTowardsPlayer = true;

            // calculate distance to move  
            float step = jumpSpeed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(targetPos.x - 10, targetPos.y + 6, targetPos.z), step);
            //transform.position = Vector3.MoveTowards(transform.position, new Vector3(targetPos.x , targetPos.y, targetPos.z), step);

            if (targetDistance < mouthOpenDistance)
            {
                if (targetDistance > attackDistance)
                    anim.SetBool("MouthOpen", true);
                else
                {
                    anim.SetBool("OpenMouth", false);
                    anim.SetBool("MouthClosed", true);
                    pickUpPoint.DestroyPickUp();
                    sharkState = (int)currentState.RETURNTOWATER;
                }
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
                if (pickUpPoint.hasPickUp) pickUpPoint.DestroyPickUp();
                target = player.transform;
                playerHasBeenEaten = true;
                sharkState = (int)currentState.RETURNTOWATER;
                StartCoroutine("RespawnPlayer");
            }

        }
    }

    IEnumerator RespawnPlayer()
    {
        player.gameObject.SetActive(false);
        playerInFountain = false;

        yield return new WaitForSeconds(3f);

        player.gameObject.SetActive(true);
        player.transform.position = respawnPoint.transform.position;
        playerInFountain = false;
    }
}


