using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhotoItem : MonoBehaviour {
    public MeshRenderer render;
    public bool showLockOnMarker = true;
    public bool isComposite = false;

    private PhotoKeyPoint[] keyPoints;

    void Start() {
        if(isComposite) keyPoints = GetComponentsInChildren<PhotoKeyPoint>();
    }

    public PhotoKeyPoint[] GetKeyPoints() {
        return keyPoints;
    }
}
