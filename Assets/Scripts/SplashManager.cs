using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplashManager : MonoBehaviour {
    public GameObject splashPrefab;
    public Transform splashParent;

    public static SplashManager self;

    void Start() {
        self = this;
    }

    public static Splash SpawnSplash(string text, Vector3 pos, float duration) {
        var obj = Instantiate(self.splashPrefab);
        var spl = obj.GetComponent<Splash>();
        spl.targetScale = Vector3.one * 1.1f;
        obj.transform.SetParent(self.splashParent);
        obj.transform.localScale = Vector3.zero;
        obj.transform.localPosition = pos;
        spl.text.text = text;
        spl.duration = duration;
        return spl;
    }
}
