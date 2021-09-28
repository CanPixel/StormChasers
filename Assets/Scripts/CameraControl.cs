using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraControl : MonoBehaviour {
    public float cameraRotateSpeedX = 200.0f, cameraRotateSpeedY = 3.0f;
    //public Cinemachine.CinemachineFreeLook freeLook;
    public Cinemachine.CinemachineVirtualCamera firstPersonLook, thirdPersonLook;
    private Cinemachine.CinemachinePOV pov;
    private Cinemachine.CinemachineOrbitalTransposer orbitalTransposer;

    public bool glitchyOnPurpose = false;

    public Vector3 rotationOffset;
    public GameObject cameraMascotte;

    [HideInInspector] public Vector2 rotationInput;

    [System.Serializable]
    public class CameraSystem {
        public float aim, shoot;
    
        public override string ToString() {
            return "Aim: " + aim + " || Shoot: " + shoot;
        }
    }
    public CameraSystem camSys;

    void Start() {
        pov = firstPersonLook.GetCinemachineComponent<Cinemachine.CinemachinePOV>();
        orbitalTransposer = thirdPersonLook.GetCinemachineComponent<Cinemachine.CinemachineOrbitalTransposer>();
    }

    void Update() {
        // /freeLook.m_XAxis.Value += rotationInput.x * cameraRotateSpeedX * Time.deltaTime;
        //freeLook.m_YAxis.Value += rotationInput.y * cameraRotateSpeedY * Time.deltaTime;

        if(camSys.aim > 0.4) FirstPersonLook();
        else ThirdPersonLook();

        cameraMascotte.transform.localScale = Vector3.Lerp(cameraMascotte.transform.localScale, Vector3.one, Time.deltaTime * 5f);
    }

    protected void FirstPersonLook() {
        firstPersonLook.Priority = 12;
        thirdPersonLook.Priority = 10;
        //freeLook.Priority = 10;

        cameraMascotte.SetActive(false);
    }

    protected void ThirdPersonLook() {
        firstPersonLook.Priority = 10;
        //freeLook.Priority = 12;
        thirdPersonLook.Priority = 12;

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
    }
}
