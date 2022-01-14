using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using KartGame.KartSystems;
using Cinemachine;
using UnityEngine.UI;

public class CarMovement : MonoBehaviour
{
    public Light[] brakeLights;
    public Material brakeMaterial;

    public float momentumReduction = 2f;
    public float maxSpeedCap = 100;

    public Transform carMesh;
    public DialogSystem dialogSystem;
    public CameraControl camControl;
    public CameraCanvas camCanvas;
    public Boost boostScript;

    public ArcadeKart kart;

    public float SuspensionResetSpeed = 2f, jumpHeight = 4f;
    public float jumpCheckHeight = 2f;

    private float baseAirbornReorient;

    private float baseSuspension;
    public bool isGrounded; 


    protected Vector2 moveVec = Vector2.zero;

    public PlayerInput playerInput;
    private Controls controls;
    public Rigidbody rb;

    private bool move = false;
    [HideInInspector] public float brake = 0, gas = 0, steering = 0, drift = 0, jump = 0;

    private Vector2 rotationInput;

    private Gamepad gamepad;
    private float hapticDuration = 0;

    private float baseAirborneReorient;

    void Start() {
        baseAirborneReorient = kart.AirborneReorientationCoefficient;
        baseSuspension = kart.SuspensionHeight;
        gamepad = Gamepad.current;
        controls = new Controls();
    }

    public void OnEnable() {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
        SetBrakeLights(false);
    }

    void Update() {
        Vector3 currentVelocity = rb.velocity;
        currentVelocity.y = 0; 

        //Max speed cap
        if(Mathf.Abs(currentVelocity.magnitude) > maxSpeedCap) {
            var finSpeed = rb.velocity.normalized * maxSpeedCap; 
            finSpeed.y = rb.velocity.y;
            rb.velocity = finSpeed;
        }

        kart.AirborneReorientationCoefficient = baseAirborneReorient * kart.AirPercent;

        if (steering == 0 && drift >= 0.5f)
        {
            kart.ActivateDriftVFX(true);
            kart.IsDrifting = true;
        }

        if (hapticDuration > 0) hapticDuration -= Time.unscaledDeltaTime;
        else InputSystem.ResetHaptics();

        if (Input.GetKeyDown(KeyCode.R)) UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);

