using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Cinemachine.PostFX;
using System.Linq;

public class CameraControl : MonoBehaviour
{
    public GameObject[] enableOnFirstPerson, disableOnFirstPerson;
    public Graphic[] fadeOnFirstPerson;
    private Color[] baseAlphaFirstPersonFade;
    public CameraSystem camSystem;

    [Header("On Picture Shot")]
    public AnimationCurve pictureShotAlphaFade = AnimationCurve.Linear(0, 1, 1, 0);
    public float pictureDelayUntilNext = 1;
    private float pictureShotTimer = 0;

    [Header("Portfolio")]
    public int maxPhotosInPortfolio = 10;
    public float portfolioPictureSpacing = 455;
    public float portfolioPictureYOffs = 343.4f;
    public Color activeMissionColor, inactiveMissionColor;
    public Color markedPictureColor;
    public Color eligibleForMissionPictureColor;

    [Header("Boost Cam")]
    public AnimationCurve zPosOffs;
    public Vector3 BoostFollowOffset = new Vector3(0, 3f, -9f);
    public float BoostFollowDamping = 6f;
    public float recenterVelocityFactor = 2f;

    [Header("Physical Discarded Picture")]
    public float DiscardForce = 100f;
    public float physPicScale = 0.35f;
    private Vector3 basePicScale;

    [HideInInspector] public bool journal = false, photobook = false;

    public int resWidth = 1920, resHeight = 1080;

    [Header("SlowMo")]
    public float slowMotionDamping = 3f;
    public float slowMotionTime = 0.25f;

    public enum CameraState
    {
        CARVIEW = 0, CAMVIEW
    }
    [HideInInspector] public CameraState cameraState = CameraState.CARVIEW;

    public Vector3 mascotteRotationOffset = new Vector3(-90, 0, 0);
    [Header("Y Axis Look")]
    public Vector2 lookYLimits = new Vector2(0.65f, 1.1f);
    public float yLookSensitivity = 1.5f, lookUpCameraSensitivity = 70;
    public AnimationCurve yPosOffs;

    [System.Serializable]
    public class ObjectProperties
    {
        public Screenshot screenshot;

        public List<ObjectProperties> subScore = new List<ObjectProperties>();

        public string name;
        public bool onScreen;
        public float physicalDistance;
        public float centerFrame;
        public float focus, motion;
        public float objectSharpness;

        public override string ToString()
        {
            return "[" + name + "]  || Center Frame: " + centerFrame + " || Motion Stability: " + motion + " || Sharpness: " + objectSharpness + " [Focus: " + focus + ", PhysDist: " + physicalDistance + "]";
        }
    }

    [System.Serializable]
    public class Screenshot
    {
        public string name;
        public Texture2D image;
        public GameObject portfolioObj;
        [HideInInspector] public Image polaroidFrame;
        [HideInInspector] public Color baseColor = new Color(0.9f, 0.9f, 0.9f, 1);
        public bool forMission = false;

        public PicturedObject[] picturedObjects;
        public string containedObjectTags;

        [System.Serializable]
        public class PicturedObject
        {
            public string tag;
            public PhotoBase objectReference;

            public int sensation;

            public ObjectProperties score;

            public PicturedObject(string tag, PhotoBase reference)
            {
                this.tag = tag;
                this.objectReference = reference;
            }
        }

        [Range(0, 100)]
        public int score = 0;

        public static Screenshot Create(string name, Texture2D sprite)
        {
            Screenshot screen = new Screenshot();
            screen.name = name;
            screen.image = sprite;
            return screen;
        }
    }

    [System.Serializable]
    public class CameraSystem
    {
        [ReadOnly] public float aim, shoot;

        [System.Serializable]
        public class ShaderFilter
        {
            public string name;
            public PostProcessProfile profile;
            public Sprite icon;
        }

        public ShaderFilter[] shaderFilters;

        [HideInInspector] public int filterIndex = 0;

        public override string ToString()
        {
            return "Aim: " + aim + " || Shoot: " + shoot;
        }
    }

    public List<Screenshot> screenshots = new List<Screenshot>();
    private Screenshot markedScreenshot;

