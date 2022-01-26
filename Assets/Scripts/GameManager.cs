using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Cinemachine;
using System.Linq;

public class GameManager : MonoBehaviour {
    public int countdownTime = 60;
    private float countdownTimer;
    public int topX = 3;
    public Top3Billboards billboards;
    public CameraControl camControl;
    public GameObject[] destroyAfterEnd;
    public Cinemachine.CinemachineInputProvider input;
    public Cinemachine.CinemachineVirtualCamera thirdPersonLook;
    private Cinemachine.CinemachineOrbitalTransposer orbitalTransposer;
    private Cinemachine.CinemachineComposer composer;
    public UnityEvent afterCountdownDone;

    public float followOffsetSpeed = 2f;
    public Vector3 followOffset = new Vector3(0, 0, -2), aimAngle = new Vector3(0, 0.8f, 0);

    private List<CameraControl.Screenshot> screenshots;

    private bool done = false;

    [Header("References")]
    public Text countdown;
    public GameObject timerObj;
    public Slider progress;
    public GameObject replay;
    public Gradient coloring;

    void Start() {
        orbitalTransposer = thirdPersonLook.GetCinemachineComponent<Cinemachine.CinemachineOrbitalTransposer>();
        composer = thirdPersonLook.GetCinemachineComponent<Cinemachine.CinemachineComposer>();
        countdownTimer = countdownTime + 1;
        timerObj.transform.localScale = Vector3.zero;
        progress.maxValue = countdownTime;
        replay.SetActive(false);
    }

    void Update() {
        if(done) {
            orbitalTransposer.m_FollowOffset = Vector3.Lerp(orbitalTransposer.m_FollowOffset, followOffset, Time.deltaTime * followOffsetSpeed);
            composer.m_TrackedObjectOffset = Vector3.Lerp(composer.m_TrackedObjectOffset, aimAngle, Time.deltaTime * followOffsetSpeed);
            return;
        }

        if(timerObj != null) timerObj.transform.localScale = Vector3.Lerp(timerObj.transform.localScale, Vector3.one * (Mathf.Sin(Time.time * 2f) * 0.1f + 1), Time.deltaTime * 6.5f);

        if(countdownTimer > 0) countdownTimer = Mathf.Clamp(countdownTimer - Time.deltaTime, 0, countdownTime);
        else if(!done) {
            afterCountdownDone.Invoke();
            EndGame();
            done = true;
        }

        float minutes = Mathf.FloorToInt(countdownTimer / 60);
        float seconds = Mathf.FloorToInt(countdownTimer % 60);
        countdown.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        var val = (countdownTime - (int)countdownTimer);
        countdown.color = coloring.Evaluate(1f - (countdownTimer / countdownTime));
        progress.value = countdownTimer;
    }

    protected void EndGame() {
        screenshots = new List<CameraControl.Screenshot>(camControl.screenshots);

        thirdPersonLook.Follow = thirdPersonLook.LookAt = billboards.focalPoint.transform;
        billboards.SubmitPlayerResults(camControl, GetTopXScoringScreenshots(topX), camControl.GetTotalScore());  
        replay.SetActive(true);
        foreach(var i in destroyAfterEnd) Destroy(i);
        Destroy(input);
    }

    public CameraControl.Screenshot[] GetTopXScoringScreenshots(int X) {
        return screenshots.OrderBy(x => x.score).Take(X).Reverse().ToArray();
    }
}
