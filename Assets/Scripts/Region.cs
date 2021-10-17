using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Region : MonoBehaviour {
    public Font font;

    void OnTriggerEnter(Collider col) {
        if(col.gameObject.tag == "Player" && gameObject.tag == "AREA") LevelSwitch.SwitchLevel(this);
    }
}