    [Space(10)]
    public LayerMask thirdPersonCull;
    public LayerMask firstPersonCull;
    public LayerMask midPersonCull;
    public Camera cam;
    public Cinemachine.CinemachineVirtualCamera firstPersonLook;
    public Cinemachine.CinemachineVirtualCamera backLook;
    public Cinemachine.CinemachineVirtualCamera thirdPersonLook;
    private Cinemachine.CinemachinePOV pov;
    private Cinemachine.CinemachineOrbitalTransposer orbitalTransposer;
    private Cinemachine.CinemachineComposer composer;
    private float baseScreenY;
    public Text scoreCounter;
    private int totalScore = 0;
    public Cinemachine.CinemachineBrain cinemachineBrain;
    public GameObject photoBookUI, polaroidPrefab;
    public GameObject cameraMascotte;
    public SpriteRenderer missionPicture;
    public Transform photoBookScrollPanel;
    public Image pictureShotOverlay;
    public RawImage photoBookSelection;
    public GameObject physicalPhotoPrefab;
    public Text photoBookCapacity;
    public UIBob reticleBob;
    public GameObject minimapCamera;
    public CarMovement carMovement;
    public CameraCanvas cameraCanvas;
    public LockOnSystem lockOnSystem;
    public Camera3D cam3D;
    public RatingSystem ratingSystem;
    [SerializeField] private InputActionReference cameraAimButton;

    public GameObject yInfoButton;

    public GameObject shaders;
    public ShaderReel shaderReel;
    public GameObject cycleFilterYButton;

    private Vector3 baseFollowOffset;
    private float baseZDamping;
    private float minimapCamBaseAngle, minimapCamBaseY;

    private int completedMissions = 0;

    private float blend = 0;
    private int currentSelectedPortfolioPhoto = 0;

    private float takePictureDelay = 0;

    [System.Serializable]
    public class JournalMission
    {
        public GameObject journalElement;
        public Text journalTitle;
        public Sprite finalPicture;
        public bool active = false;
    }
    private List<JournalMission> journalMissions = new List<JournalMission>();
    private int journalSelectedMission = 0;

    private float portfolioPictureBaseScale, portfolioTargetY = 0;

    void Start()
    {
        pov = firstPersonLook.GetCinemachineComponent<Cinemachine.CinemachinePOV>();
        orbitalTransposer = thirdPersonLook.GetCinemachineComponent<Cinemachine.CinemachineOrbitalTransposer>();
        composer = thirdPersonLook.GetCinemachineComponent<Cinemachine.CinemachineComposer>();
        baseScreenY = composer.m_ScreenY;
        foreach (var i in enableOnFirstPerson) i.SetActive(false);

        baseFollowOffset = orbitalTransposer.m_FollowOffset;
        baseZDamping = orbitalTransposer.m_ZDamping;

        pictureShotTimer = pictureShotAlphaFade.Evaluate(pictureShotAlphaFade.length);
        pictureShotOverlay.gameObject.SetActive(true);

        minimapCamBaseAngle = minimapCamera.transform.eulerAngles.x;
        minimapCamBaseY = minimapCamera.transform.position.y;

        portfolioPictureBaseScale = polaroidPrefab.transform.localScale.x;

        baseAlphaFirstPersonFade = new Color[fadeOnFirstPerson.Length];
        for (int i = 0; i < fadeOnFirstPerson.Length; i++) baseAlphaFirstPersonFade[i] = fadeOnFirstPerson[i].color;

        ratingSystem.gameObject.SetActive(true);

        basePicScale = missionPicture.transform.localScale;
        missionPicture.gameObject.SetActive(false);
        photobook = false;

        photoBookUI.SetActive(false);
    }

