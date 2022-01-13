using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerExplosion : MonoBehaviour
{
    private float explosionDelayTímer; 
    public float explosionDelayDuration;

    private float waitForResetTimer;
    public float waitForResetDuration;

    public float explosionRadius = 20f;
    
    //private float disableDelayTimer = 5f; 

    public GameObject explosionEffect;
    public GameObject player; 
    private GameObject explosion; 
    //public Collider col;

    public bool canExplode;
    public bool canRetrigger; 
    public bool isVehicleTrigger;
    public bool isActivationTrigger;
    public bool hasBeenActivated;  

    public int explosiveState;
    private int fixedControllerState;
    [HideInInspector]
    public enum CurrentState
    {
        IDLE,
        RESET,
        WAIT, 
        EXPLODE,
        DESTROY, 
        
    }

    public static CurrentState curState; 

    private void Start()
    {
       // if (isVehicleTrigger) explosionDelayDuration = 0f;
        explosiveState = (int)CurrentState.IDLE;
        explosionDelayTímer = explosionDelayDuration;
        waitForResetTimer = waitForResetDuration;

        if (isActivationTrigger) gameObject.tag = "ActivationExplosive"; 


    }

    private void Update()
    {
        switch (explosiveState)
        {
            case (int)CurrentState.IDLE:
                CheckForTrigger(); 
                break;
            case (int)CurrentState.WAIT:
                WaitForExplosion(); 
                break;
            case (int)CurrentState.EXPLODE:
                Explode(); 
                break;
            case (int)CurrentState.RESET:
                ResetExplosive(); 
                break;
        }
    }

    private void CheckForTrigger()
    {
        //if (isPhotoTrigger) ;
        if (isActivationTrigger && hasBeenActivated && canExplode) explosiveState = (int)CurrentState.WAIT;
    }


    void OnTriggerEnter(Collider other)
    {
        if (isVehicleTrigger && other.gameObject.CompareTag("Player") && canExplode)
        {
            // Player boosts through explosive object 
            if (player.GetComponent<Boost>().isBoosting)
            {
                explosiveState = (int)CurrentState.WAIT; 
                
            }
        }
    }

    void WaitForExplosion()
    {
        explosionDelayTímer -= Time.deltaTime;
        canExplode = false; 

        if (explosionDelayTímer <= 0)
        {
            explosionDelayTímer = explosionDelayDuration; 
            explosiveState = (int)CurrentState.EXPLODE;
        }
    }
          

    public void Explode()
    {            
        explosion = Instantiate(explosionEffect, transform.position, transform.rotation);
        

        Vector3 explosionPos = transform.position;
       
        Collider[] colliders = Physics.OverlapSphere(explosionPos, explosionRadius);

        foreach (Collider hit in colliders)
        {
            Rigidbody rb = hit.GetComponent<Rigidbody>();
            float knockAbleExplosionForce = 30000f;
            float carExplosionForce = 70000f; 

            if (rb != null)
            {
                if (hit.gameObject.CompareTag("Knockable"))
                {
                    Knockable knockScript = hit.gameObject.GetComponent<Knockable>();
                    if (knockScript.canbeExploded)
                    {
                        knockScript.LaunchKnockAble();
                        rb.velocity = new Vector3(0, 0, 0);
                        rb.AddExplosionForce(knockAbleExplosionForce, explosionPos, explosionRadius, 22);
                    }               
                }
                else if (hit.gameObject.CompareTag("CarCivilian"))
                {
                    hit.gameObject.GetComponent<CivilianAI>().LaunchCivilian();
                    rb.velocity = new Vector3(0, 0, 0);
                    rb.AddExplosionForce(carExplosionForce, explosionPos, explosionRadius, 90);
                }

                
            }
        }

        if (canRetrigger)
        {
    
            waitForResetTimer = waitForResetDuration;
            explosiveState = (int)CurrentState.RESET;
        }
        else Destroy(this.gameObject); 
    }


    private void ResetExplosive()
    {
        waitForResetTimer -= Time.deltaTime;

        if (waitForResetTimer <= 0)
        {
            canExplode = true;
            hasBeenActivated = false;
            explosiveState = (int)CurrentState.IDLE;
        }        
    }

    void OnDrawGizmosSelected()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.yellow * .4f;
        Gizmos.DrawSphere(transform.position, explosionRadius);
    }
}
