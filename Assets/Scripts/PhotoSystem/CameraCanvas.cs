using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine.PostFX;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.PostProcessing;

public class CameraCanvas : MonoBehaviour {
    public LayerMask raycastLayerMask;
    public float raycastRadius = 3;
    public float maxDistance = 300;

    private UnityEngine.Rendering.PostProcessing.DepthOfField dof;
    private UnityEngine.Rendering.PostProcessing.MotionBlur motionBlur;
    private float focusInput;
    private float sensitivityValue = 1;

    [Header("Motion Blur Reticle")]
    public float movementDamping = 2f;
    public float movementRange = 400f, movementSensitivityThreshold = 0.2f;
    public Vector2 motionBlurRange = new Vector2(180, 360);
    public float motionReticleSensitivity = 1.5f;

    [Header("Focus")]
    public float focalLength = 90;
  //  public float focalLengthDistanceFactor = 5;
    public float minFocusRange = 10;
    public float maxFocusRange = 300;
    public float focusSensitivity = 0.5f;
    public float objectInFocusThreshold = 75;
    private float sensitivityChangeSensitivity;
    public AnimationCurve apertureSensitivity; 
    public float apertureFactor = 1f;
    //public float physicalDistanceFactor = 0.5f;
    public float focusSensitivityHigh = 0.9f, focusSensitivityLow = 0.4f;

    [Space(10)]
    public Camera cam;
    public CameraControl cameraControl;
    public CarMovement player;
    public PostProcessVolume postProcessVolume;
    public Slider focusMeter;
    public Text highlightedObjectText;
    public Image baseReticle, movementReticle;
    public Slider speedToggle;
    public Image speedToggleIcon;
    public Sprite lowSpeedIcon, highSpeedIcon;
    public Outline focusMeterOutline;
    public Outline focusMeterImg;

    private float motion;

    public void ChangeSpeedToggle() {
        if(speedToggle.value == 1) {
            speedToggle.value = 0;
            speedToggleIcon.sprite = lowSpeedIcon;   
            sensitivityChangeSensitivity = focusSensitivityLow;
        }
        else {
            speedToggle.value = 1;
            speedToggleIcon.sprite = highSpeedIcon;
            sensitivityChangeSensitivity = focusSensitivityHigh;
        }
        SoundManager.PlayUnscaledSound("ShaderSwitch", 0.7f);
    }

    void Start() {
        sensitivityChangeSensitivity = focusSensitivityHigh;
        speedToggle.value = 1;
        speedToggleIcon.sprite = highSpeedIcon;
        ReloadFX();
    }

    void Update() {
        dof.focusDistance.value = Mathf.Clamp(dof.focusDistance.value + focusInput * focusSensitivity * sensitivityValue, minFocusRange, maxFocusRange);
        focusMeter.value = (dof.focusDistance.value / maxFocusRange);
        
        var ding = apertureSensitivity.Evaluate(focusMeter.value) * apertureFactor;
        dof.aperture.value = ding;

        sensitivityValue = sensitivityChangeSensitivity;
        focusMeterImg.effectDistance = Vector2.Lerp(focusMeterImg.effectDistance, (Vector2.one * Mathf.Clamp(focusSensitivityHigh - sensitivityValue, 1, -2) * 2), Time.unscaledDeltaTime * 5f);
    }

    public void ReloadFX() {
        dof = postProcessVolume.sharedProfile.GetSetting<UnityEngine.Rendering.PostProcessing.DepthOfField>();
        var focusDist = dof.focusDistance.value;
        var focalLn = dof.focalLength.value;
        dof = postProcessVolume.sharedProfile.GetSetting<UnityEngine.Rendering.PostProcessing.DepthOfField>();
        var focusAperture = dof.aperture.value;
        //motionBlur = postProcessVolume.sharedProfile.GetSetting<UnityEngine.Rendering.PostProcessing.MotionBlur>();
        
        dof.focusDistance.value = focusMeter.value = focusDist;
        dof.aperture.value = focusAperture;
        dof.focalLength.value = focalLn;
    }

    public PhotoBase RaycastFromReticle() {
        PhotoBase photoItem = null;
        RaycastHit hit;
        if(Physics.SphereCast(cam.ScreenPointToRay(baseReticle.transform.position), raycastRadius, out hit, maxDistance, raycastLayerMask)) {
            var ph = hit.transform.gameObject.GetComponent<PhotoItem>();
            if(ph != null) photoItem = ph;
        }
        return photoItem;
    }
    public float RaycastDistance() {
        RaycastHit hit;
        float dist = -1f;
        if(Physics.SphereCast(cam.ScreenPointToRay(baseReticle.transform.position), raycastRadius, out hit, maxDistance, raycastLayerMask)) dist = Vector3.Distance(hit.point, player.transform.position);
        return dist;
    }
    public string RaycastName(Transform trans) {
        RaycastHit hit;
        if(Physics.SphereCast(cam.ScreenPointToRay(trans.position), raycastRadius, out hit, maxDistance, raycastLayerMask)) {
            return hit.transform.gameObject.name;
        }
        return "";
    }

    public float GetMotion() {
        return motion;
    }

    public void ChangeFocus(float i) {
        focusInput = i;
    }

    public int GetFocusValue() {
        if(dof == null) dof = postProcessVolume.sharedProfile.GetSetting<UnityEngine.Rendering.PostProcessing.DepthOfField>();
        return (int)((dof.focusDistance.value / maxFocusRange) * 100f);
    }

    public float GetPhysicalDistance(PhotoBase i) {
        var dist = Mathf.Clamp(Mathf.Abs(Vector3.Distance(player.transform.position, (i != null) ? i.transform.position : player.transform.position)), 1, maxDistance);
        return (int)Mathf.Clamp(dist, 0, 100);
    }

    public void SetFocusResponse(float focus) {
        HapticManager.ManualHaptics(0, focus, 0.05f);
        focusMeterOutline.effectDistance = Vector2.one * (((1f - focus) * 2 - 1));
        focusMeterOutline.transform.localScale = Vector3.one * ((focus + 1) / 2f + 0.25f);
    }
}
