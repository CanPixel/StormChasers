using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine.PostFX;
using UnityEngine.Rendering.PostProcessing;

public class CameraCanvas : MonoBehaviour {
    public CarMovement player;
    public CinemachinePostProcessing postProcessing;
    public Slider focusMeter;

    public Image baseReticle, movementReticle;

    private UnityEngine.Rendering.PostProcessing.DepthOfField dof;

    private float focusInput;

    public float movementDamping = 2f, movementRange = 400f, movementSensitivityThreshold = 0.2f;

    [Header("Focus")]
    public float maxFocusRange = 300;
    public float focusSensitivity = 0.5f; 
    
    void Start() {
        dof = postProcessing.m_Profile.GetSetting<UnityEngine.Rendering.PostProcessing.DepthOfField>();
    }

    void Update() {
        if(player.GetLooking() == Vector2.zero) movementReticle.transform.localPosition = Vector3.Lerp(movementReticle.transform.localPosition, Vector3.zero, Time.deltaTime * movementDamping);

        dof.focusDistance.value = Mathf.Clamp(dof.focusDistance.value + focusInput * focusSensitivity, 0, maxFocusRange);

        focusMeter.value = dof.focusDistance.value / maxFocusRange;
    }

    public void SynchLook() {
        var look = player.GetLooking();
        if(look == Vector2.zero || look.magnitude < movementSensitivityThreshold) return;

        movementReticle.transform.localPosition = new Vector3(look.x, look.y, 0) * -movementRange;
    }

    public void ChangeFocus(float i) {
        focusInput = i;
    }
}
