using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
    public int countdownTime = 60;
    private float countdownTimer;
    public UnityEvent afterCountdownDone;

    private bool done = false;

    [Header("References")]
    public Text countdown;
    public GameObject timerObj;
    public Slider progress;
    public Gradient coloring;

    void Start() {
        countdownTimer = countdownTime + 1;
        timerObj.transform.localScale = Vector3.zero;
        progress.maxValue = countdownTime;
    }

    void Update() {
        timerObj.transform.localScale = Vector3.Lerp(timerObj.transform.localScale, Vector3.one * (Mathf.Sin(Time.time * 2f) * 0.1f + 1), Time.deltaTime * 6.5f);

        if(countdownTimer > 0) countdownTimer = Mathf.Clamp(countdownTimer - Time.deltaTime, 0, countdownTime);
        else if(!done) {
            afterCountdownDone.Invoke();
            done = true;
        }

        float minutes = Mathf.FloorToInt(countdownTimer / 60);
        float seconds = Mathf.FloorToInt(countdownTimer % 60);
        countdown.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        var val = (countdownTime - (int)countdownTimer);
        countdown.color = coloring.Evaluate(1f - (countdownTimer / countdownTime));
        progress.value = countdownTimer;
    }
}
