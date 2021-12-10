using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class RatingSystem : MonoBehaviour {
    public PictureQualifier[] pictureQualifiers;
    private Dictionary<Criteria, PictureQualifier> qualifierByCriteria = new Dictionary<Criteria, PictureQualifier>(); 

    [SerializeField] private float polaroidInterval = 10;
    [SerializeField] private float polaroidBottomPos = 0;
    private float polaroidTopPos, postPictureDelay = 0;

    public float polaroidScale = 0.5f;

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
    public enum LetterGrade {
        A, B, C, D, E, F
    }

    [System.Serializable]
    public class Rating {
        public string name;
        public LetterGrade letterGrade;
        [Range(0, 100)]
        public int range;
    }
    public Rating[] ratings;

    [System.Serializable]
    public enum Criteria {
        Center_Frame, Object_Sharpness, Motion_Stability
    }
    public Dictionary<string, float> scoreUnits = new Dictionary<string, float>();

    private bool ready = true;
    [Space(10)]
    [SerializeField] private CameraControl camControl;
   // [SerializeField] private VerticalLayoutGroup layoutGroup; 
    //private float baseSpacing;

    [SerializeField] private Transform splashParent;
    [SerializeField] private GameObject splashPrefab, crosshair, crosshairAnti, crosshairOmcirkel;
    [SerializeField] private Text polaroidTitle;
    //, polaroidSkip, totalScoreBase, totalScoreValue;
    [SerializeField] private Image polaroidUI, polaroidScreenshot;
    [SerializeField] private InputActionReference skipBinding;
    public ChaosMeter chaosMeter;

    private float screenshotTimer = 0;
    private int totalScore;
    //private bool crosshairRestObjects;
    private CameraControl.PictureScore mainScore;

    private List<CrosshairObject> extraItems = new List<CrosshairObject>(); 
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
        scoreUnits.Clear();
        //baseSpacing = layoutGroup.spacing;

        polaroidUI.gameObject.SetActive(true);
        
        polaroidTopPos = polaroidUI.transform.position.y;
        polaroidUI.transform.position = new Vector3(polaroidUI.transform.position.x, polaroidBottomPos, polaroidUI.transform.position.z);
        Reset();

        foreach(var i in pictureQualifiers) {
            qualifierByCriteria.Add(i.ratingName, i);
            scoreUnits.Add(i.ratingName.ToString().Replace('_', ' ').ToLower().Trim(), 0);
        } 
    }

    void Update() {
        postPictureDelay += Time.unscaledDeltaTime;

        //polaroidSkip.text = ("<color='#ff0000'>" + skipBinding.action.GetBindingDisplayString() + "</color> to Skip").ToUpper();
        //if(camControl.camSystem.shoot >= 0.7f && screenshotTimer > 1f) Reset();

        if(polaroidScreenshot.fillAmount < 1) polaroidScreenshot.fillAmount += Time.unscaledDeltaTime * 1.5f;

        if(HasTakenPicture()) {
            screenshotTimer += Time.unscaledDeltaTime * 2f;

/*             for(int i = 0; i < pictureQualifiers.Length; i++) {
                if(screenshotTimer < polaroidInterval / 4f) pictureQualifiers[i].criteriaTxt.transform.localScale = Vector3.Lerp(pictureQualifiers[i].criteriaTxt.transform.localScale, Vector3.one, screenshotTimer / 10f);
                if(screenshotTimer > polaroidInterval / 4f + (i / 2f)) pictureQualifiers[i].scoreTxt.transform.localScale = Vector3.Lerp(pictureQualifiers[i].scoreTxt.transform.localScale, Vector3.one, screenshotTimer / 45f);
            }  */
      //      if(screenshotTimer > polaroidInterval / 8f) layoutGroup.spacing = Mathf.Lerp(layoutGroup.spacing, baseSpacing, screenshotTimer / 18f);


       //     if(screenshotTimer > polaroidInterval / 2f) totalScoreBase.transform.localScale = Vector3.Lerp(totalScoreBase.transform.localScale, Vector3.one, screenshotTimer / 10f);
       //     if(screenshotTimer > polaroidInterval / 2f + 1) totalScoreValue.transform.localScale = Vector3.Lerp(totalScoreValue.transform.localScale, Vector3.one, screenshotTimer / 10f);
            ///////
            
  //          if(screenshotTimer > polaroidInterval.x / 4f && !crosshairRestObjects) {
  //              crosshairRestObjects = true;
           //     if(extraItems.Count > 0) {
                    //foreach(var sub in extraItems) {
                        //SpawnCrosshair(sub, false);
                        
                        //var s = Instantiate(splashPrefab);
                        //s.transform.SetParent(polaroidScreenshot.transform);
                        //s.transform.localPosition = new Vector3(0, 100, 0);
                        //s.transform.localScale = Vector3.one * 0.1f;
                        //s.GetComponent<Splash>().text.text = "Center Frame: " + sub.centerFrame;
              //      }
             //   } 
    //        }

           // if(screenshotTimer > polaroidInterval.y) Reset();

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
        mainScore = null;
        ready = true;
        extraItems.Clear();
        screenshotTimer = 0;
        totalScore = 0;
        polaroidUI.transform.position = new Vector3(polaroidUI.transform.position.x, polaroidBottomPos, polaroidUI.transform.position.z);
        //layoutGroup.spacing = -170;
        foreach(var i in pictureQualifiers) i.criteriaTxt.transform.localScale = i.scoreTxt.transform.localScale = Vector3.zero;
        //totalScoreBase.transform.localScale = totalScoreValue.transform.localScale = Vector3.zero;
        foreach(var i in crosshairs) Destroy(i);
        crosshairs.Clear();
        postPictureDelay = 0;
    }

    protected void SpawnCrosshair(PhotoBase item, bool isMainSubject = true) {
/*         if(camControl.lockOnSystem.GetScreenCrosshair(item) == null) return;
        var spl = Instantiate(crosshairOmcirkel);
        if(!isMainSubject) spl.GetComponent<Image>().color = Random.ColorHSV();
        spl.transform.SetParent(polaroidScreenshot.transform);
        var screenPos = camControl.lockOnSystem.GetScreenCrosshair(item).transform.localPosition;
        screenPos.z = 0;
        spl.transform.localPosition = screenPos;
        spl.transform.localScale = Vector3.one * 0.5f; */
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

    public void VisualizeScore(CameraControl.PictureScore pic, CameraControl.Screenshot screen) {
        polaroidScreenshot.fillAmount = 0;
        if(!ready || pic.item == null || screen.containedObjectTags == null || screen.containedObjectTags.Length < 1) return;
        //Debug.Log(screen.containedObjectTags);
        Reset();

        ready = false;

        mainScore = pic;
        SpawnCrosshair(pic.item);
         if(mainScore != null && mainScore.subScore != null) {
            string lm = "[" + mainScore.item.name + "]";
            foreach(var sub in mainScore.subScore) {
                if(sub == null || sub.item == null) continue;
                lm += ", " + sub.item.name;
                extraItems.Add(new CrosshairObject(sub.item, camControl.lockOnSystem.GetScreenCrosshair(sub.item).transform.position));
            }
//            Debug.LogError(lm);
         }
        
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
        if(index > 0) totalScore /= index;
        screen.score = (int)totalScore;
       // totalScoreValue.text = "<color='#" + ColorUtility.ToHtmlStringRGB(scoreGradient.Evaluate((int)totalScore / 100f)) + "'>" + screen.score.ToString() + "</color>";
        
        var actMission = MissionManager.missionManager.activeMission;
        if(actMission != null && !actMission.delivered) chaosMeter.CalculateChaos(pic, screen, actMission);
    }

/*     public void Skip() {
        screenshotTimer = polaroidInterval.x;
        Reset();
    } */

    public bool HasTakenPicture() {
        return screenshotTimer > 0;
    }

/*     public bool IsFading() {
        return screenshotTimer > polaroidInterval;
    } */

    public bool AfterDelay(float time) {
        return postPictureDelay > time;
    }
}