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
        // if (mainScript.canPull && !mainScript.pulledRbList.Contains(other.attachedRigidbody))
        if (mainScript.canPull && !other.gameObject.GetComponent<PulledByTornado>())
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
                            knockScript.isInTornado = true;
                            
                            //knockScript.rb.useGravity = false;
                            //knockScript.enabled = false;
                        }
                    }

                    //Tornado graps Civilian car
                    if (other.gameObject.CompareTag("CarCivilian"))
                    {
                        CivilianAI civilianScript = other.gameObject.GetComponent<CivilianAI>(); 

                        if(civilianScript != null)
                        {
                            civilianScript.LaunchCivilian();
                            civilianScript.isInTornado = true; 
                            //civilianScript.rb.useGravity = false;
                            //civilianScript.enabled = false; 
                        }
                    }

                    //Add trail render 
                    if (other.gameObject.GetComponentInChildren<Light>())
                    {
                        Light l = other.gameObject.GetComponentInChildren<Light>(); 
                        TrailRenderer trailObj = Instantiate(trailPrefab, l.transform.position, other.transform.rotation);
                        trailObj.gameObject.transform.SetParent(other.gameObject.transform, true);

                        trailObj.startColor = l.color;
                        trailObj.endColor = l.color;

                        l.range *= 1.5f; //Up lights a bit for that spotlight effect
       
                        //trailObj.material.color = l.color; 
                    }


                   
                    //Add pull script to object
                    other.gameObject.AddComponent<PulledByTornado>();
                    other.gameObject.GetComponent<PulledByTornado>().tornadoScript = mainScript;
                    mainScript.pulledRbList.Add(other.attachedRigidbody);
                    other.gameObject.transform.SetParent(mainScript.centerPoint, true);
                }
            }
            
        }
    }
}



