using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudManager : MonoBehaviour
{
   // [HideInInspector] public List<GameObject> lightingEffect = new List<GameObject>();

   // public float cooldownDuration = Random.Range(3f, 6f);
    public float cooldownTimer;
    private bool canLighting = true;
    public ParticleSystem lightning; 

 



    private void Start()
    {
        cooldownTimer = Random.Range(2f, 8f);
        lightning = gameObject.GetComponentInChildren<ParticleSystem>(); 
        
    }

    private void Update()
    {
        if (canLighting)
        {
            cooldownTimer -= Time.deltaTime; 
            if(cooldownTimer <= 0)
            {
                lightning.Play(); 
                //canLighting = true;
                cooldownTimer = Random.Range(2f, 9f); 
            }
        }

    }
}
