using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KartGame.KartSystems;

public class CarAnimation : MonoBehaviour {
    public ParticleSystem[] fumes;
    public float fumeA = 0.5f, fumeB = 12, fumeC = 2;
    public ParticleSystem[] flames;
    public CarMovement movement;
    public ArcadeKart kart;
    public Rigidbody rb;

    private Vector3 baseScale;

    public GameObject chassis;
    public Vector3 wiggleAxis = new Vector3(0.2f, 0, 1f);
    public float wiggleSpeed = 30, wiggleRange = 2;
    public float wiggleSpeedCap = 20, wiggleRangeCap = 10;
    public float GasBounceReduction = 35f;
    public float dampening = 10f;

    private float gasBounce = 0;
    private float lastGas = 0;

    void Start() {
        baseScale = chassis.transform.localScale;
    }

    void Update() {
        for(int i = 0; i < flames.Length; i++) {
            if(movement.boostScript.isBoosting) {
                flames[i].Play();
                fumes[i].Stop();
            }
            else {
                flames[i].Stop();
                fumes[i].Play();
            }
        }
    }

    void LateUpdate() {
        var speed = rb.velocity.magnitude / kart.baseStats.TopSpeed;

        for(int i = 0; i < fumes.Length; i++) {
            var em = fumes[i].emission;
            em.rateOverTimeMultiplier = ((movement.IsGassing() + fumeA) * fumeB) - fumeC;
        }

        float wiggle = Mathf.Sin(Time.time * Mathf.Clamp(speed * wiggleSpeed, 0, wiggleSpeedCap)) * Mathf.Clamp(speed * wiggleRange, 0, wiggleRangeCap);//Mathf.Sin(Time.time * Mathf.Clamp(speed * wiggleSpeed, 0, wiggleSpeedCap)) * Mathf.Clamp(speed * wiggleRange, 0, wiggleRangeCap);
        var b = wiggleAxis * wiggle;

        chassis.transform.localRotation = Quaternion.Lerp(chassis.transform.localRotation, Quaternion.Euler(b), Time.deltaTime * dampening);

        if(gasBounce > 0) gasBounce -= Time.deltaTime * 3f;

        if(lastGas != movement.IsGassing()) {
            gasBounce = 1;
            lastGas = movement.IsGassing();
        }

        chassis.transform.localScale = Vector3.Lerp(chassis.transform.localScale, baseScale + new Vector3(0, gasBounce / GasBounceReduction, 0), Time.deltaTime * 24f);
    }
}
