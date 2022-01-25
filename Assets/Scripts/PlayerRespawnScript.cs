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
    public Transform sharkMouth; 
    private Animator sharkAnim; 
    public CarMovement carmovementScript;

    public float groundCheckDis = 5;  
    //public int hitCount; 

    public bool disablePlayer;
    public bool playerIsInWater;
    public bool dropChecker;
    public bool sharkCanJump; 

    public float upForce;
    public float backForce;
    public Vector3 jumpOffset; 


    public float respawnDuration;
    public float respawnTimer; 

    void Start()
    {
        groundcheckObject.position = ObjectParent.position;
        groundcheckObject.parent = ObjectParent;
        sharkAnim = shark.gameObject.GetComponent<Animator>();
        sharkRb = shark.gameObject.GetComponent<Rigidbody>(); 
    }

    // Update is called once per frame
    void Update()
    {
        CheckForWater();

        if (disablePlayer) DisablePlayer(); 
       
    }

    void CheckForWater()
    {
        
        if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, groundCheckDis) && !disablePlayer)
        {
            if (hit.collider.CompareTag("Water"))
            {
                SetParent(false);
                disablePlayer = true;
                
            }
            else if (dropChecker)
            {
                SetParent(true);
            }
        }
    }
   

    void SetParent(bool parent)
    {
        if (parent)
        {
            dropChecker = false; 
            groundcheckObject.position = ObjectParent.position;
            groundcheckObject.rotation = ObjectParent.rotation; 
            groundcheckObject.parent = ObjectParent;
        }
        else
        {
            dropChecker = true; 
            groundcheckObject.parent = null;
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        
    }

    void DisablePlayer()
    {
        if (!sharkCanJump)
        {
            //shark.transform.position = new Vector3(player.transform.position.x, player.transform.position.y - 10f, player.transform.position.z);
            sharkCanJump = true;
            sharkRb.velocity = new Vector3(0, 0, 0); 

        }
        float upSpeed = .1f;

       // if (player.transform.position.y - shark.transform.position.y > 0 && sharkCanJump)
        //{
            //
       //Vector3.MoveTowards(shark.transform.position, player.transform.position, upSpeed);
       

        if(Vector3.Distance(sharkMouth.position, player.position) > 1f)
        {
            //shark.transform.position += new Vector3(0, upSpeed, 0);        
            //shark.transform.position = new Vector3(player.transform.position.x, player.transform.position.y, player.transform.position.z);
            shark.transform.position = new Vector3(player.transform.position.x, shark.transform.position.y, player.transform.position.z);
            shark.transform.position = new Vector3(player.transform.position.x, shark.transform.position.y, player.transform.position.z);
        }
        else
        {
         
            player.parent = sharkMouth;
            player.transform.position = sharkMouth.transform.position;
            player.GetComponent<Rigidbody>().isKinematic = true;
        }

       // }
       // else
       // {
       //     Time.timeScale = 0;
       // }

    }

  


}
