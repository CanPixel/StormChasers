using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MissionManager))]
public class MissionManagerEditor : Editor {

    public override void OnInspectorGUI() {
        var tar = target as MissionManager;
        base.OnInspectorGUI();
    }
}
