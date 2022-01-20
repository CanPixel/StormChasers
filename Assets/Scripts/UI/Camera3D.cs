using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera3D : MonoBehaviour {
    public Camera cameraCamera;
    public SpriteRenderer polaroid;

    public RotPos[] pictureAnimation;
    public Vector3 baseRotation, basePosition;
    public float lerpSpeed = 2f, rotLerpSpeed = 4f;

    [System.Serializable]
    public class RotPos {
        public Vector3 pos, rot;
        public float duration = 2f;
        public float fov = 109;
    }
    private int animIndex = 0;
    private float animTime = 0;

    public float takePictureDuration = 4f;
    private float takePictureTime = 0;

    void Update() {
        if(takePictureTime > 0) {
            takePictureTime -= Time.deltaTime;
            animTime += Time.deltaTime;

            if(animTime > pictureAnimation[animIndex].duration) AdvanceNext();
        }

        cameraCamera.transform.localPosition = Vector3.Lerp(cameraCamera.transform.localPosition, (IsTakingPictureAnimation() ? pictureAnimation[animIndex].pos : basePosition), Time.deltaTime * lerpSpeed);
        cameraCamera.transform.localRotation = Quaternion.Lerp(cameraCamera.transform.localRotation, Quaternion.Euler(IsTakingPictureAnimation() ? pictureAnimation[animIndex].rot : baseRotation), Time.deltaTime * rotLerpSpeed);
        cameraCamera.fieldOfView = Mathf.Lerp(cameraCamera.fieldOfView, pictureAnimation[animIndex].fov, Time.deltaTime * 4f);
    }

    protected void AdvanceNext() {
        animTime = 0;
        animIndex = Mathf.Clamp(animIndex + 1, 0, pictureAnimation.Length - 1);
    }

    public void DoPicture(CameraControl.Screenshot screen, int resWidth, int resHeight) {
        animIndex = 0;
        animTime = 0;
        takePictureTime = takePictureDuration;
        polaroid.sprite = Sprite.Create(screen.image, new Rect(0, 0, resWidth, resHeight), Vector2.one * 0.5f);
    }

    public bool IsTakingPictureAnimation() {
        return takePictureTime > 0;
    }
}
