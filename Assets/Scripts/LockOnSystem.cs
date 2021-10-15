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
    [Space(10)]
    public CameraCanvas cameraCanvas;
    public Animator animator;
    public GameObject crossHair;
    public GameObject canvas;
    public Camera cam;

    public Dictionary<PhotoItem, LockedObjects> onScreenTargets = new Dictionary<PhotoItem, LockedObjects>();
    [HideInInspector] public List<PhotoItem> sortedScreenObjects = new List<PhotoItem>();

    public PhotoItem[] allTargets {
        get; private set;
    }
    private Transform target;

    void Start() {
        allTargets = GameObject.FindObjectsOfType<PhotoItem>();
        animator.updateMode = AnimatorUpdateMode.UnscaledTime;
        foreach(var i in allTargets) {
            i.render.allowOcclusionWhenDynamic = true;
            
            if(i.isComposite) ;
        }
    }

    void Update() {
        for (int i = 0; i < allTargets.Length; i++) {
            Vector3 targetPos = cam.WorldToViewportPoint(allTargets[i].transform.position);
            bool isOnScreen = (targetPos.z > 0 && targetPos.x > 0 && targetPos.x < 1 && targetPos.y > 0 && targetPos.y < 1) ? true : false;
            bool inDistance = Vector3.Distance(allTargets[i].transform.position, cam.transform.position) < cameraCanvas.maxDistance;
            bool isOccluded = !CanSee(cam.gameObject, allTargets[i]);

            if (!isOccluded && inDistance && isOnScreen && !onScreenTargets.ContainsKey(allTargets[i])) {
                var createImage = Instantiate(crossHair) as GameObject;
                createImage.transform.SetParent(canvas.transform, true);
                createImage.SetActive(true);
                var lockedObject = new LockedObjects();
                lockedObject.crosshair = createImage.GetComponent<Crosshair>();
                lockedObject.target = allTargets[i].gameObject;
                onScreenTargets.Add(allTargets[i], lockedObject);
            } 
            else if (onScreenTargets.ContainsKey(allTargets[i]) && (!isOnScreen || !inDistance || isOccluded)) RemoveCrosshair(allTargets[i]);
        }

        foreach(KeyValuePair<PhotoItem, LockedObjects> k in onScreenTargets) {
            target = onScreenTargets[k.Key].target.transform;
            onScreenTargets[k.Key].crosshair.transform.position = cam.WorldToScreenPoint(target.position);
        }

        PhotoItem tar;
        tar = cameraCanvas.RaycastFromReticle(cameraCanvas.baseReticle.transform);
        sortedScreenObjects = onScreenTargets.Keys.ToList().OrderBy(x => Vector3.Distance(cam.WorldToViewportPoint(FormatVector(x.transform.position)), new Vector3(0.5f, 0.5f, 0))).ToList();

        cameraCanvas.highlightedObjectText.text = "";
        if(sortedScreenObjects.Count > 0) {
            string objects = "";
            foreach(var k in sortedScreenObjects) {
                objects += k.name + "! \n";
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

    public List<PhotoItem> GetOnScreenObjects() {
        return sortedScreenObjects;
    }

    public float GetObjectSharpness(PhotoItem priority) {
        var focusVal = cameraCanvas.GetFocusValue();
        var dist = cameraCanvas.GetPhysicalDistance(priority);

        var focusNormalized = Mathf.Clamp(100 - Mathf.Abs(dist - focusVal), 0, 100);
        var modifier = cameraCanvas.physicalDistanceFocusModifier.Evaluate(focusNormalized / 100f);
        var finFocus = focusNormalized * modifier;

        return finFocus;
    }

    private Vector3 FormatVector(Vector3 vec) {
        vec.z = 0;
        return vec;
    }

    public Crosshair GetScreenCrosshair(PhotoItem pi) {
        if(onScreenTargets.ContainsKey(pi)) return onScreenTargets[pi].crosshair;
        return null;
    }

    private bool CanSee(GameObject origin, PhotoItem toCheck) {
        Vector3 pointOnScreen = cam.WorldToScreenPoint(toCheck.render.bounds.center);
 
        //Is in front
        if (pointOnScreen.z < 0) return false;
        //Is in FOV
        if ((pointOnScreen.x < 0) || (pointOnScreen.x > Screen.width) || (pointOnScreen.y < 0) || (pointOnScreen.y > Screen.height)) return false;
 
        RaycastHit hit;
        Vector3 heading = toCheck.transform.position - origin.transform.position;
        Vector3 direction = heading.normalized;// / heading.magnitude;
        
        if (Physics.Linecast(cam.transform.position, toCheck.render.bounds.center, out hit)) {
            if (hit.transform.name != toCheck.transform.name) return false;
        }
        return true;
    }

    public void RemoveCrosshair(PhotoItem pi) {
        if(!onScreenTargets.ContainsKey(pi)) return;
        Destroy(onScreenTargets[pi].crosshair.gameObject);
        onScreenTargets.Remove(pi);
    }

    public void FadeCrosshair(PhotoItem pi) {
        if(!onScreenTargets.ContainsKey(pi)) return;

        var screenPos = new Vector3(0.5f, 0.5f, 0) - FormatVector(cam.WorldToViewportPoint(onScreenTargets[pi].target.transform.position));
        var uF = 100 - (((screenPos.magnitude - 0.01f) * 100f) * 2);
        var screen = (int)Mathf.Clamp(100 - uF, 0, 100);

        onScreenTargets[pi].crosshair.SetAlpha(screen * 0.01f);
    }

    public float GetCrosshairCenter(PhotoItem pi) {
        if(pi == null) return -1;
        if(!onScreenTargets.ContainsKey(pi)) return -1;
        if(onScreenTargets[pi].target == null) return -1;
        var screenPos = new Vector3(0.5f, 0.5f, 0) - FormatVector(cam.WorldToViewportPoint(onScreenTargets[pi].target.transform.position));
        var uF = 100 - (((screenPos.magnitude - 0.01f) * 100f) * 2);
        var screen = (int)Mathf.Clamp(uF, 0, 100);
        return screen;
    }
}
