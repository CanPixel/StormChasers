using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PhotoItem))]
public class PhotoItemEditor : Editor {

    public override void OnInspectorGUI() {
        var tar = target as PhotoItem;
        base.OnInspectorGUI();
    }
}
