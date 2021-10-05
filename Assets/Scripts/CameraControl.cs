using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;
using Cinemachine.PostFX;

public class CameraControl : MonoBehaviour {
    public float cameraRotateSpeedX = 200.0f, cameraRotateSpeedY = 3.0f;
    public Cinemachine.CinemachineVirtualCamera firstPersonLook, thirdPersonLook;
    private Cinemachine.CinemachinePOV pov;
    private Cinemachine.CinemachineOrbitalTransposer orbitalTransposer;
    public Cinemachine.CinemachineBrain cinemachineBrain;
    public GameObject photoBookUI, polaroidPrefab;
    public Transform photoBookScrollPanel;
    public UIBob reticleBob;
    public CarMovement carMovement;

    public LockOnSystem lockOnSystem;
    public ScoringSystem scoringSystem;

/*     public Transform splashParent;
    public GameObject splashPrefab; */
/*     public List<PictureScoring> pictureScore = new List<PictureScoring>();
    private float pictureScoreTimer = 0; */

    [HideInInspector] public bool photoBook = false;

    public Text polaroidTitle;
    public Image polaroidUI, polaroidScreenshot;
    public float polaroidBottomPos = 0, polaroidTopPos = 100;

    public int maxPhotosInPortfolio = 5;

    public int resWidth = 1600, resHeight = 900;

    public Cinemachine.PostFX.CinemachinePostProcessing postProcessing;

    public float slowMotionDamping = 3f, slowMotionTime = 0.1f;

    public GameObject[] enableOnFirstPerson;

    public enum CameraState {
        CARVIEW = 0, CAMVIEW, DASHVIEW
    }
    public CameraState cameraState = CameraState.CARVIEW;

    public Vector3 rotationOffset;
    public GameObject cameraMascotte;

    [HideInInspector] public Vector2 rotationInput;

    private float screenshotTimer = 0;
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
        public float aim, shoot, focus;

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
    public CameraSystem camSystem;
    public ShaderReel shaderReel;

    private float recenterTime = 0;
    public float recenterDuration = 1f;

    public List<Screenshot> screenshots = new List<Screenshot>();
    private List<GameObject> photoBookShots = new List<GameObject>();

    void Start() {
        pov = firstPersonLook.GetCinemachineComponent<Cinemachine.CinemachinePOV>();
        orbitalTransposer = thirdPersonLook.GetCinemachineComponent<Cinemachine.CinemachineOrbitalTransposer>();
        foreach(var i in enableOnFirstPerson) i.SetActive(false); 
        polaroidUI.transform.position = new Vector3(polaroidUI.transform.position.x, polaroidBottomPos, polaroidUI.transform.position.z);
    }

    void Update() {
        photoBookUI.SetActive(photoBook);

        Time.timeScale = Mathf.Lerp(Time.timeScale, (IsAiming()) ? slowMotionTime : 1.0f, Time.unscaledDeltaTime * slowMotionDamping * (!IsAiming() ? 4f : 1f));
        SoundManager.SlowMo();

        if(recenterTime > 0 && Mathf.Abs(orbitalTransposer.m_XAxis.Value) > 1) {
            recenterTime -= Time.deltaTime * 2f;
            orbitalTransposer.m_XAxis.Value = Mathf.Lerp(orbitalTransposer.m_XAxis.Value, 0, Time.deltaTime * 6f);
        }

        if(camSystem.aim > 0.4) FirstPersonLook();
        else ThirdPersonLook();

        cameraMascotte.transform.localScale = Vector3.Lerp(cameraMascotte.transform.localScale, Vector3.one, Time.deltaTime * 5f);

        if(screenshotTimer > 0) {
            screenshotTimer += Time.unscaledDeltaTime * 2f;

            if(screenshotTimer > 10) ResetScreenshot();

            if(screenshotTimer > 7f) polaroidUI.transform.position = new Vector3(polaroidUI.transform.position.x, Mathf.Lerp(polaroidUI.transform.position.y, polaroidBottomPos, Time.unscaledDeltaTime * 5f), polaroidUI.transform.position.z);
            else polaroidUI.transform.position = new Vector3(polaroidUI.transform.position.x, Mathf.Lerp(polaroidUI.transform.position.y, polaroidTopPos, Time.unscaledDeltaTime * 7f), polaroidUI.transform.position.z);
        }
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

        cameraMascotte.transform.rotation = firstPersonLook.transform.rotation;
        cameraMascotte.transform.Rotate(rotationOffset);
        cameraMascotte.transform.eulerAngles += new Vector3(0, orbitalTransposer.m_XAxis.Value + transform.eulerAngles.y, 0);
        
        pov.m_HorizontalAxis.Value = cameraMascotte.transform.localEulerAngles.z + transform.eulerAngles.y;
    }

    public void AnimateCameraMascotte() {
        cameraMascotte.transform.localScale = Vector3.one * 1.2f;
    }

    public void Recenter() {
        pov.m_VerticalAxis.Value = 0;
        recenterTime = recenterDuration;
    }

    private void ResetScreenshot() {
        screenshotTimer = 0;
        polaroidUI.transform.position = new Vector3(polaroidUI.transform.position.x, polaroidBottomPos, polaroidUI.transform.position.z);
    }

    protected void TakePicture() {
        if(!scoringSystem.ready) return;

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

        string photoName = "";
        int photoScore = 0;
        foreach(var i in lockOnSystem.allTargets) {
            var dist = (int)Mathf.Clamp(Mathf.Abs(Vector3.Distance(transform.position, i.transform.position)), 1, 200);

            Vector3 targetPos = cam.WorldToViewportPoint(i.transform.position);
            bool isOnScreen = (targetPos.z > 0 && targetPos.x > 0 && targetPos.x < 1 && targetPos.y > 0 && targetPos.y < 1) ? true : false;

            if(isOnScreen) {
                if(photoScore < 200 - dist) {
                    photoScore = 200 - dist;
                    photoName = i.name + "!";
                }
                Debug.Log("[" + i.name + "]: Physical Dist: " + dist + " || OnScreen: " + isOnScreen + " : ");
            }
        }
        polaroidTitle.text = sf.text.text = photoName;
        scoringSystem.CalculatePictureScore(scren);

        // sf.text.text = scren.name;
        photoBookShots.Add(pol);

        if(screenshots.Count >= maxPhotosInPortfolio) {
            Destroy(screenshots[0].image);
            screenshots.RemoveAt(0);
            photoBookShots.RemoveAt(0);
            Destroy(photoBookShots[0]);
        }

        screenshotTimer = 0.1f;
        SoundManager.PlayUnscaledSound("PhotoShoot");

        polaroidUI.transform.position = new Vector3(polaroidUI.transform.position.x, polaroidBottomPos, polaroidUI.transform.position.z);
    }
}
