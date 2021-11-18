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
    
    void Start() {
        zibra.gameObject.SetActive(false);
        zibra.sdfColliders.Add(GameObject.FindGameObjectWithTag("Player").GetComponent<com.zibra.liquid.SDFObjects.AnalyticSDFCollider>());
        //zibra.sdfColliders.Add(GameObject.FindGameObjectWithTag("Shark").GetComponent<com.zibra.liquid.SDFObjects.AnalyticSDFCollider>());

        zibra.containerPos = transform.position;
    }

    void Update() {
        if(zibra.activeParticleNumber >= zibra.MaxNumParticles) timer += Time.deltaTime;
        if(timer > 1) {
            photoObject.tags = "";
            zibra.gameObject.SetActive(false);
            timer = 0;
            reorientTimer = 0.1f;
        }

        if(reorientTimer > 0) {
            reorientTimer += Time.deltaTime;
            triggerObject.transform.rotation = Quaternion.Lerp(triggerObject.transform.rotation, Quaternion.identity, Time.deltaTime * 7f);
            if(reorientTimer > 1.5f) reorientTimer = 0;
        }
    }

    public void TriggerGeyser() {
        zibra.gameObject.SetActive(true);
        timer = 0;
        reorientTimer = 0;    
        photoObject.tags = "geyser";
    }
}
