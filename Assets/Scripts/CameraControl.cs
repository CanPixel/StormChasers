using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;
using Cinemachine.PostFX;

public class CameraControl : MonoBehaviour {
    public GameObject[] enableOnFirstPerson;
    public CameraSystem camSystem;

    public Vector3 BoostFollowOffset = new Vector3(0, 2.8f, -6);
    public float BoostFollowDamping = 2f;

    public float physicalDistanceFactor = 0.5f;

    [HideInInspector] public bool photoBook = false;

    public Vector2 polaroidInterval = new Vector2(12, 14);
    public float polaroidBottomPos = 0, polaroidTopPos = 100;
    public int maxPhotosInPortfolio = 5;

    public int resWidth = 1600, resHeight = 900;

    public float slowMotionDamping = 3f, slowMotionTime = 0.1f;

    public enum CameraState {
        CARVIEW = 0, CAMVIEW, DASHVIEW
    }
    public CameraState cameraState = CameraState.CARVIEW;

    public Vector3 mascotteRotationOffset = new Vector3(-90, 90, 0);

    [HideInInspector] public Vector2 rotationInput;

    private float screenshotTimer = 0;

    [System.Serializable]
    public class ScoringObject {
        public GameObject obj;
        public float scoreReduction = 1;
    }

    [System.Serializable]
    public class PictureScore {
        public Screenshot screenshot;

        public List<ScoringObject> scoringObjects = new List<ScoringObject>();
        
        public string name;
        public bool onScreen;
        public float physicalDistance;
        public float centerFrame;
        public float focus, motion;
        public float objectSharpness {
            get; private set;
        }

        public override string ToString() {
            return "[" + name + "]  || Center Frame: " + centerFrame + " || Motion Stability: " + motion + " || Sharpness: " + objectSharpness + " [Focus: " + focus + ", PhysDist: " + physicalDistance + "]";
        }

        public void CalculateSharpness() {
            objectSharpness = (int)Mathf.Clamp(focus - physicalDistance, 0, 100);
            objectSharpness = 100 - objectSharpness;
        }
    }

    [System.Serializable]
    public class Screenshot {
        public string name;
        public Texture2D image;

        [Range(0, 100)]
        public int score = 0;

        public static Screenshot Create(string name, Texture2D sprite) {
            Screenshot screen = new Screenshot();
            screen.name = name;
            screen.image = sprite;
            return screen;
        }
    }

    [System.Serializable]
    public class CameraSystem {
        public float aim, shoot;

        [System.Serializable]
        public class ShaderFilter {
            public string name;
            public PostProcessProfile profile;
            public Sprite icon;
        }

        public ShaderFilter[] shaderFilters;

        [HideInInspector] public int filterIndex = 0;

        public override string ToString() {
            return "Aim: " + aim + " || Shoot: " + shoot;
        }
    }

    private float recenterTime = 0;
    public float recenterDuration = 1f;

    public List<Screenshot> screenshots = new List<Screenshot>();
    private List<GameObject> photoBookShots = new List<GameObject>();

    [Space(10)]
    public Cinemachine.PostFX.CinemachinePostProcessing postProcessing;
    public Cinemachine.CinemachineVirtualCamera firstPersonLook;
    public Cinemachine.CinemachineVirtualCamera thirdPersonLook;
    private Cinemachine.CinemachinePOV pov;
    private Cinemachine.CinemachineOrbitalTransposer orbitalTransposer;
    public Cinemachine.CinemachineBrain cinemachineBrain;
    public GameObject photoBookUI, polaroidPrefab;
    public GameObject cameraMascotte;
    public Transform photoBookScrollPanel;
    public UIBob reticleBob;
    public CarMovement carMovement;
    public CameraCanvas cameraCanvas;
    public Text polaroidTitle;
    public Image polaroidUI, polaroidScreenshot;
    public LockOnSystem lockOnSystem;
    public SplashSystem splashSystem;
    public ShaderReel shaderReel;
    private UnityEngine.Rendering.PostProcessing.MotionBlur motionBlur;

    private Vector3 baseFollowOffset;
    private float motion, baseZDamping;

