using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class RatingSystem : MonoBehaviour {
    [SerializeField] private float polaroidInterval = 10;
    [SerializeField] private float polaroidBottomPos = 0;
    private float polaroidTopPos, postPictureDelay = 0;

    public float polaroidScale = 0.5f;

    public Gradient scoreGradient;

    [Space(10)]
    [SerializeField] private CameraControl camControl;

    [SerializeField] private GameObject splashPrefab, crosshair, crosshairAnti, crosshairOmcirkel;
    [SerializeField] private Text polaroidTitle, polaroidScore;
    [SerializeField] private Image polaroidUI, polaroidScreenshot;
    public ChaosMeter chaosMeter;

    private float screenshotTimer = 0;

    private List<GameObject> crosshairs = new List<GameObject>();

    [System.Serializable]
    public class CrosshairObject {
        public Vector2 screenPos;
        public PhotoBase item;

        public CrosshairObject(PhotoBase item, Vector2 pos) {
            this.item = item;
            this.screenPos = pos;
        }
    }

    [System.Serializable]
    public class SplashElement {
        [HideInInspector] public Splash splash;
        public Vector3 pos;
        public float rot, scale = 1; 
        public float duration = 2;
        public Font font;
        public Color color;
    }

    void Start() {
        polaroidUI.gameObject.SetActive(true);
        
        polaroidTopPos = polaroidUI.transform.position.y;
        polaroidUI.transform.position = new Vector3(polaroidUI.transform.position.x, polaroidBottomPos, polaroidUI.transform.position.z);
        Reset();
    }

    void Update() {
        postPictureDelay += Time.unscaledDeltaTime;

        if(polaroidScreenshot.fillAmount < 1) polaroidScreenshot.fillAmount += Time.unscaledDeltaTime * 1.5f;

        if(HasTakenPicture()) {
            screenshotTimer += Time.unscaledDeltaTime * 2f;

            if(screenshotTimer > polaroidInterval) {
                polaroidUI.transform.position = new Vector3(polaroidUI.transform.position.x, Mathf.Lerp(polaroidUI.transform.position.y, polaroidBottomPos, Time.unscaledDeltaTime * 5f), polaroidUI.transform.position.z);
                polaroidUI.transform.localScale = Vector3.Lerp(polaroidUI.transform.localScale, Vector3.zero, Time.unscaledDeltaTime * 4f);
            }
            else polaroidUI.transform.position = new Vector3(polaroidUI.transform.position.x, Mathf.Lerp(polaroidUI.transform.position.y, polaroidTopPos, Time.unscaledDeltaTime * 7f), polaroidUI.transform.position.z);
        }
    }

    public void ResetScreenshot() {
        screenshotTimer = 0.1f;
        polaroidUI.transform.position = new Vector3(polaroidUI.transform.position.x, polaroidBottomPos, polaroidUI.transform.position.z);
        polaroidUI.transform.localScale = Vector3.one * polaroidScale;
    }

    public void SetPolaroidTitle(string txt) {
        polaroidTitle.text = txt;
    }
    public void SetPolaroidSprite(Sprite spr) {
        polaroidScreenshot.sprite = spr;
    }

    private void Reset() {
        screenshotTimer = 0;
        polaroidUI.transform.position = new Vector3(polaroidUI.transform.position.x, polaroidBottomPos, polaroidUI.transform.position.z);
        foreach(var i in crosshairs) Destroy(i);
        crosshairs.Clear();
        postPictureDelay = 0;
    }

    protected void SpawnCrosshair(PhotoBase item, bool isMainSubject = true) {
        if(camControl.lockOnSystem.GetScreenCrosshair(item) == null) return;
        SpawnCrosshair(new CrosshairObject(item, camControl.lockOnSystem.GetScreenCrosshair(item).transform.localPosition), isMainSubject);
    }

    protected void SpawnCrosshair(CrosshairObject obj, bool isMainSubject = true) {
        var spl = Instantiate(crosshairOmcirkel);
        spl.transform.SetParent(polaroidScreenshot.transform);
        if(!isMainSubject) {
            spl.GetComponent<Image>().color = Random.ColorHSV(0, 1, 0.4f, 1f, 0.4f, 1);
            spl.transform.position = obj.screenPos;
        }
        else spl.transform.localPosition = obj.screenPos;
        spl.transform.localScale = Vector3.one * 0.5f * (isMainSubject ? 1.45f : 1);
        crosshairs.Add(spl);
    }

    public void VisualizeScore(CameraControl.Screenshot screen, Gradient scoring, int totalSensation) {
        polaroidScreenshot.fillAmount = 0;
        if(screen.containedObjectTags == null || screen.containedObjectTags.Length < 1) return;
        Reset();
        chaosMeter.CalculateChaos(screen, scoring, totalSensation);
    }

    public bool HasTakenPicture() {
        return screenshotTimer > 0;
    }

    public bool AfterDelay(float time) {
        return postPictureDelay > time;
    }
}