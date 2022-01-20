using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChaosMeter : MonoBehaviour {
    public GameObject meter;
    public Image slider;
    public Text showText, percentage;
    public ParticleSystem sparks;
    public Image marker;
    public Vector2 markerRange = new Vector2(855, 540);
    private Gradient gradient;

    private float target, current;

    public int totalSens;
    
    void Update() {
        if(gradient == null) return;

        slider.fillAmount = Mathf.Lerp(slider.fillAmount, target / 100f, Time.deltaTime * 3f);
        slider.color = Color.Lerp(slider.color, gradient.Evaluate(current / 100f), Time.deltaTime * 3f);
        showText.text = "Sensation: <color='#" + ColorUtility.ToHtmlStringRGB(gradient.Evaluate(totalSens / 100f)) + "'>" + totalSens.ToString() + "</color>";
        
        current = Mathf.Lerp(current, target, Time.deltaTime * 2f);
        percentage.text = current.ToString("0.0");
        percentage.color = slider.color;
        marker.color = new Color(1 - slider.color.r, 1 - slider.color.g, 1 - slider.color.b, 1);
        marker.transform.localPosition = new Vector3(slider.fillAmount * (markerRange.x) - markerRange.y, marker.transform.localPosition.y, 0);

        //if((int)current == (int)target) sparks.Stop();
    }

    public void CalculateChaos(CameraControl.Screenshot screen, Gradient scoring, int totalSensation) {
        gradient = scoring;
        current = slider.fillAmount = 0;
        totalSens = totalSensation;
        target = totalSensation;
        sparks.Play();
    }
}
