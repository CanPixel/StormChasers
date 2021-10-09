using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public struct LockedObjects {
    public GameObject target;
    public GameObject crosshair;
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
        foreach(var i in allTargets) i.render.allowOcclusionWhenDynamic = true;
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
                lockedObject.crosshair = createImage;
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
            }
            cameraCanvas.highlightedObjectText.text = objects;
        }
        if(tar != null) RemoveCrosshair(tar);
    }

    private Vector3 FormatVector(Vector3 vec) {
        vec.z = 0;
        return vec;
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
        Destroy(onScreenTargets[pi].crosshair);
        onScreenTargets.Remove(pi);
    }
}
