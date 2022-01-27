using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRespawnScript : MonoBehaviour
{
    public Transform groundcheckObject;
    public Transform ObjectParent;
    public Transform player; 
    public GameObject shark;
    private Rigidbody sharkRb;
    private Rigidbody playerRb; 
    public Transform sharkMouthCheck;
    public Transform sharkMouthPoint; 
    private Animator sharkAnim; 
    public CarMovement carmovementScript;

    public float groundCheckDis = 5;  
    //public int hitCount; 

    public bool disablePlayer;
    public bool playerIsInWater;
    public bool grounded;
    public bool sharkCanJump; 

    public float upForce;
    public float backForce;
    public Vector3 jumpOffset;


    public float returnTimer;
    public float returnDuration;
    public float respawnDuration;
    public float respawnTimer; 

    void Start()
    {
        groundcheckObject.position = ObjectParent.position;
        groundcheckObject.parent = ObjectParent;
        sharkAnim = shark.gameObject.GetComponent<Animator>();
        sharkRb = shark.gameObject.GetComponent<Rigidbody>();
        playerRb = player.GetComponent<Rigidbody>();
        sharkCanJump = true; 
        
    }

    // Update is called once per frame
    void Update()
    {
        CheckForWater();

        if (disablePlayer) DisablePlayer(); 
       
    }

    void CheckForWater()
    {
        if (!disablePlayer)
        {
            shark.transform.position += new Vector3(0, -upForce * 1.5f, 0);
            shark.transform.eulerAngles += new Vector3(0, upForce * 5, 0);
        }

        //Check for last ground spot 
        if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, groundCheckDis) && !disablePlayer)
        {
           /* if (hit.collider.CompareTag("Water"))
            {
                SetParent();
                grounded = false;
                disablePlayer = true;
                
            }
           */
            if (hit.collider.CompareTag("Ground"))
            {
                grounded = true; 
                SetParent();
            }
            else
            {
                SetParent();
                grounded = false;
                if(hit.collider.CompareTag("Water") && hit.distance < groundCheckDis - 43f) disablePlayer = true;                     
            }

        }
    }
   

    void SetParent()
    {
        //Parent groundchecker to player
        if (grounded)
        {
            grounded = false; 
            groundcheckObject.position = ObjectParent.position;
            groundcheckObject.rotation = ObjectParent.rotation; 
            groundcheckObject.parent = ObjectParent;
        }

        //Save last ground check point by dropping groundcheckobject
        else
        {
            grounded = true; 
            groundcheckObject.parent = null;
        }
    }


    void DisablePlayer()
    {
        //Set shark position under the player
        if (sharkCanJump)
        {
            sharkCanJump = false;
            shark.transform.position = new Vector3(player.transform.position.x, player.transform.position.y - 21f, player.transform.position.z);
            sharkRb.velocity = new Vector3(0, 0, 0);
            returnTimer = returnDuration;
            respawnTimer = respawnDuration;                                          
        }

        //Check how far the shark is removed from the player
        float mouthDistance = Vector3.Distance(sharkMouthCheck.position, player.position);

        //Anim
        if (mouthDistance < 20)
        {
            sharkAnim.SetTrigger("BiteTrigger");
            sharkAnim.SetBool("IsBiting", true);
        }

        //Move shark down 
        if (returnTimer <= 0)
        {
            shark.transform.position += new Vector3(0, -upForce, 0);
        }
        else
        {
            
        }

        shark.transform.eulerAngles += new Vector3(0, upForce * 7.5f, 0);



        //Check for player distance to mouth 
        if (mouthDistance > 5f)
        {
            //Move shark upwards
            shark.transform.position += new Vector3(0, upForce * returnTimer, 0);
            shark.transform.position = new Vector3(player.transform.position.x, shark.transform.position.y, player.transform.position.z);

        }
        else if (respawnTimer > 0)
        {
            //Move shark downwards (should probally use unscaledDeltaTime but the bug is kinda fun)
            if (returnTimer > 0) returnTimer -= Time.deltaTime;
            respawnTimer -= Time.deltaTime;
            shark.transform.position += new Vector3(0, upForce * returnTimer, 0);

            //Turn of player stuff
            //carmovementScript.kart.enabled = false;
            // carmovementScript.enabled = false;
            playerRb.mass = 1f;
            player.parent = sharkMouthPoint;
            player.transform.position = sharkMouthPoint.transform.position;
            player.GetComponent<Rigidbody>().isKinematic = true;
        }
        else
        {
            RespawnPlayer();
        }
       
    }

    void RespawnPlayer()
    {
        player.parent = null;
        playerRb.mass = 500f;
        playerRb.velocity = new Vector3(0, 3, 0); 
        player.transform.position = groundcheckObject.transform.position;   
        player.GetComponent<Rigidbody>().isKinematic = false;
        disablePlayer = false;
        sharkCanJump = true;
        Debug.Log("DROPTHEPLAYER??"); 
    }

  


}
