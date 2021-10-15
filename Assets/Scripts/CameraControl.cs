using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;
using Cinemachine.PostFX;

public class CameraControl : MonoBehaviour {
    public GameObject[] enableOnFirstPerson, disableOnFirstPerson;
    public CameraSystem camSystem;

    public Vector3 BoostFollowOffset = new Vector3(0, 2.8f, -6);
    public float BoostFollowDamping = 2f;

    [HideInInspector] public bool photoBook = false;

    public int maxPhotosInPortfolio = 5;

    public int resWidth = 1600, resHeight = 900;

    public float slowMotionDamping = 3f, slowMotionTime = 0.1f;

    public enum CameraState {
        CARVIEW = 0, CAMVIEW, DASHVIEW
    }
    public CameraState cameraState = CameraState.CARVIEW;

    public Vector3 mascotteRotationOffset = new Vector3(-90, 90, 0);

    [System.Serializable]
    public class PictureScore {
        public Screenshot screenshot;

        public PhotoItem item;
        public List<PictureScore> subScore = new List<PictureScore>();
        
        public string name;
        public bool onScreen;
        public float physicalDistance;
        public float centerFrame;
        public float focus, motion;
        public float objectSharpness;

        public override string ToString() {
            return "[" + name + "]  || Center Frame: " + centerFrame + " || Motion Stability: " + motion + " || Sharpness: " + objectSharpness + " [Focus: " + focus + ", PhysDist: " + physicalDistance + "]";
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
    public LayerMask thirdPersonCull;
    public LayerMask firstPersonCull;
    public Camera cam;
    public Cinemachine.PostFX.CinemachinePostProcessing postProcessing;
    public Cinemachine.CinemachineVirtualCamera firstPersonLook;
    public Cinemachine.CinemachineVirtualCamera stuntLook;
    [HideInInspector] public Cinemachine.CinemachineTransposer stuntTransposer;
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
    public LockOnSystem lockOnSystem;
    public SplashSystem splashSystem;
    public RatingSystem ratingSystem;
    public ShaderReel shaderReel;
    private UnityEngine.Rendering.PostProcessing.MotionBlur motionBlur;

    private Vector3 baseFollowOffset;
    private float motion, baseZDamping;

    void Start() {
        pov = firstPersonLook.GetCinemachineComponent<Cinemachine.CinemachinePOV>();
        orbitalTransposer = thirdPersonLook.GetCinemachineComponent<Cinemachine.CinemachineOrbitalTransposer>();
        stuntTransposer = stuntLook.GetCinemachineComponent<Cinemachine.CinemachineTransposer>();
        foreach(var i in enableOnFirstPerson) i.SetActive(false); 

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

        cameraMascotte.transform.rotation = transform.rotation;
        cameraMascotte.transform.Rotate(mascotteRotationOffset.x, orbitalTransposer.m_XAxis.Value, mascotteRotationOffset.z);
        cameraMascotte.transform.localScale = Vector3.Lerp(cameraMascotte.transform.localScale, Vector3.one, Time.deltaTime * 5f);
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

/*     public void RollDamp(float roll) {
        orbitalTransposer.m_RollDamping = roll;
        orbitalTransposer.m_AngularDamping = roll;
    } */

    public void SetCameraPriority(CinemachineVirtualCamera cam, int i) {
        cam.Priority = i;
    }

    public void CycleFilters(float add) {
        camSystem.filterIndex += (int)add;
        if(camSystem.filterIndex >= camSystem.shaderFilters.Length) camSystem.filterIndex = 0;
        if(camSystem.filterIndex < 0) camSystem.filterIndex = camSystem.shaderFilters.Length - 1;
        postProcessing.m_Profile = camSystem.shaderFilters[camSystem.filterIndex].profile;
        shaderReel.current = camSystem.filterIndex;
        cameraCanvas.ReloadDepthOfField();

        SoundManager.PlayUnscaledSound("ShaderSwitch");
        reticleBob.Bob();
    }

    protected void FirstPersonLook() {
        if(cinemachineBrain.IsLive(firstPersonLook) && !cinemachineBrain.IsBlending) cam.cullingMask = firstPersonCull;

        firstPersonLook.Priority = 15;
        thirdPersonLook.Priority = 10;

        if(cameraState != CameraState.CAMVIEW) {
            foreach(var i in enableOnFirstPerson) i.SetActive(true); 
            foreach(var i in disableOnFirstPerson) i.SetActive(false); 
            cameraState = CameraState.CAMVIEW;
        }

        SynchTransposer();

        if(camSystem.shoot >= 0.5f && !ratingSystem.HasTakenPicture()) TakePicture();
    }
    protected void ThirdPersonLook() {
        cam.cullingMask = thirdPersonCull;
        firstPersonLook.Priority = 10;
        thirdPersonLook.Priority = 12;

        if(cameraState != CameraState.CARVIEW) {
            foreach(var i in enableOnFirstPerson) i.SetActive(false); 
            foreach(var i in disableOnFirstPerson) i.SetActive(true); 
            cameraState = CameraState.CARVIEW;
        } 

        pov.m_HorizontalAxis.Value = cameraMascotte.transform.localEulerAngles.z + transform.eulerAngles.y; 

        orbitalTransposer.m_FollowOffset = Vector3.Lerp(orbitalTransposer.m_FollowOffset, (carMovement.boostScript.isBoosting) ? BoostFollowOffset : baseFollowOffset, Time.deltaTime * BoostFollowDamping);
        orbitalTransposer.m_ZDamping = Mathf.Lerp(orbitalTransposer.m_ZDamping, (carMovement.boostScript.isBoosting) ? 0.05f : baseZDamping, Time.deltaTime * BoostFollowDamping / 2f);
    }

    public void SynchTransposer() {
        cameraMascotte.transform.rotation = Quaternion.Euler(0, 0, transform.eulerAngles.y);
        cameraMascotte.transform.localRotation = Quaternion.Euler(0, 0, pov.m_HorizontalAxis.Value);
        
        orbitalTransposer.m_XAxis.Value = pov.m_HorizontalAxis.Value - transform.eulerAngles.y;
    }

    public void AnimateCameraMascotte() {
        cameraMascotte.transform.localScale = Vector3.one * 1.2f;
    }

    public void Recenter() {
        pov.m_VerticalAxis.Value = 0;
        recenterTime = recenterDuration;
    }
    public void RecenterY() {
        pov.m_VerticalAxis.Value = 0;     
    }

    public bool HasTakenPicture() {
        return ratingSystem.HasTakenPicture();
    }

    protected void TakePicture() {
        if(!ratingSystem.ready) return;

        carMovement.HapticFeedback(0f, 0.75f, 0.2f);

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
        ratingSystem.SetPolaroidSprite(temp);
        var scren = Screenshot.Create(Time.time.ToString(), shot);

        var pol = Instantiate(polaroidPrefab);
        pol.transform.SetParent(photoBookScrollPanel);
        pol.transform.localScale = polaroidPrefab.transform.localScale;
        var sf = pol.GetComponent<ShaderFilter>();
        sf.icon.sprite = temp;

        //=============== [Photo Naming - Object Prioritization - Scoring section]
        string photoName = "", photoWithoutScore = "";
        PictureScore score = new PictureScore();

        var allObjects = lockOnSystem.GetOnScreenObjects();
        List<PhotoItem> restItems = new List<PhotoItem>();

        PhotoItem i = null;
        Vector3 targetPos = Vector3.zero;
        bool isOnScreen = true;
        if(allObjects.Count > 0) {
            i = allObjects[0];
            if(i != null) {
                targetPos = cam.WorldToViewportPoint(i.transform.position);
                isOnScreen = (targetPos.z > 0 && targetPos.x > 0 && targetPos.x < 1 && targetPos.y > 0 && targetPos.y < 1) ? true : false;
            }
        }

        if(isOnScreen) {
            photoName = photoWithoutScore = (i != null) ? i.name : "Random Picture";
            score.item = i;
            score.name = photoName;
            score.screenshot = scren;
            score.physicalDistance = cameraCanvas.GetPhysicalDistance(i);
            score.onScreen = isOnScreen;
            score.motion = 100 - (int)Mathf.Clamp(motion, 0, 100);
            score.centerFrame = lockOnSystem.GetCrosshairCenter(i);
            score.focus = cameraCanvas.GetFocusValue();
            score.objectSharpness = lockOnSystem.GetObjectSharpness(i);
//                Debug.Log(score);
        }
        foreach(var obj in allObjects) if(obj != i) restItems.Add(obj);

        //SubScores with multiple objects
        foreach(var rest in restItems) {
            PictureScore sub = new PictureScore();
            targetPos = cam.WorldToViewportPoint(rest.transform.position);
            isOnScreen = (targetPos.z > 0 && targetPos.x > 0 && targetPos.x < 1 && targetPos.y > 0 && targetPos.y < 1) ? true : false;

            photoName = photoWithoutScore = (rest != null) ? rest.name : "Random Picture";
            sub.name = photoName;
            sub.item = rest;
            sub.screenshot = scren;
            sub.physicalDistance = cameraCanvas.GetPhysicalDistance(rest);
            sub.onScreen = isOnScreen;
            sub.motion = 100 - (int)Mathf.Clamp(motion, 0, 100);
            sub.centerFrame = lockOnSystem.GetCrosshairCenter(rest);
            sub.focus = cameraCanvas.GetFocusValue();
            sub.objectSharpness = lockOnSystem.GetObjectSharpness(rest);
            score.subScore.Add(sub);
        }

        ratingSystem.VisualizeScore(score, scren);
        photoName +=  " [<color='#" + ColorUtility.ToHtmlStringRGB(ratingSystem.scoreGradient.Evaluate(scren.score / 100f)) + "'>" + scren.score + "</color>]";

        ratingSystem.SetPolaroidTitle(photoWithoutScore);
        sf.text.text = score.name = photoName;

        photoBookShots.Add(pol);
        screenshots.Add(scren);

        if(screenshots.Count >= maxPhotosInPortfolio) {
            Destroy(screenshots[0].image);
            screenshots.RemoveAt(0);
            photoBookShots.RemoveAt(0);
            Destroy(photoBookShots[0]);
        }

        SoundManager.PlayUnscaledSound("PhotoShoot", 0.8f);
        ratingSystem.ResetScreenshot();
    }
}