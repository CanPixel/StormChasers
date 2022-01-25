using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using com.zibra.liquid.SDFObjects;
using com.zibra.liquid.DataStructures;
using com.zibra.liquid.Utilities;
using com.zibra.liquid.Manipulators;

public class WaterGeyser : MonoBehaviour {
    private float timer = 0, reorientTimer = 0;

    public com.zibra.liquid.Solver.ZibraLiquid zibra;
    public GameObject triggerObject;
    public PhotoItem photoObject;
    private int baseSensation;
    private string baseName;
    public bool gushing = false;
    public bool freeFlow; 
    
    void Start() {
        zibra.gameObject.SetActive(false);
        zibra.sdfColliders.Add(GameObject.FindGameObjectWithTag("Player").GetComponent<com.zibra.liquid.SDFObjects.AnalyticSDFCollider>());

        baseName = photoObject.tag;
        baseSensation = photoObject.sensation;
    }

    void Update() {
        if(zibra.activeParticleNumber >= zibra.MaxNumParticles) timer += Time.deltaTime;
        if(timer > 1) {
            zibra.gameObject.SetActive(false);
            gushing = false;
            timer = 0;
            reorientTimer = 0.1f;
        } 

        photoObject.tag = (gushing) ? ("Exploded Hydant!") : "Hydrant";
        photoObject.sensation = (gushing) ? (baseSensation + SensationScores.scores.explodedHydrantValue) : baseSensation;

        if(reorientTimer > 0) {
            reorientTimer += Time.deltaTime;
            triggerObject.transform.rotation = Quaternion.Lerp(triggerObject.transform.rotation, Quaternion.identity, Time.deltaTime * 7f);
            if(reorientTimer > 1.5f && !freeFlow) reorientTimer = 0;
        }

        if (freeFlow) TriggerGeyser();  
    }

    public void TriggerGeyser() {
        zibra.gameObject.SetActive(true);
        timer = 0;
        reorientTimer = 0;    
        gushing = true;
    }

    void OnTriggerStay(Collider col) {
        if((col.gameObject.tag == "CarCivilian"/*  || col.gameObject.tag == "Player"*/) && gushing) {
            col.attachedRigidbody.AddForce(Vector3.up * 6400f);
            col.GetComponent<CivilianAI>().enabled = false;
            col.GetComponent<PhotoItem>().tag = ("launchedcar");
        }
    }

   
}
