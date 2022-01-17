using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckForPullObjects : MonoBehaviour
{
    public TornadoScript mainScript;
    public TrailRenderer trailPrefab;


    private void Start()
    {
       // mainScript = GetComponentInParent<TornadoScript>(); 
    }

    //check for and add pullable objects to the tornado's pull list
    private void OnTriggerEnter(Collider other)
    {
        if (mainScript.canPull && !mainScript.pulledRbList.Contains(other.attachedRigidbody))
        {
            if (other.gameObject.CompareTag("Knockable") || other.gameObject.CompareTag("CarCivilian"))
            {
                if (mainScript.currentInnerObjects < mainScript.maxInnerObjects)
                {
                    //Tornado graps knockable object
                    if (other.gameObject.CompareTag("Knockable"))
                    {
                        Knockable knockScript = other.gameObject.GetComponent<Knockable>();

                        if (knockScript != null)
                        {
                            knockScript.LaunchKnockAble();
                            knockScript.rb.useGravity = false;
                            knockScript.enabled = false;
                        }
                    }

                    //Tornado graps Civilian car
                    if (other.gameObject.CompareTag("CarCivilian"))
                    {
                        CivilianAI civilianScript = other.gameObject.GetComponent<CivilianAI>(); 

                        if(civilianScript != null)
                        {
                            Debug.Log("Got a car"); 
                            civilianScript.LaunchCivilian();
                            civilianScript.rb.useGravity = false;
                            civilianScript.enabled = false; 
                        }
                    }                  
                    TrailRenderer trailObj = Instantiate(trailPrefab, other.transform.position, other.transform.rotation);
                    trailObj.gameObject.transform.SetParent(other.gameObject.transform, true);
                    other.gameObject.transform.SetParent(mainScript.centerPoint, true);
                    mainScript.pulledRbList.Add(other.attachedRigidbody);
               
                    //if (other.gameObject.GetComponent<TrailRenderer>() != null) other.gameObject.GetComponent<TrailRenderer>().enabled = true;
                }
            }
            
        }
    }
}



