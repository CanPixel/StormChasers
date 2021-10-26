using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Cinemachine.PostFX;

public class CameraControl : MonoBehaviour {
    public GameObject[] enableOnFirstPerson, disableOnFirstPerson;
    public Graphic[] fadeOnFirstPerson;
    private Color[] baseAlphaFirstPersonFade;
    public CameraSystem camSystem;

    [Header("Portfolio")]
    public int maxPhotosInPortfolio = 5;
    public Color markedPictureColor;

    [Header("Boost Cam")]
    public Vector3 BoostFollowOffset = new Vector3(0, 2.8f, -6);
    public float BoostFollowDamping = 2f;

    private float recenterTime = 0;
    [Space(5)]
    public float recenterDuration = 1f;

    [Header("Physical Discarded Picture")]
    public float DiscardForce = 10f;
    public float physPicScale = 0.5f;
    private Vector3 basePicScale;

    [HideInInspector] public bool photoBook = false;

    public int resWidth = 1600, resHeight = 900;

    [Header("SlowMo")]
    public float slowMotionDamping = 3f;
    public float slowMotionTime = 0.1f;

    public enum CameraState {
        CARVIEW = 0, CAMVIEW, DASHVIEW
    }
    [HideInInspector] public CameraState cameraState = CameraState.CARVIEW;

    public Vector3 mascotteRotationOffset = new Vector3(-90, 90, 0);
    [Header("Y Axis Look")]
    public Vector2 lookYLimits = new Vector2(0.6f, 1.4f);
    public float yLookSensitivity = 10, lookUpCameraSensitivity = 15;

    [System.Serializable]
    public class PictureScore {
        public Screenshot screenshot;

        public PhotoBase item;
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
        public GameObject portfolioObj;
        public bool forMission = false;

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
        [ReadOnly] public float aim, shoot;

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

    [HideInInspector] public List<Screenshot> screenshots = new List<Screenshot>();

    [Space(10)]
    public LayerMask thirdPersonCull;
    public LayerMask firstPersonCull;
    public Camera cam;
    public Cinemachine.CinemachineVirtualCamera firstPersonLook;
    public Cinemachine.CinemachineVirtualCamera backLook;
    public Cinemachine.CinemachineVirtualCamera thirdPersonLook;
    private Cinemachine.CinemachinePOV pov;
    private Cinemachine.CinemachineOrbitalTransposer orbitalTransposer;
    private Cinemachine.CinemachineComposer composer;
    private float baseScreenY;
    public Cinemachine.CinemachineBrain cinemachineBrain;
    public GameObject photoBookUI, polaroidPrefab;
    public GameObject cameraMascotte;
    public SpriteRenderer missionPicture;
    public Transform photoBookScrollPanel;
    public RawImage photoBookSelection;
    public GameObject physicalPhotoPrefab;
    public Text photoBookCapacity;
    public Text discardPicText, markPicText;
    [SerializeField] private InputActionReference discardPicture;
    [SerializeField] private InputActionReference markPicture;
    public UIBob reticleBob;
    public Image worldAimReticle;
    public Text worldAimControlText;
    public GameObject minimapCamera;
    public CarMovement carMovement;
    public CameraCanvas cameraCanvas;
    public LockOnSystem lockOnSystem;
    public SplashSystem splashSystem;
    public RatingSystem ratingSystem;
    public MissionManager missionManager;
    [SerializeField] private InputActionReference cameraAimButton;
    public ShaderReel shaderReel;
    public bool raceCamera = false;

    private Vector3 baseFollowOffset;
    private float baseZDamping;
    private float minimapCamBaseAngle, minimapCamBaseY;

    private float blend = 0;
    private int currentSelectedPortfolioPhoto = 0;

    void Start() {
        pov = firstPersonLook.GetCinemachineComponent<Cinemachine.CinemachinePOV>();
        orbitalTransposer = thirdPersonLook.GetCinemachineComponent<Cinemachine.CinemachineOrbitalTransposer>();
        composer = thirdPersonLook.GetCinemachineComponent<Cinemachine.CinemachineComposer>();
        baseScreenY = composer.m_ScreenY;
        foreach(var i in enableOnFirstPerson) i.SetActive(false); 

        baseFollowOffset = orbitalTransposer.m_FollowOffset;
        baseZDamping = orbitalTransposer.m_ZDamping;

        minimapCamBaseAngle = minimapCamera.transform.eulerAngles.x;
        minimapCamBaseY = minimapCamera.transform.position.y;

        baseAlphaFirstPersonFade = new Color[fadeOnFirstPerson.Length];
        for(int i = 0; i < fadeOnFirstPerson.Length; i++) baseAlphaFirstPersonFade[i] = fadeOnFirstPerson[i].color;

        ratingSystem.gameObject.SetActive(true);

        basePicScale = missionPicture.transform.localScale;
        missionPicture.gameObject.SetActive(false);
    }

    private Vector3 worldReticleTarget;
    private float distTarget = 1f;

