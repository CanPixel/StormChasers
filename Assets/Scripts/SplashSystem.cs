using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplashSystem : MonoBehaviour {
    public Transform splashParent;
    public GameObject splashPrefab;

    public Gradient scoreGradient;

    public List<SplashElement> splashes = new List<SplashElement>();
    public ScoreElement scoreFormat;
    private int splashIndex = 0;

    private int baseSplashAmount;

    [System.Serializable]
    public enum Rating {
        Center_Frame, Object_Sharpness, Motion_Stability
    }
    public Dictionary<string, float> scoreUnits = new Dictionary<string, float>();

    [HideInInspector] public bool ready = true;

    [System.Serializable]
    public class SplashElement {
        public string text;
        [HideInInspector] public Splash splash;
        public Vector3 pos;
        public float rot, scale = 1; 
        public float duration = 2;
        public Font font;
        public Color color;

        [HideInInspector] public bool isRating = false;
        [HideInInspector] public string rating;

        public SplashElement SetText(string txt) {
            text = txt;
            return this;
        }
    }

    [System.Serializable]
    public class ScoreElement : SplashElement {
        [HideInInspector] public List<PhotoItem> pictureObjects = new List<PhotoItem>();
    }

    void Start() {
        scoreUnits.Clear();
        baseSplashAmount = splashes.Count;
        foreach(var i in System.Enum.GetNames(typeof(Rating))) {
            var se = new ScoreElement().SetText(i.Replace('_', ' ').Trim());
            se.font = scoreFormat.font;
            se.scale = scoreFormat.scale;
            se.pos = scoreFormat.pos;
            se.color = scoreFormat.color;
            se.isRating = true;
            se.rating = i.Replace('_', ' ').ToLower().Trim();
            se.duration = scoreFormat.duration;
            se.rot = scoreFormat.rot;
            splashes.Add(se);
            scoreUnits.Add(i.Replace('_', ' ').ToLower().Trim(), 0);
        }
    }

    protected void CycleSplash() {
        splashIndex++;
        if(splashIndex >= splashes.Count) {
            ResetSplash();
            return;
        }
        Spawn(splashIndex, splashes[splashIndex].isRating);
    }

    private void ResetSplash() {
        ready = true;
        splashIndex = 0;
    }

    public void VisualizeScore(CameraControl.PictureScore pic, CameraControl.Screenshot screen) {
        if(!ready) return;
        ready = false;
        splashIndex = 0;
        Spawn(splashIndex);

        scoreFormat.pictureObjects.Clear();

        int index = 0;
        foreach(var i in System.Enum.GetNames(typeof(Rating))) {
            ScoreElement scoreElement = scoreFormat;
            scoreElement.pos = scoreFormat.pos + new Vector3(0, index, 0);
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
            scoreElement.text = i + ": " + scoreUnits[str];
            index++;
        }
    }

    private void Spawn(int index, bool isRating = false) {
        if(index < 0 || index >= splashes.Count) return;
        var ob = splashes[index];
        var spl = SpawnSplash(ob.text, ob.pos, ob.rot, ob.scale, ob.duration);
        if(isRating) spl.text.text = ob.text + ": <color='#" + ColorUtility.ToHtmlStringRGB(scoreGradient.Evaluate(scoreUnits[ob.rating] / 100f)) + "'>" + scoreUnits[ob.rating] + "</color>";
        spl.text.font = ob.font;
        spl.text.color = ob.color;
        spl.OnDestroyCalled.AddListener(CycleSplash);
    }
    public Splash SpawnSplash(string text, Vector3 pos, float rot, float scale, float duration) {
        var obj = Instantiate(splashPrefab);
        var spl = obj.GetComponent<Splash>();
        spl.targetScale = Vector3.one * 1.1f;
        obj.transform.SetParent(splashParent);
        obj.transform.localScale = Vector3.zero;
        obj.transform.localPosition = pos;
        obj.transform.localEulerAngles = new Vector3(0, 0, rot);
        spl.targetScale = Vector3.one * scale;
        spl.text.text = text;
        spl.duration = duration;
        return spl;
    }
}