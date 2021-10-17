using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelSwitch : MonoBehaviour {
    public Text levelTitle;

    private static float alpha = 0;

    private float fadeDelay = 0;
    
    public float untilFade = 4f;
    public float fadeSpeed = 1.2f;

    private Vector3 baseScale;

    private static LevelSwitch processor;

    void Start() {
        baseScale = levelTitle.transform.localScale;
        if(processor == null) processor = this;
        alpha = 4f;
    }

    void Update() {
        if(processor == null) return;

        if(fadeDelay > 0) {
            fadeDelay -= Time.deltaTime;
            levelTitle.transform.localScale = Vector3.Lerp(levelTitle.transform.localScale, baseScale * 1.2f, Time.deltaTime * fadeSpeed);
            return;
        }

        alpha = Mathf.Lerp(alpha, -0.1f, Time.deltaTime * fadeSpeed);

        levelTitle.color = new Color(levelTitle.color.r, levelTitle.color.g, levelTitle.color.b, alpha);
        levelTitle.transform.localScale = Vector3.Lerp(levelTitle.transform.localScale, baseScale, Time.deltaTime * fadeSpeed * 2f);
    }

    public static void SwitchLevel(Region region) {
        processor.levelTitle.text = region.name;
        processor.levelTitle.font = region.font;
        alpha = 4f;
        processor.levelTitle.transform.localScale = processor.baseScale * 1.1f;
        processor.fadeDelay = processor.untilFade;
    }
}
