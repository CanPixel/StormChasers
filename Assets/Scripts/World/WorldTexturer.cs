using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldTexturer : MonoBehaviour {
    public bool disableAtStart = true;

    void Start() {
        if(disableAtStart) Enable(false);
    }

    public SpriteRenderer[] sprites;

    public void SetSprite(Sprite tex) {
        foreach(var i in sprites) i.sprite = tex;
        Enable(true);
    }

    public void SynchToMission() {
        if(MissionManager.missionManager == null) return;
        SetSprite(MissionManager.missionManager.camControl.GetLastScreenshot());
    }

    private void Enable(bool i) {
        foreach(var l in sprites) l.gameObject.SetActive(i);
    }
}
