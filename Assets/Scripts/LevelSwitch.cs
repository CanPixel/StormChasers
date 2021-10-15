using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelSwitch : MonoBehaviour {
    public Text levelTitle;
    public Font font;

    private static float alpha = 0;

    private float fadeDelay = 0;
    private const float untilFade = 5f;
    public const float fadeSpeed = 1.2f;
    private Vector3 baseScale;

    private static LevelSwitch processor;

    void Start() {
        baseScale = levelTitle.transform.localScale;
        if(processor == null) processor = this;
        alpha = 4f;
    }

    void Update() {
        if(processor == null || processor != this) return;

        if(fadeDelay > 0) {
            fadeDelay -= Time.deltaTime;
            levelTitle.transform.localScale = Vector3.Lerp(levelTitle.transform.localScale, baseScale * 1.2f, Time.deltaTime * fadeSpeed);
            return;
        }

        alpha = Mathf.Lerp(alpha, -0.1f, Time.deltaTime * fadeSpeed);

        levelTitle.color = new Color(levelTitle.color.r, levelTitle.color.g, levelTitle.color.b, alpha);
        levelTitle.transform.localScale = Vector3.Lerp(levelTitle.transform.localScale, baseScale, Time.deltaTime * fadeSpeed * 2f);
    }

    void OnTriggerEnter(Collider col) {
        if(col.gameObject.tag == "Player" && gameObject.tag == "AREA") SwitchLevel(transform.name);
    }

    public void SwitchLevel(string txt) {
        levelTitle.text = txt;
        levelTitle.font = font;
        alpha = 1f;
        levelTitle.transform.localScale = baseScale * 1.1f;
        fadeDelay = untilFade;
    }
}