    void Update() {
        minimapCamera.transform.position = new Vector3(transform.position.x, minimapCamBaseY, transform.position.z);
        minimapCamera.transform.rotation = Quaternion.Euler(minimapCamBaseAngle, transform.eulerAngles.y, 0);

        if(isLookingBack && raceCamera) RecenterY();

        /* Blending Shaders */
        float blendTarget;
        if(cinemachineBrain.IsLive(firstPersonLook) || cinemachineBrain.IsBlending) blendTarget = 1;
        else blendTarget = -0.1f;
        blend = Mathf.Lerp(blend, blendTarget, Time.unscaledDeltaTime * 8f);
        cameraCanvas.postProcessVolume.weight = blend;

        /* 3D World Aim Reticle */
        worldAimControlText.text = "Aim (<color='#ff0000'>" + cameraAimButton.action.GetBindingDisplayString() + "</color>)";
        RaycastHit hit;
        if(Physics.Raycast(cameraMascotte.transform.position + Vector3.up, -cameraMascotte.transform.up, out hit, 200)) {
            worldReticleTarget = cam.WorldToScreenPoint(hit.point);
            distTarget = Mathf.Clamp(1f - ((hit.transform.position - transform.position).magnitude) / 100f, 0.2f, 1);
        }
        if(distTarget <= 0.21f || isLookingBack) distTarget = 0;
        worldAimReticle.transform.position = Vector3.Lerp(worldAimReticle.transform.position, worldReticleTarget, Time.unscaledDeltaTime * 4f);
        worldAimReticle.transform.localScale = Vector3.Lerp(worldAimReticle.transform.localScale, Vector3.one * distTarget, Time.unscaledDeltaTime * 4f);

        photoBookUI.SetActive(photoBook);

        /* SlowMo */
        Time.timeScale = Mathf.Lerp(Time.timeScale, (IsAiming() || (ratingSystem.HasTakenPicture() && !ratingSystem.IsFading())) ? slowMotionTime : 1.0f, Time.unscaledDeltaTime * slowMotionDamping * (!IsAiming() ? 4f : 1f));
        SoundManager.SlowMo();

        if(recenterTime > 0 && Mathf.Abs(orbitalTransposer.m_XAxis.Value) > 1) {
            recenterTime -= Time.deltaTime * 2f;
            orbitalTransposer.m_XAxis.Value = Mathf.Lerp(orbitalTransposer.m_XAxis.Value, 0, Time.deltaTime * 6f);
            composer.m_ScreenY = Mathf.Lerp(composer.m_ScreenY, baseScreenY, Time.deltaTime * 6f);
        }
        if(carMovement.GetLooking().magnitude >= 0.65f && recenterTime > 0) recenterTime = 0;

        if(IsAiming()) FirstPersonLook();
        else ThirdPersonLook();
        for(int i = 0; i < fadeOnFirstPerson.Length; i++) fadeOnFirstPerson[i].color = Color.Lerp(fadeOnFirstPerson[i].color, new Color(fadeOnFirstPerson[i].color.r, fadeOnFirstPerson[i].color.g, fadeOnFirstPerson[i].color.b, (IsAiming() ? 0 : baseAlphaFirstPersonFade[i].a)), Time.unscaledDeltaTime * 8f);

        cameraMascotte.transform.rotation = transform.rotation;
        cameraMascotte.transform.Rotate(mascotteRotationOffset.x + (baseScreenY - composer.m_ScreenY) * lookUpCameraSensitivity, orbitalTransposer.m_XAxis.Value, mascotteRotationOffset.z);
        cameraMascotte.transform.localScale = Vector3.Lerp(cameraMascotte.transform.localScale, Vector3.one, Time.deltaTime * 5f);
    
        /* PhotoBook / Portfolio */
        photoBookCapacity.text = screenshots.Count + "/" + maxPhotosInPortfolio;
        photoBookSelection.enabled = discardPicText.enabled = screenshots.Count > 0;
        markPicText.enabled = screenshots.Count > 0 && screenshots[currentSelectedPortfolioPhoto].forMission;
        if(screenshots.Count > 0) photoBookSelection.transform.position = screenshots[currentSelectedPortfolioPhoto].portfolioObj.transform.position;
        discardPicText.text = "Discard  (<color='#ff0000'>" + discardPicture.action.GetBindingDisplayString() + "</color>)";
        markPicText.text = "Mark for mission  (<color='#ffff00'>" + markPicture.action.GetBindingDisplayString() + "</color>)";
    }

