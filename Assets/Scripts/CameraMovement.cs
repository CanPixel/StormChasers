using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using KartGame.KartSystems;

public class CameraMovement : MonoBehaviour {
    public Light[] brakeLights;
    public Material brakeMaterial;

    public ArcadeKart kart;
    public Cinemachine.CinemachineFreeLook freeLook;

    public float cameraRotateSpeedX = 10.0f, cameraRotateSpeedY = 10.0f;

    private float currentSpeed = 0;
    protected Vector2 moveVec = Vector2.zero;

    private PlayerInput playerInput;
    private Controls controls;
    private Rigidbody rb;

    private bool move = false;
    private float brake = 0, gas = 0, steering = 0, drift = 0;

    private Vector2 rotationInput;

    public void OnEnable() {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
        SetBrakeLights(false);
        if(controls == null) controls = new Controls();
        if(rb == null) rb = GetComponent<Rigidbody>();
        if(playerInput == null) playerInput = GetComponent<PlayerInput>();
        currentSpeed = 0;
    }

    void Update() {
        freeLook.m_XAxis.Value += rotationInput.x * cameraRotateSpeedX * Time.deltaTime;
        freeLook.m_YAxis.Value += rotationInput.y * cameraRotateSpeedY * Time.deltaTime;
    }

    public void OnBrake(InputValue val) {
        brake = val.Get<float>();

        SetBrakeLights(brake > 0);
        MoveCalc();
    }
    public void OnGas(InputValue val) {
        gas = val.Get<float>();
        MoveCalc();
    }
    public void OnSteer(InputValue val) {
        steering = val.Get<float>();
        MoveCalc();
    }
    public void OnDrift(InputValue val) {
        drift = val.Get<float>();
        MoveCalc();
        if(drift == 1 && steering != 0) kart.LockDriftDirection(steering);
    }

    protected void SetBrakeLights(bool on) {
        foreach(var i in brakeLights) i.enabled = on;
        brakeMaterial.SetColor("_EmissionColor", on ? new Color(1, 0, 0) : new Color(0, 0, 0));
        brakeMaterial.SetColor("_Color", on ? new Color(1, 0, 0) : new Color(0, 0, 0));
    }

    protected void MoveCalc() {
        moveVec = new Vector2(steering, gas - brake);
        move = (moveVec != Vector2.zero);
    }

    public void OnLooking(InputValue val) {
        rotationInput = val.Get<Vector2>();
    }

    private void ChangeBinding() {
        //InputBinding binding = triggerAction.action.bindings[0];
        //binding.overridePath = "<Keyboard>/#(g)";
        //triggerAction.action.ApplyBindingOverride(0, binding);
    }

    protected void SwitchControls(string map) {
        playerInput.SwitchCurrentActionMap(map);
    }

    public float IsGassing() {
        return moveVec.y;
    }

    public float IsSteering() {
        return moveVec.x;
    }

    public bool IsBraking() {
        return brake > 0;
    }

    public bool IsDrifting() {
        return drift > 0;
    }
}
