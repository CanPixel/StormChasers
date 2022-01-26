using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PulledByTornado : MonoBehaviour
{
    public Rigidbody rb;
    public bool isPlayer; 
    public TornadoScript tornadoScript;
    private float destructionTimer = 5;
    private float playerLaunchTimer;
    float orignalRbMass;
    public Vector3 originalScale; 

    private int objState;
    [HideInInspector]
    private enum CurrentState
    {
        PULLED,
        RELEASED,
        LAUNCHED,

    }

    private void Start()
    {
        //rb = gameObject.GetComponent<Rigidbody>();
        rb.useGravity = false;
        orignalRbMass = rb.mass;
       // originalScale = rb.transform.localScale; 
        rb.mass = 5f; 
        rb.AddTorque(-Vector3.right * 1000f, ForceMode.VelocityChange);
        playerLaunchTimer = Random.Range(3f, 5f);
        if (isPlayer) destructionTimer = 3f; 

    }

    private void FixedUpdate()
    {

        switch (objState)
        {
            case (int)CurrentState.PULLED:
                PullObject();
                if(isPlayer) LaunchPlayer(); 
                break;

            case (int)CurrentState.RELEASED:
                DestroySelf(); 
                break; 

            case (int)CurrentState.LAUNCHED:
                break; 
        }
    }

    void PullObject() {

      
            float currentDistance = Vector3.Distance(rb.transform.position, tornadoScript.centerPoint.position);
        //    float middleDis = Vector3.Distance(rb.transform.position, tornadoScript.middlePoint.position); 
            float currentHeight = rb.transform.localPosition.y;
            //Rigidbody rb = rb.GetComponent<Rigidbody>();
            //rb.AddTorque(new Vector3(10, 50, 5), ForceMode.Impulse);

            Vector3 dir = tornadoScript.centerPoint.position - rb.transform.position;
            if (currentDistance > tornadoScript.minCenterDistance) rb.AddForce(dir.normalized * tornadoScript.pullStrength * Time.fixedDeltaTime, ForceMode.Acceleration); //Move object towards center 
           // else rb.AddForce(-dir.normalized * tornadoScript.pullStrength * 1 * Time.fixedDeltaTime, ForceMode.Acceleration); //Keep object away from center if to close 
            else rb.AddForce(-new Vector3(dir.x, 0, dir.z) * tornadoScript.pullStrength * 1f * Time.fixedDeltaTime, ForceMode.Acceleration);
          

            if (currentHeight < tornadoScript.minHeight) rb.AddForce(tornadoScript.gameObject.transform.up * tornadoScript.upForce * Time.fixedDeltaTime, ForceMode.Acceleration); //Add force if min height for object is reached  
            if (currentHeight > tornadoScript.maxHeight) rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y / 1.06f, rb.velocity.z);  //Reduce velocity if max height for object is reached
            if (currentDistance > tornadoScript.maxCenterDistance) rb.velocity = rb.velocity / 1.02f;  //Reduce velocity if max distance from center has been reached                   
    }

    void LaunchPlayer()
    {
        playerLaunchTimer -= Time.deltaTime;

        if(playerLaunchTimer <= 0)
        {         
            ReleaseObject();
            Vector3 launchDir = (tornadoScript.centerPoint.position - rb.transform.position).normalized;
            rb.AddForce(-launchDir * 40f, ForceMode.VelocityChange); 
           
            rb.mass = orignalRbMass; 
            tornadoScript.playerInTornado = false;
            isPlayer = false; 
        }
    }

    public void ReleaseObject()
    {
       
        //IMPORTANT ADD SOMETHING HERE THAT MAKES THE THROWED OBJECTS PICKABLE BY THE TORNADO AGAIN
        objState = (int)CurrentState.RELEASED; 
        rb.useGravity = true;
        gameObject.transform.parent = null;
        tornadoScript.pulledRbList.Remove(rb);
     
    }

    public void DestroySelf()
    {
        destructionTimer -= Time.fixedDeltaTime;
        if (destructionTimer <= 0)
        {
            rb.transform.localScale = originalScale;
            tornadoScript.pulledRbList.Remove(rb);
            Destroy(GetComponent<PulledByTornado>()); 
        }
    }

  
}