    public void MarkPicture() {
        screenshots[currentSelectedPortfolioPhoto].portfolioObj.GetComponent<Image>().color = markedPictureColor;
        missionPicture.sprite = Sprite.Create(screenshots[currentSelectedPortfolioPhoto].image, new Rect(0, 0, resWidth, resHeight), Vector2.one * 0.5f);
        missionPicture.transform.localScale = basePicScale * physPicScale;
        missionPicture.gameObject.SetActive(true);
        missionManager.MarkCurrentObjective();
    }
    public void DiscardPicture() {
        var obj = Instantiate(physicalPhotoPrefab, transform.position + Vector3.up * 2f, Quaternion.Euler(10,0,0));
        obj.GetComponent<Rigidbody>().AddForce(Vector3.up * DiscardForce * 50f);
        obj.transform.localScale = Vector3.one * 0.15f;
        obj.GetComponent<SpriteRenderer>().sprite = Sprite.Create(screenshots[currentSelectedPortfolioPhoto].image, new Rect(0, 0, resWidth, resHeight), Vector2.one * 0.5f);

        DeletePicture(currentSelectedPortfolioPhoto, false);

        PortfolioSelection(-1);
        photoBook = false;
    }
    
    private bool isLookingBack = false;
    public void BackLook(bool lookBack) {
        if(!raceCamera) return;
        backLook.m_Priority = (lookBack) ? 15 : 2;
        isLookingBack = lookBack;
    }

    protected bool IsAiming() {
        return camSystem.aim >= 0.5f;
    }

    public void CycleFilters(float add) {
        camSystem.filterIndex += (int)add;
        if(camSystem.filterIndex >= camSystem.shaderFilters.Length) camSystem.filterIndex = 0;
        if(camSystem.filterIndex < 0) camSystem.filterIndex = camSystem.shaderFilters.Length - 1;
        
        cameraCanvas.postProcessVolume.sharedProfile = camSystem.shaderFilters[camSystem.filterIndex].profile;
        shaderReel.current = camSystem.filterIndex;
        cameraCanvas.ReloadFX();

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

        var yOffs = carMovement.GetLooking().y;
        composer.m_ScreenY = Mathf.Clamp(composer.m_ScreenY + yOffs * yLookSensitivity * Time.unscaledDeltaTime, lookYLimits.x, lookYLimits.y);

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
        composer.m_ScreenY = baseScreenY;
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
        List<PhotoBase> restItems = new List<PhotoBase>();

        PhotoBase i = null;
        Vector3 targetPos = Vector3.zero;
        bool isOnScreen = true;
        if(allObjects.Count > 0) {
            i = allObjects[0];
            if(i != null) {
                targetPos = cam.WorldToViewportPoint(i.transform.position);
                isOnScreen = (targetPos.z > 0 && targetPos.x > 0 && targetPos.x < 1 && targetPos.y > 0 && targetPos.y < 1) ? true : false;
            }
        }

        //Main Object score
        if(isOnScreen) {
            photoName = photoWithoutScore = (i != null) ? i.name : cameraCanvas.RaycastName(cameraCanvas.baseReticle.transform).Replace('(', ' ').Replace(')', ' ').Replace('_', ' ').Trim();
            score.item = i;
            score.name = photoName;
            score.screenshot = scren;
            score.physicalDistance = cameraCanvas.GetPhysicalDistance(i);
            score.onScreen = isOnScreen;
            score.motion = 100 - (int)Mathf.Clamp(cameraCanvas.GetMotion(), 0, 100);
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
            sub.motion = 100 - (int)Mathf.Clamp(cameraCanvas.GetMotion(), 0, 100);
            sub.centerFrame = lockOnSystem.GetCrosshairCenter(rest);
            sub.focus = cameraCanvas.GetFocusValue();
            sub.objectSharpness = lockOnSystem.GetObjectSharpness(rest);
            score.subScore.Add(sub);
        }

        //Visualize
        ratingSystem.VisualizeScore(score, scren);
        photoName +=  " [<color='#" + ColorUtility.ToHtmlStringRGB(ratingSystem.scoreGradient.Evaluate(scren.score / 100f)) + "'>" + scren.score + "</color>]";

        //Mission check
        scren.portfolioObj = pol;
        missionManager.CheckCompletion(score, scren);

        ratingSystem.SetPolaroidTitle(photoWithoutScore);
        sf.text.text = score.name = photoName;

        //Portfolio
        if(screenshots.Count >= maxPhotosInPortfolio) DeletePicture(0);

    //    photoBookShots.Add(pol);
        screenshots.Add(scren);

        //Physical Pictures        
        //foreach(var pic in physPictures) {
          //  pic.sprite = Sprite.Create(screenshots[Random.Range(0, screenshots.Count)].image, new Rect(0, 0, resWidth, resHeight), Vector2.one * 0.5f);
          //  pic.transform.localScale = basePicScale * physPicScale;
        //}

        SoundManager.PlayUnscaledSound("PhotoShoot", 0.8f);
        ratingSystem.ResetScreenshot();
    }

    public void PortfolioSelection(float delta) {
        if(delta >= 0.5f) currentSelectedPortfolioPhoto++;
        else currentSelectedPortfolioPhoto--;
        currentSelectedPortfolioPhoto = Mathf.Clamp(currentSelectedPortfolioPhoto, 0, screenshots.Count - 1);
    }
    
    public void DeletePicture(int index, bool deleteTexture = true) {
        Destroy(screenshots[0].portfolioObj);
        if(deleteTexture) Destroy(screenshots[0].image);
        screenshots.RemoveAt(0);
    }
}