    private float cameraMascotteBackLook = 0;
    void Update()
    {
        scoreCounter.text = totalScore.ToString();

        if (takePictureDelay > 0) takePictureDelay -= Time.unscaledDeltaTime;

        minimapCamera.transform.position = new Vector3(transform.position.x, minimapCamBaseY, transform.position.z);
        minimapCamera.transform.rotation = Quaternion.Euler(minimapCamBaseAngle, transform.eulerAngles.y, 0);

        pictureShotTimer += Time.unscaledDeltaTime;
        pictureShotOverlay.color = new Color(pictureShotOverlay.color.r, pictureShotOverlay.color.g, pictureShotOverlay.color.b, pictureShotAlphaFade.Evaluate(pictureShotTimer));

        if (isLookingBack) RecenterY();

        /* Blending Shaders */
        float blendTarget;
        if (cinemachineBrain.IsLive(firstPersonLook) || cinemachineBrain.IsBlending) blendTarget = 1;
        else blendTarget = -0.1f;
        blend = Mathf.Lerp(blend, blendTarget, Time.unscaledDeltaTime * 8f);
        cameraCanvas.postProcessVolume.weight = blend;

        /* SlowMo */
        Time.timeScale = Mathf.Lerp(Time.timeScale, IsAiming() ? slowMotionTime : 1.0f, Time.unscaledDeltaTime * slowMotionDamping * (!IsAiming() ? 4f : 1f));
        SoundManager.SlowMo();

        /*         if(recenterTime > 0 && Mathf.Abs(orbitalTransposer.m_XAxis.Value) > 1) {
                    recenterTime -= Time.deltaTime * 2f;
                    orbitalTransposer.m_XAxis.Value = Mathf.Lerp(orbitalTransposer.m_XAxis.Value, 0, Time.deltaTime * 6f);
                    composer.m_ScreenY = Mathf.Lerp(composer.m_ScreenY, baseScreenY, Time.deltaTime * 6f); 
                } */
        //if(carMovement.GetLooking().magnitude >= 0.65f && recenterTime > 0) recenterTime = 0;

        orbitalTransposer.m_RecenterToTargetHeading.m_RecenteringTime = (100f - carMovement.rb.velocity.magnitude) / recenterVelocityFactor;

        if (IsAiming()) FirstPersonLook();
        else ThirdPersonLook();
        for (int i = 0; i < fadeOnFirstPerson.Length; i++) fadeOnFirstPerson[i].color = Color.Lerp(fadeOnFirstPerson[i].color, new Color(fadeOnFirstPerson[i].color.r, fadeOnFirstPerson[i].color.g, fadeOnFirstPerson[i].color.b, (IsAiming() ? 0 : baseAlphaFirstPersonFade[i].a)), Time.unscaledDeltaTime * 8f);

        cameraMascotte.transform.localScale = Vector3.Lerp(cameraMascotte.transform.localScale, Vector3.one, Time.deltaTime * 5f);

        /* PhotoBook / Portfolio */
        photoBookCapacity.text = screenshots.Count + "/" + maxPhotosInPortfolio;
        photoBookSelection.enabled = screenshots.Count > 0;
        if (screenshots.Count > 0 && currentSelectedPortfolioPhoto < screenshots.Count)
        {
            var screen = screenshots[currentSelectedPortfolioPhoto].portfolioObj.transform;
            photoBookSelection.transform.position = screen.position;
            screen.localScale = Vector3.Lerp(screen.localScale, Vector3.one, Time.unscaledDeltaTime * 10f);
            photoBookSelection.transform.localScale = screen.localScale;
        }

        currentSelectedPortfolioPhoto = Mathf.Clamp(currentSelectedPortfolioPhoto, 0, Mathf.Clamp(screenshots.Count - 1, 0, screenshots.Count));
        portfolioTargetY = currentSelectedPortfolioPhoto * portfolioPictureSpacing - portfolioPictureYOffs;

        cycleFilterYButton.SetActive(!journal && camSystem.aim >= 0.5f);
        shaders.SetActive(camSystem.aim >= 0.5f);
    }

    public void MarkPicture()
    {
        if (currentSelectedPortfolioPhoto >= screenshots.Count || screenshots[currentSelectedPortfolioPhoto] == null || !screenshots[currentSelectedPortfolioPhoto].forMission || screenshots.Count <= 0) return;

        if (AlreadyMarked())
        {
            UnmarkPicture();
            return;
        }

        screenshots[currentSelectedPortfolioPhoto].polaroidFrame.color = markedPictureColor;
        missionPicture.sprite = Sprite.Create(screenshots[currentSelectedPortfolioPhoto].image, new Rect(0, 0, resWidth, resHeight), Vector2.one * 0.5f);
        missionPicture.transform.localScale = basePicScale * physPicScale;
        missionPicture.gameObject.SetActive(true);
        markedScreenshot = screenshots[currentSelectedPortfolioPhoto];
    }
    public void UnmarkPicture()
    {
        missionPicture.sprite = null;
        missionPicture.gameObject.SetActive(false);
        Destroy(missionPicture.sprite);
        if (screenshots.Count <= 0 || currentSelectedPortfolioPhoto >= screenshots.Count || screenshots[currentSelectedPortfolioPhoto] == null) return;
        screenshots[currentSelectedPortfolioPhoto].polaroidFrame.color = screenshots[currentSelectedPortfolioPhoto].baseColor;
        markedScreenshot = null;
    }
    public void DiscardPicture()
    {
        if (currentSelectedPortfolioPhoto >= screenshots.Count || screenshots[currentSelectedPortfolioPhoto] == null || screenshots.Count <= 0) return;
        var obj = Instantiate(physicalPhotoPrefab, transform.position + Vector3.up * 2f, Quaternion.Euler(10, 0, 0));
        obj.GetComponent<Rigidbody>().AddForce(Vector3.up * DiscardForce * 50f);
        obj.transform.localScale = Vector3.one * 0.15f;
        obj.GetComponent<SpriteRenderer>().sprite = Sprite.Create(screenshots[currentSelectedPortfolioPhoto].image, new Rect(0, 0, resWidth, resHeight), Vector2.one * 0.5f);

        //if(screenshots[currentSelectedPortfolioPhoto].forMission) missionManager.DiscardMissionPicture();

        DeletePicture(currentSelectedPortfolioPhoto, false);
        PortfolioSelection();
        UnmarkPicture();
        //missionManager.ShowReadyForMark(false);
    }
    protected bool AlreadyMarked()
    {
        return currentSelectedPortfolioPhoto < screenshots.Count && markedScreenshot != null && screenshots[currentSelectedPortfolioPhoto] != null && markedScreenshot == screenshots[currentSelectedPortfolioPhoto];
    }

