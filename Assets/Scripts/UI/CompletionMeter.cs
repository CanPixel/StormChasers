using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CompletionMeter : MonoBehaviour {
    public Text label, percentage;
    public Image completionFill;
    public UIBob uIBob;

    public float hydrantCaseValue = 0.5f;
    public float bitBuildingCaseValue = 0.5f;

    private float fillTarget = 0;

    private float basePercentageScale;

    void Start() {
        gameObject.SetActive(false);
        basePercentageScale = percentage.transform.localScale.x;
        percentage.gameObject.SetActive(false);
    }

    void Update() {
        completionFill.fillAmount = Mathf.Lerp(completionFill.fillAmount, fillTarget, Time.deltaTime * 7f);
        percentage.transform.localScale = Vector3.one * Mathf.Lerp(percentage.transform.localScale.x, basePercentageScale, Time.deltaTime * 8f);
        percentage.text = ((int)(fillTarget * 100f)).ToString() + "%";
    }

    public void StartCompletionMeter(string label) {
        fillTarget = 0;
        this.label.text = label;
        gameObject.SetActive(true);
        uIBob.Bob();
        percentage.gameObject.SetActive(true);
        percentage.transform.localScale = Vector3.zero;
    }

    public void HideCompletion() {
        gameObject.SetActive(false);
    }

    public void SetCompletion(float i) {
        fillTarget = i;
        fillTarget = Mathf.Clamp01(fillTarget);
    }

    public void AddCompletion(float i) {
        fillTarget += i;
        fillTarget = Mathf.Clamp01(fillTarget);
    }


    private bool hydrant = false, bitBuilding = false;
    public void AddHydrant() {
        hydrant = true;
        AddCompletion(hydrantCaseValue);
    }

    public void AddBittenBuilding() {
        bitBuilding = true;
        AddCompletion(bitBuildingCaseValue);
    }
}
