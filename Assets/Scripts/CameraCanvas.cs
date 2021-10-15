using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine.PostFX;
using UnityEngine.Rendering.PostProcessing;

public class CameraCanvas : MonoBehaviour {
    public LayerMask raycastLayerMask;
    public float raycastRadius = 3;
    public float maxDistance = 300;

    private UnityEngine.Rendering.PostProcessing.DepthOfField dof;
    private float focusInput;

    [Header("Motion Blur Reticle")]
    public float movementDamping = 2f;
    public float movementRange = 400f, movementSensitivityThreshold = 0.2f;

    [Header("Focus")]
    public float minFocusRange = 10;
    public float maxFocusRange = 300;
    public float focusSensitivity = 0.5f;
    public AnimationCurve apertureSensitivity; 
    public AnimationCurve physicalDistanceFocusModifier;
    public float apertureFactor = 1f;
    public float physicalDistanceFactor = 0.5f;

    [Space(10)]
    public LockOnSystem lockOnSystem;
    public Camera cam;
    public CameraControl cameraControl;
    public CarMovement player;
    public CinemachinePostProcessing postProcessing;
    public Slider focusMeter;
    public Text highlightedObjectText;
    public Image baseReticle, movementReticle;
    public Outline focusMeterOutline;
    public Outline focusMeterImg;

    void Start() {
        dof = postProcessing.m_Profile.GetSetting<UnityEngine.Rendering.PostProcessing.DepthOfField>();
    }

    void Update() {
        if(player.GetLooking() == Vector2.zero) movementReticle.transform.localPosition = Vector3.Lerp(movementReticle.transform.localPosition, Vector3.zero, Time.deltaTime * movementDamping);

        dof.focusDistance.value = Mathf.Clamp(dof.focusDistance.value + focusInput * focusSensitivity, minFocusRange, maxFocusRange);
        focusMeter.value = (dof.focusDistance.value / maxFocusRange);
        var ding = apertureSensitivity.Evaluate(focusMeter.value) * apertureFactor;
        dof.aperture.value = ding;
    }

    public PhotoItem RaycastFromReticle(Transform trans) {
        PhotoItem photoItem = null;
        RaycastHit hit;
        if(Physics.SphereCast(cam.ScreenPointToRay(trans.position), raycastRadius, out hit, maxDistance, raycastLayerMask)) {
            var ph = hit.transform.gameObject.GetComponent<PhotoItem>();
            if(ph != null) photoItem = ph;
        }
        return photoItem;
    }

    public void SynchLook() {
        if(!cameraControl.cinemachineBrain.IsLive(cameraControl.firstPersonLook) || cameraControl.cinemachineBrain.IsBlending) return;
        var look = player.GetLooking();
        if(look == Vector2.zero || look.magnitude < movementSensitivityThreshold) return;
        movementReticle.transform.localPosition = new Vector3(look.x, look.y, 0) * -movementRange;
    }

    public void ChangeFocus(float i) {
        focusInput = i;
    }

    public int GetFocusValue() {
        return (int)((dof.focusDistance.value / maxFocusRange) * 100f);
    }

    public float GetPhysicalDistance(PhotoItem i) {
        var dist = Mathf.Clamp(Mathf.Abs(Vector3.Distance(player.transform.position, (i != null) ? i.transform.position : player.transform.position)), 1, maxDistance);
        return (int)Mathf.Clamp(dist * physicalDistanceFactor, 0, 100);
    }

    public void SetFocusResponse(float focus) {
        cameraControl.carMovement.HapticFeedback(0f, focus, 0.05f);
        focusMeterOutline.effectDistance = Vector2.one * ((1f - focus) * 2 - 1);
        focusMeterOutline.transform.localScale = Vector3.one * ((focus + 1) / 2f + 0.25f);
        focusMeterImg.effectDistance = Vector2.one * (1f - focus) * 3;
    }
}