    public void DeliverReset()
    {
        deliverTime = 0;
        deliverPicture = null;
    }

    private GameObject deliverPicture = null;
    private Transform deliverTo;
    private float deliverTime = 0;
    public void DeliverPicture(Transform pos)
    {
        if (screenshots.Count <= 0 || screenshots[currentSelectedPortfolioPhoto] == null || deliverPicture != null) return;

        deliverPicture = Instantiate(physicalPhotoPrefab, transform.position + Vector3.up * 2f, Quaternion.Euler(10, 0, 0));
        var rb = deliverPicture.GetComponent<Rigidbody>();
        rb.AddForce(Vector3.up * DiscardForce * 50f);
        rb.useGravity = false;
        deliverPicture.transform.localScale = Vector3.one * 0.15f;
        var spr = Sprite.Create(screenshots[currentSelectedPortfolioPhoto].image, new Rect(0, 0, resWidth, resHeight), Vector2.one * 0.5f);

        deliverPicture.GetComponent<SpriteRenderer>().sprite = spr;
        journalMissions[journalSelectedMission].finalPicture = spr;

        DeletePicture(currentSelectedPortfolioPhoto, false);
        currentSelectedPortfolioPhoto = 0;
        UnmarkPicture();

        journal = false;
        deliverTo = pos;
        deliverTime = 0;
    }

    private Sprite lastMissionSprite;

    private bool isLookingBack = false;
    public void BackLook(bool lookBack)
    {
        backLook.m_Priority = (lookBack) ? 15 : 2;
        isLookingBack = lookBack;
    }

    protected bool IsAiming()
    {
        return camSystem.aim >= 0.5f;
    }

    public void CycleFilters(float add)
    {
        camSystem.filterIndex += (int)add;
        if (camSystem.filterIndex >= camSystem.shaderFilters.Length) camSystem.filterIndex = 0;
        if (camSystem.filterIndex < 0) camSystem.filterIndex = camSystem.shaderFilters.Length - 1;

        cameraCanvas.postProcessVolume.sharedProfile = camSystem.shaderFilters[camSystem.filterIndex].profile;
        shaderReel.current = camSystem.filterIndex;
        cameraCanvas.ReloadFX();

        SoundManager.PlayUnscaledSound("ShaderSwitch");
        reticleBob.Bob();
    }

