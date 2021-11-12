using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using com.zibra.liquid.SDFObjects;
using com.zibra.liquid.DataStructures;
using com.zibra.liquid.Utilities;
using com.zibra.liquid.Manipulators;

public class WaterGeyser : MonoBehaviour {
    public GameObject geyserFluid;

    private float timer = 0;

    public com.zibra.liquid.Solver.ZibraLiquid zibra;
    
    void Start() {
        geyserFluid.SetActive(false);
        zibra.sdfColliders.Add(GameObject.FindGameObjectWithTag("Player").GetComponent<com.zibra.liquid.SDFObjects.AnalyticSDFCollider>());
        zibra.sdfColliders.Add(GameObject.FindGameObjectWithTag("Shark").GetComponent<com.zibra.liquid.SDFObjects.AnalyticSDFCollider>());
    }

    void Update() {
        timer += Time.deltaTime;
        //if(timer > 5) geyserFluid.SetActive(false);
    }

    public void TriggerGeyser() {
        geyserFluid.SetActive(true);
        timer = 0;
    }
}
