using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public struct LockedObjects {
    public GameObject target;
    public Crosshair crosshair;
}

public class LockOnSystem : MonoBehaviour {
    public Transform player;
    public CameraCanvas cameraCanvas;
    public Animator animator;
    public GameObject crossHair;
    public GameObject canvas;
    public Camera cam;

    [Space(10)]
    public Dictionary<PhotoBase, LockedObjects> onScreenTargets = new Dictionary<PhotoBase, LockedObjects>();
    [HideInInspector] public List<PhotoBase> sortedScreenObjects = new List<PhotoBase>();

    [SerializeField] private List<PhotoItem> allTargets = new List<PhotoItem>();
    private Transform target;

    void OnValidate() {
        allTargets = GameObject.FindObjectsOfType<PhotoItem>().ToList();
    }

    void Start() {
        allTargets = GameObject.FindObjectsOfType<PhotoItem>().ToList();
        animator.updateMode = AnimatorUpdateMode.UnscaledTime;
        
        foreach(var i in allTargets) i.render.allowOcclusionWhenDynamic = true;
    }

    void Update() {
        for (int i = 0; i < allTargets.Count; i++) {
            if(!allTargets[i].active) continue;
            bool isComposite = allTargets[i].isComposite;

            if(isComposite) for(int j = 0; j < allTargets[i].GetKeyPoints().Length; j++) PhotoLogic(allTargets[i].GetKeyPoints()[j], allTargets[i]);
            else PhotoLogic(allTargets[i]);
        }

        foreach(KeyValuePair<PhotoBase, LockedObjects> k in onScreenTargets) {
            target = onScreenTargets[k.Key].target.transform;
            onScreenTargets[k.Key].crosshair.transform.position = cam.WorldToScreenPoint(target.position);
        }

        PhotoBase tar;
        tar = cameraCanvas.RaycastFromReticle();
        sortedScreenObjects = onScreenTargets.Keys.ToList().OrderBy(x => Vector3.Distance(cam.WorldToViewportPoint(FormatVector(x.transform.position)), new Vector3(0.5f, 0.5f, 0))).ToList();

        cameraCanvas.highlightedObjectText.text = "";
        if(sortedScreenObjects.Count > 0) {
            string objects = "";
            foreach(var k in sortedScreenObjects) {
                if(!k.isKeyPoint) objects += k.name.Trim() + "! \n";
                else objects += "<color='#ff00ff'>" + k.name + "</color> \n";
                FadeCrosshair(k);
            }
            cameraCanvas.highlightedObjectText.text = objects;
        }

        //Focus haptics
        if(sortedScreenObjects.Count > 0 && cameraCanvas.cameraControl.camSystem.aim >= 0.5f) {
           /*  var priority = sortedScreenObjects[0];
            var focusVal = cameraCanvas.GetFocusValue();
            var dist = cameraCanvas.GetPhysicalDistance(priority);

            var focusNormalized = Mathf.Clamp(100 - Mathf.Abs(dist - focusVal), 0, 100);
            var modifier = cameraCanvas.physicalDistanceFocusModifier.Evaluate(focusNormalized / 100f); */
            var finFocus = GetObjectSharpness(sortedScreenObjects[0]);//focusNormalized * modifier;
            var hapticFocus = Mathf.Clamp((finFocus / 100f) * 1.5f - 1f, 0, 0.4f);

            cameraCanvas.SetFocusResponse(hapticFocus);

            //bool aboveMin = focusVal >= dist - 3;             `\\ check if true
            //bool roof = (focusVal <= dist + 15);                      \\ check if true
        }
    }