    private bool canTakePicture = true;
    protected void FirstPersonLook()
    {
        if (cinemachineBrain.IsLive(firstPersonLook))
        {
            if (!cinemachineBrain.IsBlending) cam.cullingMask = firstPersonCull;
            else cam.cullingMask = midPersonCull;
        }
        else cam.cullingMask = thirdPersonCull;

        firstPersonLook.Priority = 16;
        thirdPersonLook.Priority = 10;

        if (cameraState != CameraState.CAMVIEW)
        {
            foreach (var i in enableOnFirstPerson) i.SetActive(true);
            foreach (var i in disableOnFirstPerson) i.SetActive(false);
            cameraState = CameraState.CAMVIEW;
        }

        SynchTransposer();

        if (camSystem.shoot < 0.4f) canTakePicture = true;
        if (camSystem.shoot >= 0.5f && canTakePicture) TakePicture();
    }
    protected void ThirdPersonLook()
    {
        cam.cullingMask = thirdPersonCull;
        firstPersonLook.Priority = 10;
        thirdPersonLook.Priority = 12;

        if (cameraState != CameraState.CARVIEW)
        {
            foreach (var i in enableOnFirstPerson) i.SetActive(false);
            foreach (var i in disableOnFirstPerson) i.SetActive(true);
            cameraState = CameraState.CARVIEW;
        }

        cameraMascotte.transform.rotation = transform.rotation;
        cameraMascotteBackLook = Mathf.Lerp(cameraMascotteBackLook, isLookingBack ? 180 : 0, Time.deltaTime * 4f);
        cameraMascotte.transform.Rotate(mascotteRotationOffset.x + (baseScreenY - composer.m_ScreenY) * lookUpCameraSensitivity, orbitalTransposer.m_XAxis.Value + cameraMascotteBackLook, mascotteRotationOffset.z);

        var yOffs = carMovement.GetLooking().y;
        composer.m_ScreenY = Mathf.Clamp(composer.m_ScreenY + yOffs * yLookSensitivity * Time.unscaledDeltaTime, lookYLimits.x, lookYLimits.y);

        pov.m_HorizontalAxis.Value = cameraMascotte.transform.localEulerAngles.z + transform.eulerAngles.y;

        orbitalTransposer.m_FollowOffset = Vector3.Lerp(orbitalTransposer.m_FollowOffset, (carMovement.boostScript.isBoosting) ? BoostFollowOffset + new Vector3(0, yPosOffs.Evaluate(composer.m_ScreenY), zPosOffs.Evaluate(composer.m_ScreenY)) : baseFollowOffset + new Vector3(0, yPosOffs.Evaluate(composer.m_ScreenY), zPosOffs.Evaluate(composer.m_ScreenY)), Time.deltaTime * BoostFollowDamping);
        orbitalTransposer.m_ZDamping = Mathf.Lerp(orbitalTransposer.m_ZDamping, (carMovement.boostScript.isBoosting) ? 0.05f : baseZDamping, Time.deltaTime * BoostFollowDamping / 2f);
    }

    public void SynchTransposer()
    {
        //mascotteRotationOffset.x + (baseScreenY - composer.m_ScreenY) * lookUpCameraSensitivity

        //pov.m_VerticalAxis.Value - x axis
        cameraMascotte.transform.rotation = Quaternion.Euler(cameraMascotte.transform.eulerAngles.x, cameraMascotte.transform.eulerAngles.y, transform.eulerAngles.y);
        cameraMascotte.transform.localRotation = Quaternion.Euler(0, cameraMascotte.transform.localEulerAngles.y, pov.m_HorizontalAxis.Value + cameraMascotteBackLook);
        orbitalTransposer.m_XAxis.Value = pov.m_HorizontalAxis.Value - transform.eulerAngles.y;
    }

    public void AnimateCameraMascotte()
    {
        cameraMascotte.transform.localScale = Vector3.one * 1.2f;
    }

    public void Recenter()
    {
        pov.m_VerticalAxis.Value = 0;
        //recenterTime = recenterDuration;
    }
    public void RecenterY()
    {
        pov.m_VerticalAxis.Value = 0;
        composer.m_ScreenY = baseScreenY;
    }

    public bool HasTakenPicture()
    {
        return ratingSystem.HasTakenPicture();
    }

    public Sprite GetLastScreenshot()
    {
        return lastMissionSprite;
    }

