using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MallAnimatronic : MonoBehaviour
{
    private bool canSeeObject = false;

    public Transform target;
    Vector3 lookDirection; 
    private float viewRange = 200f;
    public float viewAngle; 
    public float rotateSpeed = 10f; 

    void Update(){

        LookAtobject(); 
    }

    void OnTriggerStay(Collider other)
    {
        if(other.gameObject.CompareTag("Player")){   
            target = other.gameObject.transform;
            canSeeObject = true; 
          //  LookAtobject(gameObject.tag); 
        }

    }

    /*
    void OnTriggerExit(Collider other){
        if (target == null) return; 
        if(target.tag == other.tag)
        {
            target = null; 
        }
    }
    */
    
    void LookAtobject()
    {
        if (canSeeObject)
        {
            

          
            Vector3 targetDirection = (target.transform.position - transform.position);
            float singleStep = rotateSpeed * Time.deltaTime; //step distance per call
            lookDirection = Vector3.RotateTowards(transform.forward, targetDirection, singleStep, 0.0f); //determin new rotation direciton 
            transform.rotation = Quaternion.LookRotation(lookDirection); //Set rotation 
            transform.eulerAngles = new Vector3(-90, transform.eulerAngles.y, transform.eulerAngles.z); 

        }


        
    }
    

}
