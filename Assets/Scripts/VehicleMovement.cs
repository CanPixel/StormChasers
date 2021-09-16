using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class VehicleMovement : MonoBehaviour {
    public Camera playerCam;
    public Transform CamPosition;
    public float speed = 2f;
    public float acceleration = 10f;
    public float damping = 1f;

    public float cameraRotateSpeed = 10.0f;
    private float MouseX = 0, MouseY = 0;

    public bool enableCamMovement = true;

    private float currentSpeed = 0;
    protected Vector2 moveVec = Vector2.zero;

    private PlayerInput playerInput;
    private Controls controls;
    private Rigidbody rb;

    private bool move = false;
    private float brake = 0;

    private Vector3 camBaseOffset;

    void Start() {
        camBaseOffset = playerCam.transform.position;
    }

    public void OnEnable() {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
        if(controls == null) controls = new Controls();
        if(rb == null) rb = GetComponent<Rigidbody>();
        if(playerInput == null) playerInput = GetComponent<PlayerInput>();
        currentSpeed = 0;
    }

   // void FixedUpdate() {
     //   return;
        //currentSpeed = Mathf.Lerp(currentSpeed, (move) ? speed : -0.1f, Time.deltaTime * acceleration);

//        moveVec = Vector3.Lerp(moveVec, Vector3.one * (move ? currentSpeed : 0), Time.deltaTime);

       // rb.velocity = moveVec.y * transform.forward * currentSpeed;

       // float RotationX = horizontalSensitivity * MouseX * Time.deltaTime;
       // float RotationY = verticalSensitivity * MouseY * Time.deltaTime;

        //Vector3 CamRot = playerCam.transform.rotation.eulerAngles;
        //CamRot.x -= RotationY;
        //CamRot.y += RotationX;
    //}

    void Update() {
        return;
        playerCam.transform.position = Vector3.Lerp(playerCam.transform.position, CamPosition.position, Time.deltaTime * damping * 2f);
        playerCam.transform.rotation = Quaternion.Lerp(playerCam.transform.rotation, CamPosition.rotation, Time.deltaTime * damping);
        
        //playerCam.transform.position = Vector3.Lerp(playerCam.transform.position, transform.position + camBaseOffset, Time.deltaTime * damping * 2f);
        //playerCam.transform.rotation = Quaternion.Lerp(playerCam.transform.rotation, Quaternion.Euler(playerCam.transform.eulerAngles.x, transform.eulerAngles.y, 0), Time.deltaTime * damping);
        
        float steer = cameraRotateSpeed * moveVec.x * Time.deltaTime;
        transform.Rotate(0, steer, 0);





        float RotationX = cameraRotateSpeed * MouseX * Time.deltaTime;
        camBaseOffset = Quaternion.AngleAxis(RotationX, Vector3.up) * camBaseOffset;

        playerCam.transform.Rotate(0, RotationX, 0);
        float RotationY = cameraRotateSpeed * MouseY * Time.deltaTime;

        float desired = transform.eulerAngles.y;
        var rotation = Quaternion.Euler(0, desired, 0);

        //playerCam.transform.position = Vector3.Lerp(playerCam.transform.position, transform.position + camBaseOffset, Time.deltaTime * damping);
        //playerCam.transform.LookAt(transform.position);
    }

    public void OnBrake(InputValue val) {
        brake = val.Get<float>();
    }

    public void OnMovement(InputValue value){
        //context.action.bindings[0];
        //context.control.
        //Debug.Log(value.Get<Vector2>());

        moveVec = value.Get<Vector2>();
        move = (moveVec != Vector2.zero);
    }
    public void OnRotationX(InputValue val) {
        MouseX = val.Get<float>();
    }
    public void OnRotationY(InputValue val) {
        MouseY = val.Get<float>();
    }

    private void ChangeBinding() {
        //InputBinding binding = triggerAction.action.bindings[0];
        //binding.overridePath = "<Keyboard>/#(g)";
        //triggerAction.action.ApplyBindingOverride(0, binding);
    }

    protected void SwitchControls(string map) {
        playerInput.SwitchCurrentActionMap(map);
    }

    public Vector2 GetMousePos() {
        return Mouse.current.position.ReadValue();
    }

    public float IsGassing() {
        return moveVec.y;
    }

    public float IsSteering() {
        return moveVec.x;
    }

    public bool IsBraking() {
        return moveVec.y < 0 || brake > 0;
    }

    public Vector2 GetMouse() {
        return new Vector2(MouseX, MouseY);
    }
}