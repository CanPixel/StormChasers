using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class RatingSystem : MonoBehaviour {
    public PictureQualifier[] pictureQualifiers;
    private Dictionary<Criteria, PictureQualifier> qualifierByCriteria = new Dictionary<Criteria, PictureQualifier>(); 

    [SerializeField] private CameraControl camControl;
    [SerializeField] private VerticalLayoutGroup layoutGroup; 
    [SerializeField] private Transform splashParent;
    [SerializeField] private GameObject splashPrefab, crosshair, crosshairAnti, crosshairOmcirkel;
    [SerializeField] private Image fadeScreen;
    private float baseFadeAlpha;
    [SerializeField] private Text polaroidTitle, polaroidSkip;
    [SerializeField] private Image polaroidUI, polaroidScreenshot;
    [SerializeField] private Vector2 polaroidInterval = new Vector2(12, 14);
    [SerializeField] private float polaroidBottomPos = 0;
    private float polaroidTopPos;
    [SerializeField] private InputActionReference skipBinding;

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

    public List<SplashElement> ratings = new List<SplashElement>();
    private int splashIndex = 0;

    //private float skipDelay = 0;

    [System.Serializable]
    public enum Criteria {
        Center_Frame, Object_Sharpness, Motion_Stability
    }
    public Dictionary<string, float> scoreUnits = new Dictionary<string, float>();

    [HideInInspector] public bool ready = true;

    private float screenshotTimer = 0;
    private int totalScore;

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
        
        polaroidTopPos = polaroidUI.transform.position.y;
        polaroidUI.transform.position = new Vector3(polaroidUI.transform.position.x, polaroidBottomPos, polaroidUI.transform.position.z);

        int ind = 0;
        foreach(var i in pictureQualifiers) {
            qualifierByCriteria.Add(i.ratingName, i);
            scoreUnits.Add(i.ratingName.ToString().Replace('_', ' ').ToLower().Trim(), 0);
            ind++;
        } 
    }

    void Update() {
        polaroidSkip.text = ("<color='#ff0000'>" + skipBinding.action.GetBindingDisplayString() + "</color> to Skip").ToUpper();
        if(camControl.camSystem.shoot >= 0.7f && screenshotTimer > 1f) {
            Skip();
            screenshotTimer = polaroidInterval.x;
        }

        SetFade(Mathf.Lerp(fadeScreen.color.a, (HasTakenPicture() ? baseFadeAlpha : 0), Time.unscaledDeltaTime * 4f));

        if(HasTakenPicture()) {
            screenshotTimer += Time.unscaledDeltaTime * 2f;

            if(screenshotTimer > polaroidInterval.y) screenshotTimer = 0;

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

    protected void CycleSplash() {
        splashIndex++;
        Spawn(splashIndex);
    }

    private void ResetSplash() {
        ready = true;
        splashIndex = 0;
    }

    public void VisualizeScore(CameraControl.PictureScore pic, CameraControl.Screenshot screen, PhotoItem photoItem) {
        if(!ready || photoItem == null) return;
        ready = false;
        splashIndex = 0;
        Spawn(splashIndex);

        int index = 0;
        totalScore = 0;
        foreach(var i in System.Enum.GetNames(typeof(Criteria))) {
            //ScoreElement scoreElement = scoreFormat;
            //scoreElement.pos = scoreFormat.pos + new Vector3(0, index, 0);
            var str = i.Replace('_', ' ').ToLower().Trim();
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
            //scoreElement.text = i + ": " + (int)scoreUnits[str];
            totalScore += (int)scoreUnits[str];
            index++;
        }
        totalScore /= index;
        screen.score = totalScore;
    }

    private void Spawn(int index) {
        if(index < 0) {
            ResetSplash();
            return;
        }
        var ob = splashes[index];
        var spl = SpawnSplash(ob.text, ob.pos, ob.rot, ob.scale, ob.duration, ob.alignment);
        if(ob.rating.ToLower().Trim().Length > 1 && scoreUnits.ContainsKey(ob.rating.ToLower().Trim())) {
            spl.text.text = ob.text + ": <color='#" + ColorUtility.ToHtmlStringRGB(scoreGradient.Evaluate(scoreUnits[ob.rating.ToLower().Trim()] / 100f)) + "'>" + scoreUnits[ob.rating.ToLower().Trim()] + "</color>";
        }
        spl.text.font = ob.font;
        spl.text.alignment = ob.alignment;
        spl.text.color = ob.color;
        CycleSplash();
    }
    private Splash SpawnSplash(string text, Vector3 pos, float rot, float scale, float duration, TextAnchor align) {
        var obj = Instantiate(splashPrefab);
        var spl = obj.GetComponent<Splash>();
        spl.targetScale = Vector3.one * 1.1f;
        obj.transform.SetParent(splashParent);
        obj.transform.localScale = Vector3.zero;
        obj.transform.localPosition = pos;
        obj.transform.localEulerAngles = new Vector3(0, 0, rot);
        spl.targetScale = Vector3.one * scale;
        spl.text.text = text;
        spl.text.alignment = align;
        spl.duration = duration;
        return spl;
    }

    public void Skip() {
        splashIndex = 100;
    }

    public bool HasTakenPicture() {
        return screenshotTimer > 0;
    }
}