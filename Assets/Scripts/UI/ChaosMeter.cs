using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChaosMeter : MonoBehaviour {
    public GameObject meter;
    public Image slider;
    public Text showText, percentage;
    private Gradient gradient;

    private float target, current;

    public int totalSens;
    
    void Update() {
        if(gradient == null) return;

        slider.fillAmount = Mathf.Lerp(slider.fillAmount, target, Time.deltaTime * 3f);
        showText.text = "Sensation: <color='#" + ColorUtility.ToHtmlStringRGB(gradient.Evaluate(totalSens / 100f)) + "'>" + totalSens.ToString() + "</color>";
        
        current = Mathf.Lerp(current, target, Time.deltaTime * 2f);
        percentage.text = current.ToString("0.0");
    }

    public void CalculateChaos(CameraControl.Screenshot screen, Gradient scoring, int totalSensation) {
        gradient = scoring;
        current = slider.fillAmount = 0;
        totalSens = totalSensation;
        target = totalSensation;
    }
}
