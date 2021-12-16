using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class boostpad : MonoBehaviour
{

    public float boostDuration;

    public float boostForce, impulseBoostForce;

    private float defaultBoostForce, defaultImpulseForce;

    private float boostTimer;

    private bool usingBoostPad;
    private bool nearBoostPad;

    private int effectTriggerCount, boostTriggerCount;


    private Vector3 startingScale;

    private float effectTimer = 0;


    public boostEffectTrigger boostTrigger;
    public Boost boostScript;

    // Start is called before the first frame update
    void Start()
    {
        boostTimer = boostDuration;
        usingBoostPad = false;
        startingScale = transform.localScale;

        defaultBoostForce = boostScript.boostForce;
        defaultImpulseForce = boostScript.impulseBoostForce;
    }

    // Update is called once per frame
    void Update()
    {
        CheckForTrigger();

        if (usingBoostPad)
        {
            BoostTimer();
            //BoostCircleEffect();
        }


    }

    void CheckForTrigger()
    {


        if (boostTrigger.hasTriggered && boostTriggerCount < 1)
        {
            //if (boostScript.isBoosting) boostScript.EndBoostState();

            boostTriggerCount++;
            Debug.Log("BoostColCheck");
            boostScript.usingBoostPad = usingBoostPad = true;
            boostScript.boostForce = boostForce;
            boostScript.impulseBoostForce = impulseBoostForce;
            boostScript.StartBoostState();
        }
    }

    void BoostTimer()
    {
        if (boostTimer > 0) boostTimer -= Time.deltaTime;
        else if (boostTimer < 0)
        {
            ResetBoostPad();
        }
    }

    /*
        void BoostCircleEffect()
        {
            float effectDuration = .25f;
            float shrinkFactor = .8f;
            float spinSpeed = 8f;

            if (effectTimer < effectDuration)
            {
                effectTimer += Time.deltaTime;
                transform.localScale = Vector3.Lerp(startingScale, startingScale * shrinkFactor, effectTimer);
                transform.eulerAngles += new Vector3(spinSpeed, 0, 0);
            }
        }
        */

    void ResetBoostPad()
    {
        boostScript.usingBoostPad = usingBoostPad = false;
        boostScript.boostForce = defaultBoostForce;
        boostScript.impulseBoostForce = defaultImpulseForce;

        boostScript.EndBoostState();
        boostTimer = boostDuration;
        boostTriggerCount = 0;

        //Effects 
        effectTimer = 0;
        transform.localScale = startingScale;
    }
}