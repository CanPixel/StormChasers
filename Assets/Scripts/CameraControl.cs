using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.Rendering.PostProcessing;

public class CameraControl : MonoBehaviour {
    public float cameraRotateSpeedX = 200.0f, cameraRotateSpeedY = 3.0f;
    //public Cinemachine.CinemachineFreeLook freeLook;
    public Cinemachine.CinemachineVirtualCamera firstPersonLook, thirdPersonLook;
    private Cinemachine.CinemachinePOV pov;
    private Cinemachine.CinemachineOrbitalTransposer orbitalTransposer;

    public Cinemachine.PostFX.CinemachinePostProcessing postProcessing;

    public GameObject[] enableOnFirstPerson;

    public bool glitchyOnPurpose = false;

    public enum CameraState {
        CARVIEW = 0, CAMVIEW, DASHVIEW
    }
    public CameraState cameraState = CameraState.CARVIEW;

    public Vector3 rotationOffset;
    public GameObject cameraMascotte;

    [HideInInspector] public Vector2 rotationInput;

    [System.Serializable]
    public class CameraSystem {
        public float aim, shoot, focus;

        public PostProcessProfile[] filters;
        [HideInInspector] public int filterIndex = 0;

        public override string ToString() {
            return "Aim: " + aim + " || Shoot: " + shoot;
        }
    }
    public CameraSystem camSystem;

    private float recenterTime = 0;
    public float recenterDuration = 1f;

    void Start() {
        pov = firstPersonLook.GetCinemachineComponent<Cinemachine.CinemachinePOV>();
        orbitalTransposer = thirdPersonLook.GetCinemachineComponent<Cinemachine.CinemachineOrbitalTransposer>();
        foreach(var i in enableOnFirstPerson) i.SetActive(false); 
    }

    void Update() {
        // /freeLook.m_XAxis.Value += rotationInput.x * cameraRotateSpeedX * Time.deltaTime;
        //freeLook.m_YAxis.Value += rotationInput.y * cameraRotateSpeedY * Time.deltaTime;

        if(recenterTime > 0) {
            recenterTime -= Time.deltaTime;
            orbitalTransposer.m_XAxis.Value = Mathf.Lerp(orbitalTransposer.m_XAxis.Value, 0, Time.deltaTime * 4f);
        }

        if(camSystem.aim > 0.4) FirstPersonLook();
        else ThirdPersonLook();

        cameraMascotte.transform.localScale = Vector3.Lerp(cameraMascotte.transform.localScale, Vector3.one, Time.deltaTime * 5f);
    }

    public void CycleFilters(float add) {
        camSystem.filterIndex += (int)add;
        if(camSystem.filterIndex >= camSystem.filters.Length) camSystem.filterIndex = 0;
        if(camSystem.filterIndex < 0) camSystem.filterIndex = camSystem.filters.Length - 1;
        postProcessing.m_Profile = camSystem.filters[camSystem.filterIndex];
    }

    protected void FirstPersonLook() {
        firstPersonLook.Priority = 12;
        thirdPersonLook.Priority = 10;
        //freeLook.Priority = 10;

        if(cameraState != CameraState.CAMVIEW) {
            foreach(var i in enableOnFirstPerson) i.SetActive(true); 
            cameraState = CameraState.CAMVIEW;
        }

        cameraMascotte.SetActive(false);
    }

    protected void ThirdPersonLook() {
        firstPersonLook.Priority = 10;
        //freeLook.Priority = 12;
        thirdPersonLook.Priority = 12;

        if(cameraState != CameraState.CARVIEW) {
            foreach(var i in enableOnFirstPerson) i.SetActive(false); 
            cameraState = CameraState.CARVIEW;
        }

        cameraMascotte.SetActive(true);

        cameraMascotte.transform.rotation = firstPersonLook.transform.rotation;
        cameraMascotte.transform.Rotate(rotationOffset);
        cameraMascotte.transform.eulerAngles += new Vector3(0, orbitalTransposer.m_XAxis.Value + transform.eulerAngles.y, 0);

        
        pov.m_HorizontalAxis.Value = cameraMascotte.transform.localEulerAngles.z + transform.eulerAngles.y;
    }

    public void AnimateCameraMascotte() {
        cameraMascotte.transform.localScale = Vector3.one * 1.2f;
    }

    public void Recenter() {
/*         orbitalTransposer.m_RecenterToTargetHeading = new Cinemachine.AxisState.Recentering();
        orbitalTransposer.m_RecenterToTargetHeading.DoRecentering(); */
        if(glitchyOnPurpose) orbitalTransposer.ForceCameraPosition(orbitalTransposer.transform.position, transform.localRotation);
        orbitalTransposer.m_XAxis.Value = 0;
        recenterTime = recenterDuration;
    }
}
