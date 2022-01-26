using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorldTexturer : MonoBehaviour {
    public bool disableAtStart = true;
    public Text label;

    void Start() {
        if(disableAtStart) Enable(false);
    }

    public SpriteRenderer[] sprites;

    public void SetSprite(Sprite tex) {
        foreach(var i in sprites) i.sprite = tex;
        Enable(true);
    }

    public void SetLabel(string txt) {
        label.text = txt;
    }

/*     public void SynchToMission() {
        if(MissionManager.missionManager == null) return;
        SetSprite(MissionManager.missionManager.camControl.GetLastScreenshot());
    } */

    private void Enable(bool i) {
        foreach(var l in sprites) l.gameObject.SetActive(i);
    }
}