    protected void TakePicture()
    {
        if (takePictureDelay > 0 || !canTakePicture) return;

        canTakePicture = false;
        pictureShotTimer = 0;
        HapticManager.Haptics("TakePicture");

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
        pol.transform.localPosition = Vector3.zero;
        pol.transform.localScale = polaroidPrefab.transform.localScale;
        var sf = pol.GetComponent<ShaderFilter>();
        sf.icon.sprite = temp;

        //=============== [Photo Naming - Object Section]
        var allObjects = lockOnSystem.GetOnScreenObjects();
        string photoName = "";
        string objectTags = "";

        Vector3 targetPos = Vector3.zero;
        scren.picturedObjects = new Screenshot.PicturedObject[allObjects.Count];
        int index = 0;

        string pluralName = "";
        Dictionary<string, int> tagsOnPicture = new Dictionary<string, int>();
        int sensationScore = 0;
        foreach (var rest in allObjects)
        {
            bool isOnScreen = true;
            string tag = rest.tag.ToUpper().Trim();
            if (tagsOnPicture.ContainsKey(tag))
            {
                pluralName = tag + "s";
                tagsOnPicture[tag] += rest.sensation;
            }
            else tagsOnPicture.Add(tag, rest.sensation);

            //Debug.Log(tag + ": " + rest.sensation);

            objectTags += tag + " ";
            sensationScore += rest.sensation;

            //Debug.Log(rest.name);
            if (rest.CompareTag("ActivationExplosive"))
            {
                TriggerExplosion explosive = rest.gameObject.GetComponent<TriggerExplosion>();
                explosive.hasBeenActivated = true;
                explosive.explosionDelayDuration = 0f;
            }

            scren.picturedObjects[index] = new Screenshot.PicturedObject(rest.tag, rest);

            ObjectProperties sub = new ObjectProperties();
            targetPos = cam.WorldToViewportPoint(rest.transform.position);
            isOnScreen = (targetPos.z > 0 && targetPos.x > 0 && targetPos.x < 1 && targetPos.y > 0 && targetPos.y < 1) ? true : false;

            photoName = tag;
            sub.name = photoName;
            sub.screenshot = scren;
            sub.physicalDistance = cameraCanvas.GetPhysicalDistance(rest);
            sub.onScreen = isOnScreen;
            sub.motion = 100 - (int)Mathf.Clamp(cameraCanvas.GetMotion(), 0, 100);
            sub.centerFrame = lockOnSystem.GetCrosshairCenter(rest);
            sub.focus = cameraCanvas.GetFocusValue();
            sub.objectSharpness = lockOnSystem.GetObjectSharpness(rest);

            scren.picturedObjects[index].score = sub;
            scren.picturedObjects[index].sensation = rest.sensation;
            index++;
        }
        scren.containedObjectTags = objectTags;
        scren.score = sensationScore;
        totalScore += sensationScore;

        if (totalScore > 0) SplashSystem.self.SpawnSplash(ratingSystem.GetScoreLabel(totalScore), new Vector3(0, 1, 0), 2, 1, 3, TextAnchor.MiddleCenter);

        //Visualize
        cam3D.DoPicture(scren, resWidth, resHeight);
        lockOnSystem.VisualizePictureItems(scren);
        ratingSystem.VisualizeScore(scren, ratingSystem.scoreGradient, sensationScore);

        //Find highest sensation scoring tag and name picture accordingly
        var sortedBySensation = tagsOnPicture.OrderByDescending(x => x.Value).Select(x => x.Key).ToList();
        photoName = (pluralName.Length > 1) ? ("''" + pluralName + "''") : ((sortedBySensation.Count > 0) ? ("''" + sortedBySensation[0] + "''") : (""));

        //photoName +=  " [<color='#" + ColorUtility.ToHtmlStringRGB(ratingSystem.scoreGradient.Evaluate(scren.score / 100f)) + "'>" + scren.score + "</color>]";

        scren.name = photoName;
        scren.portfolioObj = pol;
        scren.polaroidFrame = pol.GetComponent<Image>();

        //Portfolio
        //if(screenshots.Count >= maxPhotosInPortfolio) DeletePicture(0);

        //Border Colors
        scren.polaroidFrame.color = scren.baseColor = eligibleForMissionPictureColor;

        ratingSystem.SetPolaroidTitle(photoName);
        sf.text.text = photoName;

        takePictureDelay = 1f;

        //Physical Pictures        
        //foreach(var pic in physPictures) {
        //  pic.sprite = Sprite.Create(screenshots[Random.Range(0, screenshots.Count)].image, new Rect(0, 0, resWidth, resHeight), Vector2.one * 0.5f);
        //  pic.transform.localScale = basePicScale * physPicScale;
        //}

        screenshots.Add(scren);
        SoundManager.PlayUnscaledSound("PhotoShoot", 0.6f);
        ratingSystem.ResetScreenshot();
    }

    public void PortfolioSelection(Vector2 delta)
    {
        currentSelectedPortfolioPhoto = Mathf.Clamp(currentSelectedPortfolioPhoto - (int)delta.y, 0, Mathf.Clamp(screenshots.Count - 1, 0, screenshots.Count));
        PortfolioSelection();
    }
    public void PortfolioSelection()
    {
        foreach (var i in screenshots) i.portfolioObj.transform.localScale = Vector3.one * portfolioPictureBaseScale;
    }

    public int GetTotalScore()
    {
        return totalScore;
    }

    public void DeletePicture(int index, bool deleteTexture = true)
    {
        return;
        if (screenshots.Count <= 0 || screenshots[index] == null) return;
        Destroy(screenshots[index].portfolioObj);
        if (deleteTexture) Destroy(screenshots[index].image);
        screenshots.RemoveAt(index);
    }
}