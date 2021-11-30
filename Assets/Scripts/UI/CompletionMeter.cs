using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CompletionMeter : MonoBehaviour {
    public Text label, percentage;
    public Image completionFill;
    public UIBob uIBob;

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

    public void SetCompletion(float i) {
        fillTarget = i;
        fillTarget = Mathf.Clamp01(fillTarget);
    }

    public void AddCompletion(float i) {
        fillTarget += i;
        fillTarget = Mathf.Clamp01(fillTarget);
    }
}
