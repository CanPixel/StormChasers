using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KartGame.KartSystems;

public class CarAnimation : MonoBehaviour {
    public ParticleSystem[] fumes;
    public CarMovement movement;
    public ArcadeKart kart;
    public Rigidbody rb;

    private Vector3 baseScale;

    public GameObject chassis;
    public Vector3 wiggleAxis = new Vector3(0, 0, 1);

    public float wiggleSpeed = 1, wiggleRange = 1;
    public float wiggleSpeedCap = 10, wiggleRangeCap = 10;
    public float GasBounceReduction = 6f;

    public float dampening = 20f;

    private float gasBounce = 0;
    private float lastGas = 0;

    void Start() {
        baseScale = chassis.transform.localScale;
    }

    void LateUpdate() {
        var speed = rb.velocity.magnitude / kart.baseStats.TopSpeed;

        for(int i = 0; i < fumes.Length; i++) {
            if(movement.IsGassing() > 0.01f) fumes[i].Play();
            else fumes[i].Stop();
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
