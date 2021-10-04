using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class Splash : MonoBehaviour {
    public Text text;
    public Image icon;

    public float duration;
    private float time = 0;

    public Vector3 targetScale;

    public bool markForDestroy = false;

    public UnityEvent OnDestroyCalled;

    void Start() {
        text = GetComponentInChildren<Text>();
        icon = GetComponentInChildren<Image>();
    }

    void Update() {
        time += Time.unscaledDeltaTime;
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * 15f);
        if(time > duration) {
            markForDestroy = true;
            targetScale = Vector3.one * -0.1f;
        }

        if(markForDestroy && transform.localScale.x <= 0) {
            OnDestroyCalled.Invoke();
            Destroy(gameObject);
        }
    }
}
