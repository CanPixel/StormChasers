using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoringSystem : MonoBehaviour {
    public Transform splashParent;
    public GameObject splashPrefab;

    public List<PictureScoring> pictureScore = new List<PictureScoring>();
    private float pictureScoreTimer = 0;
    private int scoringSplashIndex = 0;

    public bool ready = true;

    [System.Serializable]
    public class PictureScoring {
        public string text;
        [HideInInspector] public Splash splash;
        //public int bonus = 0;
        public Vector3 pos; 
        public float duration = 2;
        public Font font;
    }

/*     void Update() {
        if(pictureScoreTimer > 0) pictureScoreTimer += Time.unscaledDeltaTime;

        if(!ready) {
            //if(pictureScore[scoringSplashIndex].splash && pictureScore[scoringSplashIndex].splash.markForDestroy && pictureScore[scoringSplashIndex].markedForDestroy) CycleSplash();
        }
    }  */

    protected void CycleSplash() {
        scoringSplashIndex++;
        Spawn(scoringSplashIndex);
        
        if(scoringSplashIndex >= pictureScore.Count) {
            ready = true;
            scoringSplashIndex = 0;
            pictureScoreTimer = 0;
        }
    }

    public void CalculatePictureScore(CameraControl.Screenshot screen) {
        if(!ready) return;
        ready = false;
        scoringSplashIndex = 0;
        pictureScoreTimer = 0.1f;
        Spawn(scoringSplashIndex);
    }

    private void Spawn(int index) {
        if(index < 0 || index >= pictureScore.Count) return;
        var ob = pictureScore[index];
        var spl = SplashManager.SpawnSplash(ob.text, ob.pos, ob.duration);
        spl.text.font = ob.font;
        spl.OnDestroyCalled.AddListener(CycleSplash);
    }
}