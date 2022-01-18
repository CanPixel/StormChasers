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
    public TornadoScript tornadoScript; 
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
                //Launch a knockable 
                if (hit.gameObject.CompareTag("Knockable"))
                {
                    Knockable knockScript = hit.gameObject.GetComponent<Knockable>();
                 
                    if (knockScript.canbeExploded)
                    {
                        //Release an object from the tornado
                        if (knockScript.isInTornado)
                        {
                            hit.gameObject.GetComponent<PulledByTornado>().ReleaseObject();
                            //PulledByTornado script = hit.gameObject.GetComponent<PulledByTornado>();
                          //  script.ReleaseObject(); 
                            knockScript.isInTornado = false; 
                        }

                        //Add physics to object
                        knockScript.LaunchKnockAble(); 
                        rb.velocity = new Vector3(0, 0, 0); 
                       // rb.AddExplosionForce(knockAbleExplosionForce, explosionPos, explosionRadius, 22);
                    }               
                }

                //Launch a civilian car
                else if (hit.gameObject.CompareTag("CarCivilian"))
                {
                    CivilianAI civilianScript = hit.gameObject.GetComponent<CivilianAI>();

                    //Release an object from the tornado
                    if (civilianScript.isInTornado)
                    {
                        hit.gameObject.GetComponent<PulledByTornado>().ReleaseObject();
                        civilianScript.isInTornado = false;
                    }

                    //Add physics to object
                    civilianScript.LaunchCivilian();
                    rb.velocity = new Vector3(0, 0, 0);
                   // rb.AddExplosionForce(carExplosionForce, explosionPos, explosionRadius, 90);
                }

                Vector3 dir = rb.transform.position - transform.position;
                float explosionForce = 50f;
                float upForce = 700f; 
                float distanceModifier = Vector3.Distance(rb.transform.position, transform.position);

                //rb.AddTorque(Vector3.)
                rb.AddForce(dir * (explosionForce / distanceModifier), ForceMode.VelocityChange);
                rb.AddForce(Vector3.up * (upForce / distanceModifier), ForceMode.VelocityChange);


                // Debug.Log(hit.gameObject.name +" | " + distanceModifier);
                //Debug.Log(rb.velocity.magnitude + " | " + distanceModifier);
                Debug.Log(dir * (explosionForce / distanceModifier)); 



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
