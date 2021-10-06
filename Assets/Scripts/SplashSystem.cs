using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplashSystem : MonoBehaviour {
    public Transform splashParent;
    public GameObject splashPrefab;

    public List<SplashElement> splashes = new List<SplashElement>();
    private int scoringSplashIndex = 0;

    [HideInInspector] public bool ready = true;

    [System.Serializable]
    public class SplashElement {
        public string text;
        [HideInInspector] public Splash splash;
        public Vector3 pos;
        public float rot, scale = 1; 
        public float duration = 2;
        public Font font;
    }

    protected void CycleSplash() {
        scoringSplashIndex++;
        Spawn(scoringSplashIndex);
        
        if(scoringSplashIndex >= splashes.Count) {
            ready = true;
            scoringSplashIndex = 0;
        }
    }

    public void CalculatePictureScore(CameraControl.Screenshot screen) {
        if(!ready) return;
        ready = false;
        scoringSplashIndex = 0;
        Spawn(scoringSplashIndex);
    }

    private void Spawn(int index) {
        if(index < 0 || index >= splashes.Count) return;
        var ob = splashes[index];
        var spl = SpawnSplash(ob.text, ob.pos, ob.rot, ob.scale, ob.duration);
        spl.text.font = ob.font;
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