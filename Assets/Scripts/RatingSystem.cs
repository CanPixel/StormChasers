using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class RatingSystem : MonoBehaviour {
    public PictureQualifier[] pictureQualifiers;
    private Dictionary<Criteria, PictureQualifier> qualifierByCriteria = new Dictionary<Criteria, PictureQualifier>(); 

    [SerializeField] private Vector2 polaroidInterval = new Vector2(12, 14);
    [SerializeField] private float polaroidBottomPos = 0;
    private float polaroidTopPos;

    [System.Serializable]
    public class PictureQualifier {
        public string name = "PictureQualifier";
        public Criteria ratingName;
        public Text criteriaTxt, scoreTxt;
        public int score {
            get; private set;
        }

        public void SetScore(int score) {
            this.score = score;
            scoreTxt.text = score.ToString();
        }
    }

    public Gradient scoreGradient;

    [System.Serializable]
    public class Rating {
        public string name;
        [Range(0, 100)]
        public int range;
    }
    public Rating[] ratings;

    [System.Serializable]
    public enum Criteria {
        Center_Frame, Object_Sharpness, Motion_Stability
    }
    public Dictionary<string, float> scoreUnits = new Dictionary<string, float>();

    [HideInInspector] public bool ready = true;
    [Space(10)]
    [SerializeField] private CameraControl camControl;
    [SerializeField] private VerticalLayoutGroup layoutGroup; 
    private float baseSpacing;

    [SerializeField] private Transform splashParent;
    [SerializeField] private GameObject splashPrefab, crosshair, crosshairAnti, crosshairOmcirkel;
    [SerializeField] private Image fadeScreen;
    private float baseFadeAlpha;
    [SerializeField] private Text polaroidTitle, polaroidSkip, totalScoreBase, totalScoreValue;
    [SerializeField] private Image polaroidUI, polaroidScreenshot;
    [SerializeField] private InputActionReference skipBinding;

    private float screenshotTimer = 0;
    private int totalScore;
    private bool crosshairRestObjects;
    private CameraControl.PictureScore mainScore;

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
        scoreUnits.Clear();
        baseFadeAlpha = fadeScreen.color.a;
        SetFade(0);
        baseSpacing = layoutGroup.spacing;
        
        polaroidTopPos = polaroidUI.transform.position.y;
        polaroidUI.transform.position = new Vector3(polaroidUI.transform.position.x, polaroidBottomPos, polaroidUI.transform.position.z);
        Reset();

        foreach(var i in pictureQualifiers) {
            qualifierByCriteria.Add(i.ratingName, i);
            scoreUnits.Add(i.ratingName.ToString().Replace('_', ' ').ToLower().Trim(), 0);
        } 
    }

    void Update() {
        polaroidSkip.text = ("<color='#ff0000'>" + skipBinding.action.GetBindingDisplayString() + "</color> to Skip").ToUpper();
        if(camControl.camSystem.shoot >= 0.7f && screenshotTimer > 1f) {
            Skip();
            screenshotTimer = polaroidInterval.x;
        }

        if(polaroidScreenshot.fillAmount < 1) polaroidScreenshot.fillAmount += Time.unscaledDeltaTime * 2f;

        SetFade(Mathf.Lerp(fadeScreen.color.a, (HasTakenPicture() && !IsFading() ? baseFadeAlpha : 0), Time.unscaledDeltaTime * 6.5f));

        if(HasTakenPicture()) {
            screenshotTimer += Time.unscaledDeltaTime * 2f;

            for(int i = 0; i < pictureQualifiers.Length; i++) {
                if(screenshotTimer < polaroidInterval.x / 4f) {
                    pictureQualifiers[i].criteriaTxt.transform.localScale = Vector3.Lerp(pictureQualifiers[i].criteriaTxt.transform.localScale, Vector3.one, screenshotTimer / 10f);
                }
                if(screenshotTimer > polaroidInterval.x / 4f + (i / 2f)) pictureQualifiers[i].scoreTxt.transform.localScale = Vector3.Lerp(pictureQualifiers[i].scoreTxt.transform.localScale, Vector3.one, screenshotTimer / 45f);
            } 
            if(screenshotTimer > polaroidInterval.x / 8f) layoutGroup.spacing = Mathf.Lerp(layoutGroup.spacing, baseSpacing, screenshotTimer / 18f);


            if(screenshotTimer > polaroidInterval.x / 2f) totalScoreBase.transform.localScale = Vector3.Lerp(totalScoreBase.transform.localScale, Vector3.one, screenshotTimer / 10f);
            if(screenshotTimer > polaroidInterval.x / 2f + 1) totalScoreValue.transform.localScale = Vector3.Lerp(totalScoreValue.transform.localScale, Vector3.one, screenshotTimer / 10f);
            ///////
            
            if(screenshotTimer > polaroidInterval.x / 2f && !crosshairRestObjects) {
                crosshairRestObjects = true;
                foreach(var sub in mainScore.subScore) {
                    SpawnCrosshair(sub.item);
                    var s = Instantiate(splashPrefab);
                    s.transform.SetParent(polaroidScreenshot.transform);
                    s.transform.localPosition = new Vector3(0, 100, 0);
                    s.transform.localScale = Vector3.one * 0.1f;
                    s.GetComponent<Splash>().text.text = "Center Frame: " + sub.centerFrame;
                }
            }

            if(screenshotTimer > polaroidInterval.y) Reset();

            if(screenshotTimer > polaroidInterval.x) polaroidUI.transform.position = new Vector3(polaroidUI.transform.position.x, Mathf.Lerp(polaroidUI.transform.position.y, polaroidBottomPos, Time.unscaledDeltaTime * 5f), polaroidUI.transform.position.z);
            else polaroidUI.transform.position = new Vector3(polaroidUI.transform.position.x, Mathf.Lerp(polaroidUI.transform.position.y, polaroidTopPos, Time.unscaledDeltaTime * 7f), polaroidUI.transform.position.z);
        }
    }

    protected void SetFade(float i) {
        fadeScreen.color = new Color(fadeScreen.color.r, fadeScreen.color.g, fadeScreen.color.b, i);
    }

    public void ResetScreenshot() {
        screenshotTimer = 0.1f;
        polaroidUI.transform.position = new Vector3(polaroidUI.transform.position.x, polaroidBottomPos, polaroidUI.transform.position.z);
    }

    public void SetPolaroidTitle(string txt) {
        polaroidTitle.text = txt;
    }
    public void SetPolaroidSprite(Sprite spr) {
        polaroidScreenshot.sprite = spr;
    }

    private void Reset() {
        ready = true;
        mainScore = null;
        crosshairRestObjects = false;
        screenshotTimer = 0;
        totalScore = 0;
        polaroidUI.transform.position = new Vector3(polaroidUI.transform.position.x, polaroidBottomPos, polaroidUI.transform.position.z);
        layoutGroup.spacing = -170;
        foreach(var i in pictureQualifiers) i.criteriaTxt.transform.localScale = i.scoreTxt.transform.localScale = Vector3.zero;
        totalScoreBase.transform.localScale = totalScoreValue.transform.localScale = Vector3.zero;
    }

    protected void SpawnCrosshair(PhotoItem item) {
        if(camControl.lockOnSystem.GetScreenCrosshair(item) == null) return;
        var spl = Instantiate(crosshairOmcirkel);
        spl.transform.SetParent(polaroidScreenshot.transform);
        var screenPos = camControl.lockOnSystem.GetScreenCrosshair(item).transform.localPosition;
        screenPos.z = 0;
        screenPos /= 3f;
        spl.transform.localPosition = screenPos;
        spl.transform.localScale = Vector3.one * 0.5f;
    }

    public void VisualizeScore(CameraControl.PictureScore pic, CameraControl.Screenshot screen) {
        if(!ready || pic.item == null) return;
        Reset();

        polaroidScreenshot.fillAmount = 0;

        ready = false;

        mainScore = pic;
        SpawnCrosshair(pic.item);
        crosshairRestObjects = false;

        int index = 0;
        foreach(KeyValuePair<Criteria, PictureQualifier> i in qualifierByCriteria) {
            var str = i.Key.ToString().Replace('_', ' ').ToLower().Trim();
            switch(str) {
                case "center frame":
                    scoreUnits[str] = pic.centerFrame;
                    break;
                case "object sharpness":
                    scoreUnits[str] = pic.objectSharpness;
                    break;
                case "motion stability":
                    scoreUnits[str] = pic.motion;
                    break;
                default: break;
            }
            qualifierByCriteria[i.Key].scoreTxt.text = "<color='#" + ColorUtility.ToHtmlStringRGB(scoreGradient.Evaluate((int)scoreUnits[str] / 100f)) + "'>" + ((int)scoreUnits[str]).ToString() + "</color>";
            totalScore += (int)scoreUnits[str];
            index++;
        }
        totalScore /= index;
        screen.score = (int)totalScore;
        totalScoreValue.text = "<color='#" + ColorUtility.ToHtmlStringRGB(scoreGradient.Evaluate((int)totalScore / 100f)) + "'>" + screen.score.ToString() + "</color>";
    }

    public void Skip() {
        screenshotTimer = polaroidInterval.x;
        Reset();
    }

    public bool HasTakenPicture() {
        return screenshotTimer > 0;
    }

    public bool IsFading() {
        return screenshotTimer > polaroidInterval.x;
    }
}