using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Crosshair : MonoBehaviour {
    public Animator animator;
    public Image crosshair;

    [HideInInspector] public PhotoBase target;

    public Text label, sensation;

    public bool updateCrosshairOnDestroy = false;

    public float fillSpeed = 1;
    public float duration = 3;
    public bool destroyAfterDuration = false;
    private float time = 0;
    public bool fillOverTime = false;

    void Start() {
        if(fillOverTime) crosshair.fillAmount = 0;
    }

    void LateUpdate() {
        time += Time.unscaledDeltaTime;

        if(destroyAfterDuration && time > duration) Destroy(gameObject);
        if(fillOverTime && crosshair.fillAmount < 1) crosshair.fillAmount += Time.unscaledDeltaTime * fillSpeed; 
    }

    public void SetAlpha(float a) {
        crosshair.color = new Color(crosshair.color.r, crosshair.color.g, crosshair.color.b, a);
    }

    void OnDestroy() {
        if(updateCrosshairOnDestroy) LockOnSystem.RemoveScreenTarget(this);
    }

    public void EnableRender(bool i) {
        crosshair.enabled = sensation.enabled = label.enabled = i;
    }
}
