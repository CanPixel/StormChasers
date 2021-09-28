using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedLines : MonoBehaviour
{
    public ParticleSystem speedPS;
    public float minSpeed = 5.0f;
    public bool velocityAmount = true;
    public bool velocityPlay = true;
    private int lineAmount = 0;
    private int currVel = 0;
    private int minVel;   
    private Vector3 prevPos;


    void Start()
    {
        StartCoroutine(CalcVelocity());
    }

    void FixedUpdate()
    {
        ParticleSystem.EmissionModule psEmission = speedPS.emission;

        if (currVel >= 0 && velocityAmount == true)
        {
            lineAmount = (minVel * minVel) * 50;
            psEmission.rateOverTime = lineAmount;
        }
        if (currVel >= 0 && velocityPlay == true)
        {
            lineAmount = 100;
            PlayParticle();
        }

    }

    void PlayParticle()
    {
        speedPS.Play();
//        Debug.Log(currVel);
    }

    IEnumerator CalcVelocity()
    {
        while (Application.isPlaying)
        {
            // Position at frame start
            prevPos = transform.position;
            // Wait till it the end of the frame
            yield return new WaitForEndOfFrame();
            // Calculate velocity: Velocity = DeltaPosition / DeltaTime
            currVel = Mathf.RoundToInt((Vector3.Distance(transform.position, prevPos) / Time.fixedDeltaTime) - minSpeed);
            minVel = Mathf.Clamp(currVel, 0, 10);
        }
    }

}
