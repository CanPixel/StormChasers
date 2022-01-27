using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Cinemachine;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public int countdownTime = 60;
    [HideInInspector] public float countdownTimer;


    public int topX = 3;
    public Top3Billboards billboards;

    public CameraControl camControl;
    public GameObject[] destroyAfterEnd;
    public Cinemachine.CinemachineVirtualCamera endCamera;
    public UnityEvent afterCountdownDone;

    public float followOffsetSpeed = 2f;
    public Vector3 followOffset = new Vector3(0, 0, -2), aimAngle = new Vector3(0, 0.8f, 0);

    public List<CameraControl.Screenshot> screenshots;

    private bool done = false;

    [Header("SpawnEvents")]
    public GameObject tornado;
    public float tornadoSpawnDuration;
    public float tornadoEatBuildingDuration;
    TornadoScript tornadoScript;
    private float eventSpawnTimer;

    [Header("References")]
    public Text countdown;
    public GameObject timerObj;
    public Slider progress;
    public GameObject replay;
    public Gradient coloring;

    void Start()
    {
        endCamera.Priority = -1;
        countdownTimer = countdownTime + 1;
        timerObj.transform.localScale = Vector3.zero;
        progress.maxValue = countdownTime;
        replay.SetActive(false);
        tornadoScript = tornado.GetComponent<TornadoScript>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();

        if (done) return;


        if (timerObj != null) timerObj.transform.localScale = Vector3.Lerp(timerObj.transform.localScale, Vector3.one * (Mathf.Sin(Time.time * 2f) * 0.1f + 1), Time.deltaTime * 6.5f);

        if (countdownTimer > 0) countdownTimer = Mathf.Clamp(countdownTimer - Time.deltaTime, 0, countdownTime);
        else if (!done)
        {
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

        SpawnStuff();
    }

    protected void EndGame()
    {
        screenshots = new List<CameraControl.Screenshot>(camControl.screenshots);
        endCamera.Priority = 100;
        billboards.SubmitPlayerResults(camControl, GetTopXScoringScreenshots(topX), camControl.GetTotalScore());
        replay.SetActive(true);
        foreach (var i in destroyAfterEnd) Destroy(i);
    }

    void SpawnStuff()
    {
        eventSpawnTimer += Time.deltaTime;
        if (eventSpawnTimer > tornadoSpawnDuration && !tornado.activeInHierarchy)
        {
            //Debug.Log("SpawnTornado");
            eventSpawnTimer = 0f;
            tornado.SetActive(true);
        }

        if (eventSpawnTimer > tornadoEatBuildingDuration && !tornadoScript.canEatBuilding)
        {
            tornadoScript.canEatBuilding = true;
            tornado.transform.localScale *= 1.5f;
        }
    }

    public CameraControl.Screenshot[] GetTopXScoringScreenshots(int X)
    {
        return screenshots.OrderBy(x => x.score).Take(X).Reverse().ToArray();
    }

    void OnApplicationQuit()
    {
        foreach (var i in camControl.screenshots) Destroy(i.image);
        camControl.screenshots.Clear();
        if (screenshots == null) return;
        foreach (var i in screenshots) Destroy(i.image);
        screenshots.Clear();
    }
}