    void Start() {
        pov = firstPersonLook.GetCinemachineComponent<Cinemachine.CinemachinePOV>();
        orbitalTransposer = thirdPersonLook.GetCinemachineComponent<Cinemachine.CinemachineOrbitalTransposer>();
        foreach(var i in enableOnFirstPerson) i.SetActive(false); 
        polaroidUI.transform.position = new Vector3(polaroidUI.transform.position.x, polaroidBottomPos, polaroidUI.transform.position.z);

        baseFollowOffset = orbitalTransposer.m_FollowOffset;
        baseZDamping = orbitalTransposer.m_ZDamping;

        motionBlur = postProcessing.m_Profile.GetSetting<UnityEngine.Rendering.PostProcessing.MotionBlur>();
    }

    void Update() {
        MotionBlurReticle();
        
        photoBookUI.SetActive(photoBook);

        Time.timeScale = Mathf.Lerp(Time.timeScale, (IsAiming()) ? slowMotionTime : 1.0f, Time.unscaledDeltaTime * slowMotionDamping * (!IsAiming() ? 4f : 1f));
        SoundManager.SlowMo();

        if(recenterTime > 0 && Mathf.Abs(orbitalTransposer.m_XAxis.Value) > 1) {
            recenterTime -= Time.deltaTime * 2f;
            orbitalTransposer.m_XAxis.Value = Mathf.Lerp(orbitalTransposer.m_XAxis.Value, 0, Time.deltaTime * 6f);
        }
        if(carMovement.GetLooking().magnitude >= 0.65f && recenterTime > 0) recenterTime = 0;

        if(camSystem.aim > 0.4) FirstPersonLook();
        else ThirdPersonLook();

        cameraMascotte.transform.localScale = Vector3.Lerp(cameraMascotte.transform.localScale, Vector3.one, Time.deltaTime * 5f);

        if(screenshotTimer > 0) {
            screenshotTimer += Time.unscaledDeltaTime * 2f;

            if(screenshotTimer > polaroidInterval.y) screenshotTimer = 0;

            if(screenshotTimer > polaroidInterval.x) polaroidUI.transform.position = new Vector3(polaroidUI.transform.position.x, Mathf.Lerp(polaroidUI.transform.position.y, polaroidBottomPos, Time.unscaledDeltaTime * 5f), polaroidUI.transform.position.z);
            else polaroidUI.transform.position = new Vector3(polaroidUI.transform.position.x, Mathf.Lerp(polaroidUI.transform.position.y, polaroidTopPos, Time.unscaledDeltaTime * 7f), polaroidUI.transform.position.z);
        }
    }

    protected void MotionBlurReticle() {
        var motionDist = Vector3.Distance(cameraCanvas.movementReticle.transform.position, cameraCanvas.baseReticle.transform.position) * 1.5f;
        motionBlur.shutterAngle.value = Mathf.Clamp(motionDist, 180, 360);
        if(motionDist < 100) motionBlur.enabled.value = false;
        else motionBlur.enabled.value = true; 
        motion = motionDist;
    }

    protected bool IsAiming() {
        return camSystem.aim >= 0.5f;
    }

    public void CycleFilters(float add) {
        camSystem.filterIndex += (int)add;
        if(camSystem.filterIndex >= camSystem.shaderFilters.Length) camSystem.filterIndex = 0;
        if(camSystem.filterIndex < 0) camSystem.filterIndex = camSystem.shaderFilters.Length - 1;
        postProcessing.m_Profile = camSystem.shaderFilters[camSystem.filterIndex].profile;
        shaderReel.current = camSystem.filterIndex;

        SoundManager.PlayUnscaledSound("ShaderSwitch");
        reticleBob.Bob();
    }

    protected void FirstPersonLook() {
        firstPersonLook.Priority = 12;
        thirdPersonLook.Priority = 10;

        if(cameraState != CameraState.CAMVIEW) {
            foreach(var i in enableOnFirstPerson) i.SetActive(true); 
            cameraState = CameraState.CAMVIEW;
        }

        if(camSystem.shoot >= 0.5f && screenshotTimer <= 0) TakePicture();

        cameraMascotte.SetActive(false);
    }
    protected void ThirdPersonLook() {
        firstPersonLook.Priority = 10;
        thirdPersonLook.Priority = 12;

        if(cameraState != CameraState.CARVIEW) {
            foreach(var i in enableOnFirstPerson) i.SetActive(false); 
            cameraState = CameraState.CARVIEW;
        } 

        cameraMascotte.SetActive(true);

        cameraMascotte.transform.rotation = transform.rotation;
        cameraMascotte.transform.Rotate(mascotteRotationOffset.x, orbitalTransposer.m_XAxis.Value, mascotteRotationOffset.z);
        
        pov.m_HorizontalAxis.Value = cameraMascotte.transform.localEulerAngles.z + transform.eulerAngles.y;

        orbitalTransposer.m_FollowOffset = Vector3.Lerp(orbitalTransposer.m_FollowOffset, (carMovement.boostScript.isBoosting) ? BoostFollowOffset : baseFollowOffset, Time.deltaTime * BoostFollowDamping);
        orbitalTransposer.m_ZDamping = Mathf.Lerp(orbitalTransposer.m_ZDamping, (carMovement.boostScript.isBoosting) ? 0.05f : baseZDamping, Time.deltaTime * BoostFollowDamping / 2f);
    }

