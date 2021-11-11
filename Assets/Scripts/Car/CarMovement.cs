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

    public GameObject raycastOrigin;

    public Transform carMesh;
    public CameraControl camControl;
    public CameraCanvas camCanvas;
    public Boost boostScript;

    public ArcadeKart kart;

    public float SuspensionResetSpeed = 2f, jumpHeight = 4f;

    public float jumpCheckHeight;
    private float baseAirbornReorient;

    private float baseSuspension;


    protected Vector2 moveVec = Vector2.zero;

    public PlayerInput playerInput;
    private Controls controls;
    public Rigidbody rb;

    private bool move = false;
    private float brake = 0, gas = 0, steering = 0, drift = 0, jump = 0, boost = 0;

    private Vector2 rotationInput;

    private Gamepad gamepad;
    private float hapticDuration = 0;
    public LayerMask groundCheckLayerMask;

    void Start()
    {
        baseAirbornReorient = kart.AirborneReorientationCoefficient;
        baseSuspension = kart.SuspensionHeight;
        gamepad = Gamepad.current;
        controls = new Controls();
    }

    public void OnEnable()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
        SetBrakeLights(false);
    }

    void Update()
    {
        kart.AirborneReorientationCoefficient = baseAirbornReorient * kart.GroundPercent;
        Debug.Log(kart.AirPercent + " | " + kart.GroundPercent);

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
        drift = val.Get<float>();
        MoveCalc();
        if (drift >= 0.5f)
        {
            if (steering != 0)
            {
                kart.LockDriftDirection(steering);
            }
            else MoveCalc();
        }
    }
    public void OnJump(InputValue val)
    {
        jump = val.Get<float>();

        if (Physics.Raycast(raycastOrigin.transform.position, -transform.up, jumpCheckHeight))
        {
            if (jump >= 0.5f) Jump();
        }

        //if (jump >= 0.5f && kart.AirPercent < 0.0) Jump();
    }
    public void OnBoost(InputValue val)
    {
        boost = val.Get<float>();
        Boost();
    }

    public void OnBacklook(InputValue val)
    {
        //camControl.Recenter();
        bool pressed = val.Get<float>() >= 0.4f;
        if (camControl.raceCamera)
        {
            if (pressed) camControl.BackLook(true);
            else camControl.BackLook(false);
        }
        else if (pressed) camControl.Recenter();
    }
    public void OnCycleFilter(InputValue val)
    {
        var fl = val.Get<float>();
        if (camControl != null && camControl.camSystem.aim >= 0.5f && fl != 0) camControl.CycleFilters(fl);
    }
    public void OnChangeFocus(InputValue val)
    {
        if (camCanvas != null) camCanvas.ChangeFocus(val.Get<float>());
    }
    public void OnChangeFocusSensitivity(InputValue val)
    {
        if (camCanvas != null) camCanvas.ChangeFocusSensitivity(val.Get<float>());
    }

    public void OnLooking(InputValue val)
    {
        if (camControl.ratingSystem.HasTakenPicture() && !camControl.ratingSystem.IsFading()) return;
        rotationInput = val.Get<Vector2>();
        camCanvas.SynchLook();
    }

    public void OnCameraAim(InputValue val)
    {
        if (camControl.photoBook) return;
        camControl.camSystem.aim = val.Get<float>();
        if (camControl.camSystem.aim >= 0.5f)
        {
            camControl.AnimateCameraMascotte();
            SoundManager.PlayUnscaledSound("CamMode", 2f);
            InputSystem.ResetHaptics();
        }
        else
        {
            if (camControl.HasTakenPicture()) camControl.Recenter();
            camControl.RecenterY();
        }
    }
    public void OnCameraShoot(InputValue val)
    {
        if (camControl == null) return;
        camControl.camSystem.shoot = val.Get<float>();
    }

    /* PhotoBook / Portfoio */
    public void OnPhotoBook(InputValue val)
    {
        if (camControl == null) return;
        if (val.Get<float>() >= 0.5f) camControl.photoBook = !camControl.photoBook;
    }
    public void OnScrollPortfolio(InputValue val)
    {
        if (camControl == null || !camControl.photoBook) return;
        var v = val.Get<float>();
        if (v != 0) camControl.PortfolioSelection(v);
    }
    public void OnDiscardPicture(InputValue val)
    {
        if (camControl == null || !camControl.photoBook) return;
        if (val.Get<float>() >= 0.5f) camControl.DiscardPicture();
    }
    public void OnMarkPictureForMission(InputValue val)
    {
        if (camControl == null || !camControl.photoBook) return;
        if (val.Get<float>() >= 0.5f) camControl.MarkPicture();
    }

    protected void Jump()
    {
        kart.SuspensionHeight = baseSuspension * jumpHeight;
        SoundManager.PlaySound("Jump", 0.15f);
        HapticFeedback(0.4f, 0.3f, 0.2f);
    }
    protected void Boost()
    {
        if (boost > 0.3f && gas > 0 && brake < 0.5f && !boostScript.isBoosting) boostScript.StartBoostState();
        else if (boost < 0.3f) boostScript.EndBoostState();
    }

    public void HapticFeedback(float lowFreq = 0.25f, float hiFreq = 0.75f, float duration = 1)
    {
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
        HapticFeedback(left, right, gas / 3f);
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
