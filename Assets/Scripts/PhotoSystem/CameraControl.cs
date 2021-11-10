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

    [Header("On Picture Shot")]
    public AnimationCurve pictureShotAlphaFade = AnimationCurve.Linear(0,  1, 1, 0);
    public float pictureDelayUntilNext = 1;
    private float pictureShotTimer = 0;

    [Header("Portfolio")]
    public int maxPhotosInPortfolio = 5;
    public float portfolioPictureSpacing = 300;
    public float portfolioPictureYOffs = 300;
    public Color activeMissionColor, inactiveMissionColor;
    public Color markedPictureColor;
    public Color eligibleForMissionPictureColor;

    [Header("Boost Cam")]
    public AnimationCurve zPosOffs;
    public Vector3 BoostFollowOffset = new Vector3(0, 2.8f, -6);
    public float BoostFollowDamping = 2f;

    private float recenterTime = 0;
    [Space(5)]
    public float recenterDuration = 1f;

    [Header("Physical Discarded Picture")]
    public float DiscardForce = 10f;
    public float physPicScale = 0.5f;
    private Vector3 basePicScale;

    [HideInInspector] public bool journal = false, photobook = false;

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
    public AnimationCurve yPosOffs;

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
        [HideInInspector] public Image polaroidFrame;
        [HideInInspector] public Color baseColor = new Color(0.9f, 0.9f, 0.9f, 1);
        public bool forMission = false;
        [HideInInspector] public MissionManager.Mission missionReference;

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
    public Cinemachine.CinemachineBrain cinemachineBrain;
    public GameObject journalUI, photoBookUI, polaroidPrefab;
    public GameObject cameraMascotte;
    public SpriteRenderer missionPicture;
    public Transform photoBookScrollPanel;
    public Image pictureShotOverlay;
    public RawImage photoBookSelection;
    public GameObject physicalPhotoPrefab;
    public Text photoBookCapacity;
    public UIBob reticleBob;
    public Image worldAimReticle;
    public Text worldAimControlText;
    public GameObject minimapCamera;
    public CarMovement carMovement;
    public CameraCanvas cameraCanvas;
    public LockOnSystem lockOnSystem;
    public RatingSystem ratingSystem;
    public MissionManager missionManager;
    [SerializeField] private InputActionReference cameraAimButton;

    public GameObject journalMissionPrefab;
    public GameObject journalSelection, journalPictureDeliveredPanel;
    public Outline journalSelectionOutline;
    public GameObject journalInfoSelectedBG;
    public GameObject baseJournalButtons;
    public Image pictureDelivered;
    public GameObject markPictureButton, discardPictureButton;
    public GameObject missionListOverlay;
    public GameObject yInfoButton;
    public Text openMissionText, completedMissionText;
    public Transform missionContentParent, missionPanelContentParent;
    public Text missionDescription, markButtonText; 
    public MissionManager.MissionCriteria[] missionRequirement;

    public ShaderReel shaderReel;
    public GameObject cycleFilterYButton;

    private Vector3 baseFollowOffset;
    private float baseZDamping;
    private float minimapCamBaseAngle, minimapCamBaseY;

    private int completedMissions = 0;

    private float blend = 0;
    private int currentSelectedPortfolioPhoto = 0;

    [System.Serializable]
    public class JournalMission {
        public MissionManager.Mission mission;
        public GameObject journalElement;
        public Text journalTitle;
        public Sprite finalPicture;
        public bool active = false;
    }
    private List<JournalMission> journalMissions = new List<JournalMission>();
    private int journalSelectedMission = 0;

    private float portfolioPictureBaseScale, portfolioTargetY = 0;

    void Start() {
        pov = firstPersonLook.GetCinemachineComponent<Cinemachine.CinemachinePOV>();
        orbitalTransposer = thirdPersonLook.GetCinemachineComponent<Cinemachine.CinemachineOrbitalTransposer>();
        composer = thirdPersonLook.GetCinemachineComponent<Cinemachine.CinemachineComposer>();
        baseScreenY = composer.m_ScreenY;
        foreach(var i in enableOnFirstPerson) i.SetActive(false); 
        missionPanelContentParent.gameObject.SetActive(false);

        baseFollowOffset = orbitalTransposer.m_FollowOffset;
        baseZDamping = orbitalTransposer.m_ZDamping;

        pictureShotTimer = pictureShotAlphaFade.Evaluate(pictureShotAlphaFade.length);
        pictureShotOverlay.gameObject.SetActive(true);

        minimapCamBaseAngle = minimapCamera.transform.eulerAngles.x;
        minimapCamBaseY = minimapCamera.transform.position.y;

        portfolioPictureBaseScale = polaroidPrefab.transform.localScale.x;

        baseAlphaFirstPersonFade = new Color[fadeOnFirstPerson.Length];
        for(int i = 0; i < fadeOnFirstPerson.Length; i++) baseAlphaFirstPersonFade[i] = fadeOnFirstPerson[i].color;

        ratingSystem.gameObject.SetActive(true);

        basePicScale = missionPicture.transform.localScale;
        missionPicture.gameObject.SetActive(false);
        photobook = false;

        journalInfoSelectedBG.SetActive(false);
        photoBookUI.SetActive(false);
        journalSelectionOutline.enabled = false;
        markPictureButton.SetActive(false);
        discardPictureButton.SetActive(false);
        missionListOverlay.SetActive(false);
    }

    private Vector3 worldReticleTarget;
    private float distTarget = 1f;

    void Update() {
        minimapCamera.transform.position = new Vector3(transform.position.x, minimapCamBaseY, transform.position.z);
        minimapCamera.transform.rotation = Quaternion.Euler(minimapCamBaseAngle, transform.eulerAngles.y, 0);

        pictureShotTimer += Time.unscaledDeltaTime;
        pictureShotOverlay.color = new Color(pictureShotOverlay.color.r, pictureShotOverlay.color.g, pictureShotOverlay.color.b, pictureShotAlphaFade.Evaluate(pictureShotTimer));

        if(isLookingBack) RecenterY();

        if(deliverPicture) {
            deliverTime += Time.deltaTime;

            deliverPicture.transform.localScale = Vector3.Lerp(deliverPicture.transform.localScale, Vector3.zero, Time.deltaTime * 1f);
            deliverPicture.transform.position = Vector3.Lerp(deliverPicture.transform.position, deliverTo, Time.deltaTime * 1.5f);
            if(deliverTime > 1) {
                Destroy(deliverPicture);
                missionManager.Deliver();
            }
        }

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

        journalUI.SetActive(journal);

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
        photoBookSelection.enabled = screenshots.Count > 0;
        if(screenshots.Count > 0 && currentSelectedPortfolioPhoto < screenshots.Count) {
            var screen = screenshots[currentSelectedPortfolioPhoto].portfolioObj.transform;
            photoBookSelection.transform.position = screen.position;
            screen.localScale = Vector3.Lerp(screen.localScale, Vector3.one, Time.unscaledDeltaTime * 10f);
            photoBookSelection.transform.localScale = screen.localScale;
        }
        
        currentSelectedPortfolioPhoto = Mathf.Clamp(currentSelectedPortfolioPhoto, 0, Mathf.Clamp(screenshots.Count - 1, 0, screenshots.Count));
        portfolioTargetY = currentSelectedPortfolioPhoto * portfolioPictureSpacing - portfolioPictureYOffs;

        if(journal) {
            if(journalMissions.Count > 0) journalSelection.transform.position = Vector3.Lerp(journalSelection.transform.position, new Vector3(journalSelection.transform.position.x, journalMissions[journalSelectedMission].journalElement.transform.position.y, journalSelection.transform.position.z), Time.unscaledDeltaTime * 16f);
            foreach(var jM in journalMissions) jM.journalTitle.color = (jM.active && !jM.mission.delivered) ? activeMissionColor : inactiveMissionColor;
            photoBookScrollPanel.localPosition = Vector3.Lerp(photoBookScrollPanel.localPosition, new Vector3(photoBookScrollPanel.localPosition.x, portfolioTargetY, photoBookScrollPanel.localPosition.z), Time.unscaledDeltaTime * 12f);
        }
        cycleFilterYButton.SetActive(!journal);
        journalSelection.SetActive(journalMissions.Count > 0);
        markPictureButton.SetActive(screenshots.Count > 0 && screenshots[currentSelectedPortfolioPhoto] != null && screenshots[currentSelectedPortfolioPhoto].forMission);
        discardPictureButton.SetActive(screenshots.Count > 0 && screenshots[currentSelectedPortfolioPhoto] != null);
        yInfoButton.SetActive(screenshots.Count > 0);
        markButtonText.text = ((markedScreenshot != null) ? "UN" : "") +  "MARK";
    }

    public void OpenJournal() {
        JournalScroll();
    }

    private void ShowMissionInfo(MissionManager.Mission mission) {
        missionPanelContentParent.gameObject.SetActive(mission != null);
        if(mission != null) {
            journalPictureDeliveredPanel.SetActive(mission.delivered);
            yInfoButton.SetActive(!mission.delivered);

            missionDescription.text = mission.description;
            for(int i = 0; i < missionRequirement.Length; i++) {
                if(mission.objectives.Length <= i) {
                    missionRequirement[i].Disable();
                    continue;
                }
                missionRequirement[i].Load(mission.objectives[i]);
                if(mission.objectives[i].finished) missionRequirement[i].Clear();
            }
        }
    }

    public void ForceSelect(MissionManager.Mission miss) {
        for(int i = 0; i < journalMissions.Count; i++) {
            if(journalMissions[i].mission == miss) {
                journalSelectedMission = i;
                JournalSelect();
                JournalScroll();
                return;
            }
        }
    }

    public void JournalSelect() {
        if(photobook) {
            MarkPicture();
            return;
        }

        if(journalMissions[journalSelectedMission].mission.delivered) return;

        foreach(var i in journalMissions) {
            i.active = false;
            if(i == journalMissions[journalSelectedMission]) i.active = true;
        }
        missionManager.SetCurrentMission(journalMissions[journalSelectedMission].mission, true);
        for(int i = 0; i < journalMissions[journalSelectedMission].mission.objectives.Length; i++) missionManager.MarkCurrentObjective(i, journalMissions[journalSelectedMission].mission.objectives[i].finished);
    }
    public void ShowJournalInfo() {
        if(!journal || journalMissions.Count <= 0 || journalMissions[journalSelectedMission].mission.delivered || screenshots.Count <= 0) return;
        journalSelectionOutline.enabled = true;
        missionListOverlay.SetActive(true);

        if(!photobook) {
            photobook = true;
            baseJournalButtons.SetActive(false);
            photoBookUI.SetActive(true);
        }
        journalInfoSelectedBG.SetActive(true);
    }
    public void ShowJournalBaseScreen() {
        missionListOverlay.SetActive(false);
        journalSelectionOutline.enabled = false;
        baseJournalButtons.SetActive(true);
        journalInfoSelectedBG.SetActive(false);
        photoBookUI.SetActive(false);
        photobook = false;
    }
    public void JournalMarkObjective(int objectiveIndex) {
        journalMissions[journalSelectedMission].mission.objectives[objectiveIndex].finished = true;
        missionRequirement[objectiveIndex].Clear();
    }
    public void JournalFinish() {
        journalMissions[journalSelectedMission].mission.delivered = true;
        journalMissions[journalSelectedMission].journalElement.transform.SetAsLastSibling();
        completedMissions++;
    }

    public void JournalScroll(Vector2 i) {
        missionPanelContentParent.gameObject.SetActive(journalMissions.Count > 0);
        journalSelectedMission = (int)(journalSelectedMission + i.y);
        journalSelectedMission = Mathf.Clamp(journalSelectedMission, 0, journalMissions.Count - 1);
        JournalScroll();
    }
    public void JournalScroll() {
        if(journalMissions.Count > 0 && journalMissions[journalSelectedMission] != null) ShowMissionInfo(journalMissions[journalSelectedMission].mission);
        pictureDelivered.sprite = journalMissions[journalSelectedMission].finalPicture;
    }
    public void AddMissionToJournal(MissionManager.Mission miss) {
        var obj = Instantiate(journalMissionPrefab);
        obj.transform.SetParent(missionContentParent);
        obj.transform.localScale = Vector3.one * 0.85f;
        obj.transform.SetSiblingIndex(1);

        var jM = new JournalMission();
        jM.journalElement = obj;
        jM.mission = miss;
        jM.journalTitle = obj.GetComponentInChildren<Text>();
        jM.journalTitle.text = miss.name;
        if(journalMissions.Count <= 0) jM.active = true;
        journalMissions.Add(jM);
        JournalScroll();
    }

    public void MarkPicture() {
        if(currentSelectedPortfolioPhoto >= screenshots.Count || screenshots[currentSelectedPortfolioPhoto] == null || !screenshots[currentSelectedPortfolioPhoto].forMission || screenshots.Count <= 0) return;

        //Already marked
        if(AlreadyMarked()) {
            UnmarkPicture();
            return;
        }

        screenshots[currentSelectedPortfolioPhoto].polaroidFrame.color = markedPictureColor;
        missionPicture.sprite = Sprite.Create(screenshots[currentSelectedPortfolioPhoto].image, new Rect(0, 0, resWidth, resHeight), Vector2.one * 0.5f);
        missionPicture.transform.localScale = basePicScale * physPicScale;
        missionPicture.gameObject.SetActive(true);
        missionManager.MarkForCurrentMission(true);
        missionManager.ShowReadyForMark(false);
        markedScreenshot = screenshots[currentSelectedPortfolioPhoto];
    }
    public void UnmarkPicture() {
        missionPicture.sprite = null;
        missionPicture.gameObject.SetActive(false);
        missionManager.MarkForCurrentMission(false);
        missionManager.ShowReadyForMark(true);
        Destroy(missionPicture.sprite);
        if(screenshots.Count <= 0 || currentSelectedPortfolioPhoto >= screenshots.Count || screenshots[currentSelectedPortfolioPhoto] == null) return;
        screenshots[currentSelectedPortfolioPhoto].polaroidFrame.color = screenshots[currentSelectedPortfolioPhoto].baseColor;
        markedScreenshot = null;
    }
    public void DiscardPicture() {
        if(currentSelectedPortfolioPhoto >= screenshots.Count || screenshots[currentSelectedPortfolioPhoto] == null || screenshots.Count <= 0) return;
        var obj = Instantiate(physicalPhotoPrefab, transform.position + Vector3.up * 2f, Quaternion.Euler(10,0,0));
        obj.GetComponent<Rigidbody>().AddForce(Vector3.up * DiscardForce * 50f);
        obj.transform.localScale = Vector3.one * 0.15f;
        obj.GetComponent<SpriteRenderer>().sprite = Sprite.Create(screenshots[currentSelectedPortfolioPhoto].image, new Rect(0, 0, resWidth, resHeight), Vector2.one * 0.5f);

        if(screenshots[currentSelectedPortfolioPhoto].forMission) missionManager.DiscardMissionPicture();

        DeletePicture(currentSelectedPortfolioPhoto, false);
        PortfolioSelection();
        UnmarkPicture();
        missionManager.ShowReadyForMark(false);
    }
    protected bool AlreadyMarked() {
        return currentSelectedPortfolioPhoto < screenshots.Count && markedScreenshot != null && screenshots[currentSelectedPortfolioPhoto] != null && markedScreenshot == screenshots[currentSelectedPortfolioPhoto];
    }

    private GameObject deliverPicture;
    private Vector3 deliverTo;
    private float deliverTime = 0;
    public void DeliverPicture(Vector3 pos) {
        if(screenshots[currentSelectedPortfolioPhoto] == null || deliverPicture != null) return;

        deliverPicture = Instantiate(physicalPhotoPrefab, transform.position + Vector3.up * 2f, Quaternion.Euler(10,0,0));
        var rb = deliverPicture.GetComponent<Rigidbody>();
        rb.AddForce(Vector3.up * DiscardForce * 50f);
        rb.useGravity = false;
        deliverPicture.transform.localScale = Vector3.one * 0.15f;
        var spr = Sprite.Create(screenshots[currentSelectedPortfolioPhoto].image, new Rect(0, 0, resWidth, resHeight), Vector2.one * 0.5f);
        deliverPicture.GetComponent<SpriteRenderer>().sprite = spr;
        
        journalMissions[journalSelectedMission].finalPicture = spr;
        ShowJournalBaseScreen();

        DeletePicture(currentSelectedPortfolioPhoto, false);
        currentSelectedPortfolioPhoto = 0;
        UnmarkPicture();
        
        journal = false;
        deliverTo = pos;
        deliverTime = 0;
    }
    
    private bool isLookingBack = false;
    public void BackLook(bool lookBack) {
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
        if(cinemachineBrain.IsLive(firstPersonLook)) {
            if(!cinemachineBrain.IsBlending) cam.cullingMask = firstPersonCull;
            else cam.cullingMask = midPersonCull; 
        } else cam.cullingMask = thirdPersonCull;

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

        orbitalTransposer.m_FollowOffset = Vector3.Lerp(orbitalTransposer.m_FollowOffset, (carMovement.boostScript.isBoosting) ? BoostFollowOffset + new Vector3(0, yPosOffs.Evaluate(composer.m_ScreenY), zPosOffs.Evaluate(composer.m_ScreenY)) : baseFollowOffset + new Vector3(0, yPosOffs.Evaluate(composer.m_ScreenY), zPosOffs.Evaluate(composer.m_ScreenY)), Time.deltaTime * BoostFollowDamping);
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
        if(!ratingSystem.ready || !ratingSystem.AfterDelay(pictureDelayUntilNext) || ratingSystem.HasTakenPicture()) return;

        pictureShotTimer = 0;
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
        pol.transform.localPosition = Vector3.zero;
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
        scren.polaroidFrame = pol.GetComponent<Image>();
        var completed = missionManager.CheckCompletion(score, scren);

        //Border Colors
        if(completed) scren.polaroidFrame.color = scren.baseColor = eligibleForMissionPictureColor;;

        ratingSystem.SetPolaroidTitle(photoWithoutScore);
        sf.text.text = score.name = photoName;

        //Portfolio
        if(screenshots.Count >= maxPhotosInPortfolio) DeletePicture(0);
        screenshots.Add(scren);

        //Physical Pictures        
        //foreach(var pic in physPictures) {
          //  pic.sprite = Sprite.Create(screenshots[Random.Range(0, screenshots.Count)].image, new Rect(0, 0, resWidth, resHeight), Vector2.one * 0.5f);
          //  pic.transform.localScale = basePicScale * physPicScale;
        //}

        SoundManager.PlayUnscaledSound("PhotoShoot", 0.8f);
        ratingSystem.ResetScreenshot();
    }

    public void PortfolioSelection(Vector2 delta) {
        currentSelectedPortfolioPhoto = Mathf.Clamp(currentSelectedPortfolioPhoto - (int)delta.y, 0, Mathf.Clamp(screenshots.Count - 1, 0, screenshots.Count));
        PortfolioSelection();
    }
    public void PortfolioSelection() {
        foreach(var i in screenshots) i.portfolioObj.transform.localScale = Vector3.one * portfolioPictureBaseScale;
    }
    
    public void DeletePicture(int index, bool deleteTexture = true) {
        if(screenshots.Count <= 0 || screenshots[index] == null) return;
        Destroy(screenshots[index].portfolioObj);
        if(deleteTexture) Destroy(screenshots[index].image);
        screenshots.RemoveAt(index);
    }
}