    public void AnimateCameraMascotte() {
        cameraMascotte.transform.localScale = Vector3.one * 1.2f;
    }

    public void Recenter() {
        pov.m_VerticalAxis.Value = 0;
        recenterTime = recenterDuration;
    }

/*     private void ResetScreenshot() {
        screenshotTimer = 0;
        polaroidUI.transform.position = new Vector3(polaroidUI.transform.position.x, polaroidBottomPos, polaroidUI.transform.position.z);
    } */

    protected void TakePicture() {
        if(!splashSystem.ready) return;

        carMovement.HapticFeedback(0f, 0.5f, 0.4f);

        var cam = cinemachineBrain.OutputCamera;
        RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
        cam.targetTexture = rt;
        var shot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
        cam.Render();
        RenderTexture.active = rt;
        shot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
        shot.Apply();
        cam.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);

        Sprite temp = Sprite.Create(shot, new Rect(0, 0, resWidth, resHeight), Vector2.zero);
        polaroidScreenshot.sprite = temp;
        var scren = Screenshot.Create(Time.time.ToString(), shot);
        screenshots.Add(scren);

        var pol = Instantiate(polaroidPrefab);
        pol.transform.SetParent(photoBookScrollPanel);
        pol.transform.localScale = Vector3.one * 0.5f;
        var sf = pol.GetComponent<ShaderFilter>();
        sf.icon.sprite = temp;

        //[Photo Naming - Object Prioritization - Scoring section]
        string photoName = "";
        PictureScore score = new PictureScore();
        //foreach(var i in lockOnSystem.allTargets) {
        var i = cameraCanvas.targetedObject;
        if(i != null) {
            Vector3 targetPos = cam.WorldToViewportPoint(i.transform.position);
            bool isOnScreen = (targetPos.z > 0 && targetPos.x > 0 && targetPos.x < 1 && targetPos.y > 0 && targetPos.y < 1) ? true : false;

            if(isOnScreen) {
                var dist = Mathf.Clamp(Mathf.Abs(Vector3.Distance(transform.position, i.transform.position)), 1, cameraCanvas.maxDistance);

                photoName = i.name + "!";
                score.name = photoName;
                score.screenshot = scren;
                score.physicalDistance = (int)Mathf.Clamp(dist * physicalDistanceFactor, 0, 100);
                score.onScreen = isOnScreen;
                score.motion = 100 - (int)Mathf.Clamp(motion, 0, 100);
                score.focus = (int)cameraCanvas.GetFocusValue();
                score.CalculateSharpness();

                var camPos = cam.WorldToViewportPoint(i.transform.position);
                camPos.z = 0;
                var centerFrame = Vector3.Distance(new Vector3(0.5f, 0.5f, 0), camPos) * 800f;
                score.centerFrame = 100 - (int)Mathf.Clamp(centerFrame, 0, 100);

                Debug.Log(score);
            }
        }
        //}
        polaroidTitle.text = sf.text.text = photoName;
        splashSystem.VisualizeScore(score, scren);

        photoBookShots.Add(pol);

        if(screenshots.Count >= maxPhotosInPortfolio) {
            Destroy(screenshots[0].image);
            screenshots.RemoveAt(0);
            photoBookShots.RemoveAt(0);
            Destroy(photoBookShots[0]);
        }

        screenshotTimer = 0.1f;
        SoundManager.PlayUnscaledSound("PhotoShoot", 0.8f);

        polaroidUI.transform.position = new Vector3(polaroidUI.transform.position.x, polaroidBottomPos, polaroidUI.transform.position.z);
    }
}