using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhotoItem : MonoBehaviour {
    public bool showLockOnMarker = true;
    public int baseScore = 10;

    [System.Serializable]
    public class AntiObject {
        public GameObject obj;
        public float scoreReduction = 1;
    }

    [System.Serializable]
    public enum PictureMechanic {
        CENTER_FRAME, OBJECT_ORIENTATION, FOCUS
    }
    [System.Serializable]
    public class PictureQualification {
        public PictureMechanic pictureMechanic;
        [Range(0f, 1f)]
        public float weight = 0.5f;

        [Range(0, 100)]
        public int score = 0;
    }

    [System.Serializable]
    public class PictureScore {
        public PictureQualification[] pictureQualifications;
        public AntiObject[] antiObject;
        [Range(0f, 1f)]
        public float antiObjectFactor;
        public float minDistance = 10;
    }

    public PictureScore pictureScore;

    void OnValidate() {
        if(pictureScore.minDistance < 2) pictureScore.minDistance = 2;
    }
}
