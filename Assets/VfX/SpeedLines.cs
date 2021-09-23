using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedLines : MonoBehaviour
{

    public ParticleSystem speedPS;
    private int lineSpeed;
    private int currVel;
    private int minVel;
    private Vector3 prevPos;


    void Start()
    {
        StartCoroutine(CalcVelocity());
    }

    void FixedUpdate()
    {
        lineSpeed = (minVel * minVel) * 15;
        ParticleSystem.EmissionModule psEmission = speedPS.emission;
        psEmission.rateOverTime = lineSpeed;
        Debug.Log(lineSpeed);
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
            currVel = Mathf.RoundToInt((Vector3.Distance(transform.position, prevPos) / Time.fixedDeltaTime) - 5.0f);
            minVel = Mathf.Clamp(currVel, 0, 10);
        }
    }
}
