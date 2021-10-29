using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplashSystem : MonoBehaviour {
    public Transform splashParent;
    public GameObject splashPrefab;

    public static SplashSystem self;

    void Start() {
        self = this;
    }

    public Splash SpawnSplash(string text, Vector3 pos, float rot, float scale, float duration, TextAnchor align) {
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
}