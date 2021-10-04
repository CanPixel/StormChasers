using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using KartGame.KartSystems;
using Cinemachine;

public class CarMovement : MonoBehaviour {
    public Light[] brakeLights;
    public Material brakeMaterial;

    public CameraControl camControl;
    public CameraCanvas camCanvas;

    public ArcadeKart kart;

    public float SuspensionResetSpeed = 2f, jumpHeight = 4f;

    private float baseSuspension;
    private float currentSpeed = 0;
    protected Vector2 moveVec = Vector2.zero;

    private PlayerInput playerInput;
    private Controls controls;
    private Rigidbody rb;
    private StatBoost statBoost;

    private bool move = false;
    private float brake = 0, gas = 0, steering = 0, drift = 0, jump = 0, boost = 0;

    private Vector2 rotationInput;

    void Start() {
        baseSuspension = kart.SuspensionHeight;
    }

    public void OnEnable() {
        statBoost = GetComponent<StatBoost>();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
        SetBrakeLights(false);
        if(controls == null) controls = new Controls();
        if(rb == null) rb = GetComponent<Rigidbody>();
        if(playerInput == null) playerInput = GetComponent<PlayerInput>();
        currentSpeed = 0;
    }

    void Update() {
        if(steering == 0 && drift >= 0.5f) {
            kart.ActivateDriftVFX(true);
            kart.IsDrifting = true;
        }

        camControl.rotationInput = rotationInput;

        if(Input.GetKeyDown(KeyCode.R)) UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);

        //freeLook.m_XAxis.Value += rotationInput.x * cameraRotateSpeedX * Time.deltaTime;
        //freeLook.m_YAxis.Value += rotationInput.y * cameraRotateSpeedY * Time.deltaTime;

        kart.SuspensionHeight = Mathf.Lerp(kart.SuspensionHeight, baseSuspension, Time.deltaTime * SuspensionResetSpeed);
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
        if(drift >= 0.5f) {
            if(steering != 0) kart.LockDriftDirection(steering);
            else MoveCalc();
        } 
    }
    public void OnJump(InputValue val) {
        jump = val.Get<float>();
        if(jump >= 0.5f && kart.GroundPercent > 0.0f) Jump();
    }
    public void OnBoost(InputValue val) {
        if(statBoost == null) return;
        boost = val.Get<float>();
        if(boost >= 0.4f && gas > 0 && brake < 0.5f) Boost();
    }

    public void OnRecenter(InputValue val) {
        if(val.Get<float>() >= 0.4f) camControl.Recenter();
    }
    public void OnCycleFilter(InputValue val) {
        var fl = val.Get<float>();
        if(camControl.camSystem.aim >= 0.5f && fl != 0) camControl.CycleFilters(fl);
    }
    public void OnChangeFocus(InputValue val) {
        camCanvas.ChangeFocus(val.Get<float>());
    }

    public void OnCameraAim(InputValue val) {
        camControl.camSystem.aim = val.Get<float>();
        if(camControl.camSystem.aim >= 0.5f) camControl.AnimateCameraMascotte();
        else camControl.Recenter();
        //SwitchControlMap(camControl.camSys.aim >= 0.5 ? "CameraControls" : "VehicleControls");
    }
    public void OnCameraShoot(InputValue val) {
        camControl.camSystem.shoot = val.Get<float>();
    }
    public void OnPhotoBook(InputValue val) {
        if(val.Get<float>() >= 0.5f) camControl.photoBook = !camControl.photoBook;
    }

    protected void Jump() {
        kart.SuspensionHeight = baseSuspension * jumpHeight;
        SoundManager.PlaySound("Jump");
    }

    protected void Boost() {
        statBoost.TriggerStatBoost();
        SoundManager.PlaySound("Boost");
    }

    protected void SetBrakeLights(bool on) {
        foreach(var i in brakeLights) i.enabled = on;
        brakeMaterial.SetColor("_EmissionColor", on ? new Color(1, 0, 0) : new Color(0, 0, 0));
        brakeMaterial.SetColor("_Color", on ? new Color(1, 0, 0) : new Color(0, 0, 0));
    }

    protected void MoveCalc() {
        float driftStop = 1;
        if(steering == 0 && drift >= 0.5f) driftStop = 0;
        float baseVel = gas * driftStop - brake * (1 - driftStop);
        if(gas > 0 && brake > 0) baseVel = 0;
        if(gas <= 0 && brake > 0) baseVel = -1;

        moveVec = new Vector2(steering, baseVel * driftStop);
        move = (moveVec != Vector2.zero);
    }

    public void OnLooking(InputValue val) {
        rotationInput = val.Get<Vector2>();
        camCanvas.SynchLook();
    }

    private void ChangeBinding() {
        //InputBinding binding = triggerAction.action.bindings[0];
        //binding.overridePath = "<Keyboard>/#(g)";
        //triggerAction.action.ApplyBindingOverride(0, binding);
    }

    protected void SwitchControlMap(string map) {
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

    public Vector2 GetLooking() {
        return rotationInput;
    }
}
