using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MallAnimatronic : MonoBehaviour
{
    public bool canSeeObject;

    public Transform target; 
    public float viewRange = 20f;
    public float viewAngle; 
    public float rotateSpeed = 10f; 

    void Update(){

       // CheckForObject(); 
    }

    void OnTriggerStay(Collider other)
    {
        if(other.gameObject.CompareTag("Player")){   
            target = other.gameObject.transform;
            LookAtobject(gameObject.tag); 
        }
    }

    void OnTriggerExit(Collider other){
        if(target.tag == other.tag)
        {
            target = null; 
        }
    }

    
    void LookAtobject(string ObjType)
    {
        if(ObjType == ("Player"))
        {
            Debug.Log(" Rotating??"); 
            Vector3 targetDirection = target.position - transform.position;
            float singleStep = rotateSpeed * Time.deltaTime;
            Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, singleStep, 0.0f);
            transform.rotation = Quaternion.LookRotation(newDirection);

        }
    }
    

/*
   void CheckForObject()
    {
        RaycastHit hit;
        //Debug.Log(" Looking for player");

        if(Physics.SphereCast(transform.position, viewRange, Vector3.zero, out hit, viewRange))
        {
            if(hit.transform)
            {
                Debug.Log("PlayerInView"); 
                objToFollow = hit.transform; 
                LookAtobject("Player"); 
            }
        }

        //else objToFollow = null; 
        
    }
    */

}
