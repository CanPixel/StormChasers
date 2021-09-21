using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMovement : MonoBehaviour {
    public Camera playerCam;
    public Transform CamPosition;
    public float damping = 1f;

    public Cinemachine.CinemachineFreeLook freeLook;

    public float cameraRotateSpeedX = 10.0f, cameraRotateSpeedY = 10.0f;
    private float MouseX = 0, MouseY = 0;

    public bool enableCamMovement = true;
    //public float returnCameraAfter = 3f;

    private float currentSpeed = 0;
    protected Vector2 moveVec = Vector2.zero;

    private PlayerInput playerInput;
    private Controls controls;
    private Rigidbody rb;

    private bool move = false;
    private float brake = 0, gas = 0, steering = 0, drift = 0;

    //public Vector3 camOffset;
    private float returnCamTimer = 0;

    private Vector2 rotationInput;

    public void OnEnable() {
     //   Cursor.visible = false;
        //Cursor.lockState = CursorLockMode.Confined;
        if(controls == null) controls = new Controls();
        if(rb == null) rb = GetComponent<Rigidbody>();
        if(playerInput == null) playerInput = GetComponent<PlayerInput>();
        currentSpeed = 0;
    }

/*     void FixedUpdate() {
       float RotationX = cameraRotateSpeed * MouseX * Time.deltaTime;
       float RotationY = cameraRotateSpeed * MouseY * Time.deltaTime;

        Vector3 CamRot = playerCam.transform.rotation.eulerAngles;
        CamRot.x -= RotationY;
        CamRot.y += RotationX;
    } */

    void Update() {
        freeLook.m_XAxis.Value += rotationInput.x * cameraRotateSpeedX * Time.deltaTime;
        freeLook.m_YAxis.Value += rotationInput.y * cameraRotateSpeedY * Time.deltaTime;

        if(!enableCamMovement) return;
        //playerCam.transform.position = Vector3.Lerp(playerCam.transform.position, CamPosition.position, Time.deltaTime * damping * 2f);
        //playerCam.transform.rotation = Quaternion.Lerp(playerCam.transform.rotation, CamPosition.rotation, Time.deltaTime * damping);
        
/*         playerCam.transform.position = Vector3.Lerp(playerCam.transform.position, transform.position + camOffset, Time.deltaTime * damping * 2f);
        playerCam.transform.rotation = Quaternion.Lerp(playerCam.transform.rotation, Quaternion.Euler(playerCam.transform.eulerAngles.x, transform.eulerAngles.y, 0), Time.deltaTime * damping);
         */
        //float RotationX = cameraRotateSpeed * MouseX * Time.deltaTime;
        //camOffset = Quaternion.AngleAxis(RotationX, Vector3.up) * camOffset;

        //playerCam.transform.Rotate(0, RotationX, 0);
        //float RotationY = cameraRotateSpeed * MouseY * Time.deltaTime;

        float desired = transform.eulerAngles.y;
        var rotation = Quaternion.Euler(0, desired, 0);

        //playerCam.transform.position = Vector3.Lerp(playerCam.transform.position, transform.position + camOffset, Time.deltaTime * damping);
        playerCam.transform.LookAt(transform.position);

       // FreeLook();
    }

  /*   protected void FreeLook() {
        if(rotationInput == Vector2.zero) returnCamTimer += Time.deltaTime;
        else returnCamTimer = 0;
    
        if(returnCamTimer > returnCameraAfter) {

        }


        var radius = camOffset;
     //   var angle = 
        playerCam.transform.position = transform.position;
    } */

    public void OnBrake(InputValue val) {
        brake = val.Get<float>();
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
    }

    protected void MoveCalc() {
        moveVec = new Vector2(steering, gas - brake);
        move = (moveVec != Vector2.zero);
    }

    public void OnRotationX(InputValue val) {
        if(playerInput.currentControlScheme == "Gamepad") return;
        MouseX = val.Get<float>();
    }
    public void OnRotationY(InputValue val) {
        if(playerInput.currentControlScheme == "Gamepad") return;
        MouseY = val.Get<float>();
    }
    public void OnRotation(InputValue val) {
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

/*     public Vector2 GetMousePos() {
        return Mouse.current.position.ReadValue();
    } */

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

/*     public Vector2 GetMouse() {
        return new Vector2(MouseX, MouseY);
    } */
}