        kart.SuspensionHeight = Mathf.Lerp(kart.SuspensionHeight, baseSuspension, Time.deltaTime * SuspensionResetSpeed);
    }

    public void OnBrake(InputValue val)
    {
        brake = val.Get<float>();
        SetBrakeLights(brake > 0);
        MoveCalc();
    }
    public void OnGas(InputValue val)
    {
        gas = val.Get<float>();
        MoveCalc();
    }
    public void OnSteer(InputValue val)
    {
        steering = val.Get<float>();
        MoveCalc();
    }
    public void OnDrift(InputValue val)
    {
        if (camControl.journal)
        {
            drift = 0;
            return;
        }
        drift = val.Get<float>();
        MoveCalc();
        if (drift >= 0.5f)
        {
            if (steering != 0) kart.LockDriftDirection(steering);
            else MoveCalc();
        }
    }
    public void OnJump(InputValue val)
    {
        if (camControl.journal && val.Get<float>() >= 0.5f)
        {
            camControl.JournalSelect();
            return;
        }
        jump = val.Get<float>();
        //if(jump >= 0.5f && kart.GroundPercent > 0.0f ) Jump();
        if (jump >= 0.5f && Physics.Raycast(transform.position, -transform.up, jumpCheckHeight))
        {
            
            Jump();
        }
       
    }
    public void OnBoost(InputValue val)
    {
        var valu = val.Get<float>();
        if (camControl.journal && valu >= 0.5f)
        {
            if (camControl.photobook) camControl.ShowJournalBaseScreen();
            else camControl.journal = false;
            return;
        }
        Boost(valu);
    }

    public void OnBacklook(InputValue val)
    {
        bool pressed = val.Get<float>() >= 0.4f;
        if (pressed && camControl.camSystem.aim <= 0.5f) camControl.BackLook(true);
        else camControl.BackLook(false);
    }
    public void OnCycleFilter(InputValue val) {
        var fl = val.Get<float>();
        if (fl != 0) {
            if (camControl.journal) ;//camControl.ShowJournalInfo();
            else if(camControl.camSystem.aim >= 0.5) camControl.CycleFilters(fl);
        }
    }
    public void OnChangeFocus(InputValue val) {
        camCanvas.ChangeFocus(val.Get<float>());
    }

    public void OnLooking(InputValue val) {
        //if (camControl.ratingSystem.HasTakenPicture() && !camControl.ratingSystem.IsFading()) return;
        rotationInput = val.Get<Vector2>();
        //camCanvas.SynchLook();
    }

    public void OnCameraAim(InputValue val) {
        var ding = val.Get<float>();

        camControl.camSystem.aim = ding;
        if (camControl.camSystem.aim >= 0.5f) {
            camControl.ShowJournalBaseScreen();
            camControl.journal = camControl.photobook = false;
            camControl.AnimateCameraMascotte();
            SoundManager.PlayUnscaledSound("CamMode", 2f);
            InputSystem.ResetHaptics();
        } else {
            if (camControl.HasTakenPicture()) camControl.Recenter();
            camControl.RecenterY();
        }
    }

    public void OnCameraShoot(InputValue val) {
        if (camControl == null) return;
        camControl.camSystem.shoot = val.Get<float>();
        if (camControl.camSystem.shoot >= 0.4f && camControl.camSystem.aim <= 0.3f) dialogSystem.CompleteTypewriterSentence();
    }

    /* PhotoBook / Portfoio */
    public void OnPhotoBook(InputValue val) {
        if (camControl == null || camControl.camSystem.aim >= 0.4f) {
            camControl.journal = camControl.photobook = false;
            return;
        }
        if (val.Get<float>() >= 0.5f) {
            camControl.journal = !camControl.journal;
            if (camControl.journal) camControl.OpenJournal();
            else camControl.ShowJournalBaseScreen();
        }
    }
    public void OnScrollPortfolio(InputValue val) {
        if (camControl == null || !camControl.journal) return;
        var v = val.Get<Vector2>();
        if (v != Vector2.zero)
        {
            if (!camControl.photobook) camControl.JournalScroll(v);
            else camControl.PortfolioSelection(v);
        }
    }
    public void OnDiscardPicture(InputValue val) {
        if (camControl == null || !camControl.journal || !camControl.photobook) return;
        if (val.Get<float>() >= 0.5f) camControl.DiscardPicture();
    }

    public void Jump() {
        var front = (kart.FrontLeftWheel.transform.position.y + kart.FrontRightWheel.transform.position.y) / 2f;
        var back = (kart.RearLeftWheel.transform.position.y + kart.RearRightWheel.transform.position.y) / 2f;
        // kart.SuspensionHeight = baseSuspension * (jumpHeight * Mathf.Clamp01(1f - (front - back)));
        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z); 

        var jumpDirection = transform.up + rb.velocity.normalized / momentumReduction;
        rb.AddForce(jumpDirection * jumpHeight, ForceMode.VelocityChange);
       
        SoundManager.PlaySound("Jump", 0.15f);
        HapticManager.Haptics("Jump");
    }
    protected void Boost(float boost) {
        if (camControl.camSystem.aim >= 0.5f)
        {
            boostScript.EndBoostState();
            //if (boost > 0.35f) camCanvas.ChangeSpeedToggle();
            return;
        }

        if (boost > 0.3f && gas > 0 && brake < 0.5f && !boostScript.isBoosting) boostScript.StartBoostState();
        else if (boost < 0.3f) boostScript.EndBoostState();
    }

    public void HapticFeedback(HapticManager.HapticFeedback haptic) {
        if (gamepad == null) return;
        gamepad.SetMotorSpeeds(haptic.lowFreq, haptic.hiFreq);
        hapticDuration = haptic.duration;
    }

    public void HapticFeedback(float lowFreq = 0.25f, float hiFreq = 0.75f, float duration = 1) {
        if (gamepad == null) return;
        gamepad.SetMotorSpeeds(lowFreq, hiFreq);
        hapticDuration = duration;
    } 

    protected void SetBrakeLights(bool on)
    {
        foreach (var i in brakeLights) i.enabled = on;
        brakeMaterial.SetColor("_EmissionColor", on ? new Color(1, 0, 0) : new Color(0, 0, 0));
        brakeMaterial.SetColor("_Color", on ? new Color(1, 0, 0) : new Color(0, 0, 0));
    }

    protected void MoveCalc()
    {
        float driftStop = 1;
        if (steering == 0 && drift >= 0.5f) driftStop = 0;
        float baseVel = gas * driftStop - brake * (1 - driftStop);
        if (gas > 0 && brake > 0) baseVel = 0;
        if (gas <= 0 && brake > 0) baseVel = -1;

        moveVec = new Vector2(steering, baseVel * driftStop);
        move = (moveVec != Vector2.zero);

        float left = 0.1f;
        float right = 0.1f;
        if (steering > 0) left = 0;
        if (steering < 0) right = 0;
        if (drift >= 0.5f)
        {
            right *= 2f;
            left *= 2f;
        }
        //HapticFeedback(left, right, gas / 3f);
    }

    //private void ChangeBinding() {
    //InputBinding binding = triggerAction.action.bindings[0];
    //binding.overridePath = "<Keyboard>/#(g)";
    //triggerAction.action.ApplyBindingOverride(0, binding);
    //}

    public float IsGassing()
    {
        return moveVec.y;
    }
    public float IsSteering()
    {
        return moveVec.x;
    }
    public bool IsBraking()
    {
        return brake > 0;
    }
    public bool IsDrifting()
    {
        return drift > 0;
    }

    public Vector2 GetLooking()
    {
        return rotationInput;
    }
}
