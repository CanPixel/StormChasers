using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

public class Boost : MonoBehaviour {
    [Header("FOV")] 
    public Vector2 fovChange = new Vector2(40, 75);
    public float fovChangeSpeed = 5f;
    private float fovTarget;

    [Header("Boost")] 
    public float currentBoostFuel;
    public float maxBoostFuel;
    public float minBoostFuel; 
    public float boostExpandAmount; 
    public float boostRegenSpeed; 
    public float boostRegenCooldown = 2f; 
    private float boostRegenTimer;
    public float boostForce;
    public float impulseBoostForce;

    private bool canBoost = true; 
    public bool isBoosting {
        get; private set;
    } 
    private bool canRegenFuel = true; 

    private float boostPressedInterval = 0;
    public float boostSpamCap = 1f;

    public Rigidbody carRb; 
    public Image boostFill; 
    public GameObject doorLeftStatic, doorRightStatic, doorLeft, doorRight;
    public Cinemachine.CinemachineVirtualCamera virtualCamera;
    public CarMovement carMovement;
    public GameObject speedLinesFX;
    public Image boostOverlay;
    private float boostAlpha;

    void Start() {
        currentBoostFuel = maxBoostFuel; 
        fovTarget = virtualCamera.m_Lens.FieldOfView;
        boostAlpha = boostOverlay.color.a;

        SetHingesStatic(true);
    }

    void Update() {
        if(boostPressedInterval > 0) boostPressedInterval -= Time.unscaledDeltaTime;
        boostOverlay.color = Color.Lerp(boostOverlay.color, new Color(1, 1, 1, 0), Time.unscaledDeltaTime * 2.3f);

        if(isBoosting) {
            ExpandFuel(); 
            ApplyBoost();
        }
        if(canRegenFuel) RegenFuel();

        if(boostRegenTimer > 0) {
            boostRegenTimer += Time.deltaTime;

            if(boostRegenTimer > boostRegenCooldown) {
                if(!isBoosting) {
                    canRegenFuel = true; 
                    boostRegenTimer = 0;
                }
            }
        }

        boostFill.fillAmount = currentBoostFuel / maxBoostFuel;

        doorLeftStatic.transform.localRotation = Quaternion.Lerp(doorLeftStatic.transform.localRotation, Quaternion.identity, Time.deltaTime * 3f);
        doorRightStatic.transform.localRotation = Quaternion.Lerp(doorRightStatic.transform.localRotation, Quaternion.identity, Time.deltaTime * 3f);

        virtualCamera.m_Lens.FieldOfView = Mathf.Lerp(virtualCamera.m_Lens.FieldOfView, fovTarget, Time.deltaTime * fovChangeSpeed);
    }

    public void StartBoostState() {       
        if(boostPressedInterval > 0) return;

        if(canBoost) {
            isBoosting = true; 
            canRegenFuel = false;
            boostRegenTimer = 0;
            SetHingesStatic(false);
            speedLinesFX.SetActive(true);
            fovTarget = fovChange.y;
            carRb.AddForce(carRb.transform.forward * impulseBoostForce, ForceMode.VelocityChange);
            carMovement.HapticFeedback(0.8f, 0.2f, 0.3f);
            
            SoundManager.PlaySound("Boost", 0.7f);
            BoostOverlay();
            
            boostPressedInterval = boostSpamCap;
        }
    }
    public void EndBoostState() {
        isBoosting = false;
        fovTarget = fovChange.x;
        speedLinesFX.SetActive(false);
        SetHingesStatic(true);
        boostRegenTimer = 0.1f;    
        SoundManager.StopSound("Boost");
    }

    private void ExpandFuel() {
        currentBoostFuel -= boostExpandAmount * Time.deltaTime; 

        //Out of fuel - empty bar
        if(currentBoostFuel <= minBoostFuel){
            currentBoostFuel = minBoostFuel; 
            canBoost = false; 
            EndBoostState();
        }
    }

    public void BoostOverlay() {
        boostOverlay.color = new Color(1, 1, 1, boostAlpha);
    }

    public void ApplyBoost() {    
        carRb.AddForce(carRb.transform.forward * boostForce, ForceMode.Acceleration);
        carMovement.HapticFeedback(0.4f, 0.8f, 0.5f);
    }

    private void RegenFuel() {
        if(currentBoostFuel < maxBoostFuel) currentBoostFuel += boostRegenSpeed * Time.deltaTime;
        else currentBoostFuel = maxBoostFuel;      

        canBoost = true;
    }

    private void RegainInstantFuel(float fuelAmount) {
        if((currentBoostFuel + fuelAmount) < maxBoostFuel) currentBoostFuel += fuelAmount;        
        else currentBoostFuel = maxBoostFuel;  

        canBoost = true;
    }

    public void SetHingesStatic(bool enabled) {
        doorLeftStatic.SetActive(enabled);
        doorRightStatic.SetActive(enabled);
        doorLeft.SetActive(!enabled);
        doorRight.SetActive(!enabled);
        doorLeftStatic.transform.localEulerAngles = doorLeft.transform.localEulerAngles;
        doorRightStatic.transform.localEulerAngles = doorRight.transform.localEulerAngles;
        doorLeft.transform.localEulerAngles = doorRight.transform.localEulerAngles = Vector3.zero;
    }
}