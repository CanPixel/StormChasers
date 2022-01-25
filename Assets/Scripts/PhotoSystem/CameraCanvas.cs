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

    [Header("Focus")]
    private float focalLength = 110;
    private float minFocusRange = 3, maxFocusRange = 65;
    private float focusSensitivity = 0.7f;
    private float sensitivityChangeSensitivity;
    public AnimationCurve apertureSensitivity;
    public AnimationCurve sweepingRange; 
    private float sweepFactor = 1f, sweepBaseValue = 80;
    private float apertureFactor = 2.5f;
    private float focusSensitivityHigh = 1.5f, focusSensitivityLow = 0.4f;

    [Space(10)]
    public Camera cam;
    public CameraControl cameraControl;
    public CarMovement player;
    public PostProcessVolume postProcessVolume;
    public Slider focusMeter;
    public Text highlightedObjectText;
    public Image baseReticle;
    public Outline focusMeterOutline;
    public Outline focusMeterImg;

    private float motion;

    void Start() {
        sensitivityChangeSensitivity = focusSensitivityHigh;
        ReloadFX();
    }

    void Update() {
        dof.focusDistance.value = Mathf.Clamp(dof.focusDistance.value + focusInput * focusSensitivity * sensitivityValue, minFocusRange, maxFocusRange);
        focusMeter.value = (dof.focusDistance.value / maxFocusRange);
        
        var ding = apertureSensitivity.Evaluate(focusMeter.value) * apertureFactor;
        dof.aperture.value = ding;

        var sweep = sweepingRange.Evaluate(1f - focusMeter.value) * sweepFactor;
        dof.focalLength.value = sweep * sweepBaseValue;

        sensitivityValue = sensitivityChangeSensitivity;
        focusMeterImg.effectDistance = Vector2.Lerp(focusMeterImg.effectDistance, (Vector2.one * Mathf.Clamp(focusSensitivityHigh - sensitivityValue, 1, -2) * 2), Time.unscaledDeltaTime * 5f);
    }

    public void ReloadFX() {
        dof = postProcessVolume.sharedProfile.GetSetting<UnityEngine.Rendering.PostProcessing.DepthOfField>();
        var focusDist = dof.focusDistance.value;
        var focalLn = dof.focalLength.value;
        dof = postProcessVolume.sharedProfile.GetSetting<UnityEngine.Rendering.PostProcessing.DepthOfField>();
        var focusAperture = dof.aperture.value;
        
        dof.focusDistance.value = focusMeter.value = focusDist;
        dof.aperture.value = focusAperture;
        dof.focalLength.value = focalLn;
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