    protected void PhotoLogic(PhotoBase target, PhotoItem host = null) {
        Vector3 targetPos = cam.WorldToViewportPoint(target.transform.position);
        bool isOnScreen = (targetPos.z > 0 && targetPos.x > 0 && targetPos.x < 1 && targetPos.y > 0 && targetPos.y < 1) ? true : false;
        bool inDistance = Vector3.Distance(target.transform.position, cam.transform.position) < cameraCanvas.maxDistance;
        bool isOccluded = !CanSee(target, host);
        bool isOrientation = (target.specificOrientation & target.InOrientation(player.position)) | !target.specificOrientation;
        var inFocus = GetObjectSharpness(target);
        bool isInFocus = inFocus >= cameraCanvas.objectInFocusThreshold;

        if (!isOccluded && inDistance && isOrientation && isOnScreen && !onScreenTargets.ContainsKey(target) && target.active && isInFocus) AddCrosshair(target);
        else if (onScreenTargets.ContainsKey(target) && (!isOnScreen || !inDistance || isOccluded || !isOrientation || !isInFocus)) RemoveCrosshair(target);
    }

    public List<PhotoBase> GetOnScreenObjects() {
        return sortedScreenObjects;
    }

    public float GetObjectSharpness(PhotoBase priority) {
        var focusVal = cameraCanvas.GetFocusValue();
        var dist = cameraCanvas.GetPhysicalDistance(priority);

        var focusNormalized = Mathf.Clamp(100 - Mathf.Abs(dist - focusVal), 0, 100);
        var modifier = focusNormalized / 100f;
        var finFocus = focusNormalized * modifier;

        return finFocus;
    }

    private Vector3 FormatVector(Vector3 vec) {
        vec.z = 0;
        return vec;
    }

    public Crosshair GetScreenCrosshair(PhotoBase pi) {
        if(onScreenTargets.ContainsKey(pi)) return onScreenTargets[pi].crosshair;
        return null;
    }

    public List<PhotoItem> GetAllTargets() {
        return allTargets;
    }

    private bool CanSee(PhotoBase toCheck, PhotoItem host = null) {
        var point = toCheck.transform.position;

        Vector3 pointOnScreen = cam.WorldToScreenPoint(point);
 
        //Is in front
        if (pointOnScreen.z < 0) return false;
        //Is in FOV
        if ((pointOnScreen.x < 0) || (pointOnScreen.x > Screen.width) || (pointOnScreen.y < 0) || (pointOnScreen.y > Screen.height)) return false;
        
        RaycastHit hit;
        if (Physics.Linecast(cam.transform.position, point, out hit)) {
            if (hit.transform != toCheck.transform && (host == null || (host != null && hit.transform != host.transform))) return false;
        }
        return true;
    }

    protected void AddCrosshair(PhotoBase pi) {
        var createImage = Instantiate(crossHair) as GameObject;
        createImage.transform.SetParent(canvas.transform, true);
        createImage.SetActive(true);
        var lockedObject = new LockedObjects();
        lockedObject.crosshair = createImage.GetComponent<Crosshair>();
        lockedObject.target = pi.gameObject;
        onScreenTargets.Add(pi, lockedObject);
    }
    protected void RemoveCrosshair(PhotoBase pi) {
        if(!onScreenTargets.ContainsKey(pi)) return;
        Destroy(onScreenTargets[pi].crosshair.gameObject);
        onScreenTargets.Remove(pi);
    }
    protected void FadeCrosshair(PhotoBase pi) {
        if(!onScreenTargets.ContainsKey(pi)) return;

        var screenPos = new Vector3(0.5f, 0.5f, 0) - FormatVector(cam.WorldToViewportPoint(onScreenTargets[pi].target.transform.position));
        var uF = 100 - (((screenPos.magnitude - 0.01f) * 100f) * 2);
        var screen = (int)Mathf.Clamp(100 - uF, 0, 100);

        onScreenTargets[pi].crosshair.SetAlpha(screen * 0.01f);
    }

    public float GetCrosshairCenter(PhotoBase pi) {
        if(pi == null) return -1;
        if(!onScreenTargets.ContainsKey(pi)) return -1;
        if(onScreenTargets[pi].target == null) return -1;
        var screenPos = new Vector3(0.5f, 0.5f, 0) - FormatVector(cam.WorldToViewportPoint(onScreenTargets[pi].target.transform.position));
        var uF = 100 - (((screenPos.magnitude - 0.01f) * 100f) * 2);
        var screen = (int)Mathf.Clamp(uF, 0, 100);
        return screen;
    }
}
