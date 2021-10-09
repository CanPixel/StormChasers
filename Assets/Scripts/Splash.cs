using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class Splash : MonoBehaviour {
    public Text text;

    public float duration;
    private float time = 0;

    public Vector3 targetScale;

    private bool markForDestroy = false;
    public bool DestroyAfterDuration = false;

    public UnityEvent OnDestroyCalled;

    void Update() {
        time += Time.unscaledDeltaTime;
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * 15f);
        if(time > duration) {
            markForDestroy = true;
            targetScale = Vector3.one * -0.1f;
        }

        if(DestroyAfterDuration && markForDestroy && transform.localScale.x <= 0) ManualDestroy();
    }

    public void ManualDestroy() {
        OnDestroyCalled.Invoke();
        Destroy(gameObject);
    }
}
