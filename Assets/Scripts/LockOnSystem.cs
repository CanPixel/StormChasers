using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class LockedObjects {
    public GameObject target;
    public GameObject crosshair;
}


public class LockOnSystem : MonoBehaviour {
    public Animator animator;
    public GameObject crossHair;
    public GameObject canvas;
    public Camera cam;

    Transform target;

    List<GameObject> targetsInGame = new List<GameObject>();
    private List<GameObject> targetsInFrame = new List<GameObject>();
    List<LockedObjects> crossHairs = new List<LockedObjects>();

    public PhotoItem[] allTargets {
        get; private set;
    }

    void Start() {
 //       GameObject[] allTargets = GameObject.FindGameObjectsWithTag("Target");
        allTargets = GameObject.FindObjectsOfType<PhotoItem>();

        foreach (PhotoItem t in allTargets) if(t.showLockOnMarker) targetsInGame.Add(t.gameObject);
        animator.updateMode = AnimatorUpdateMode.UnscaledTime;
    }

    void Update() {
        var tempList = crossHairs;
        if (targetsInGame.Count > 0) {
            for (int i = 0; i < targetsInGame.Count; i++) {
                if(targetsInGame.Count <= i) return;
                
                Vector3 targetPos = cam.WorldToViewportPoint(targetsInGame[i].transform.position);
                bool isOnScreen = (targetPos.z > 0 && targetPos.x > 0 && targetPos.x < 1 && targetPos.y > 0 && targetPos.y < 1) ? true : false;

                if (isOnScreen && !targetsInFrame.Contains(targetsInGame[i]))
                {
                    targetsInFrame.Add(targetsInGame[i]);
                    var createImage = Instantiate(crossHair) as GameObject;
                    createImage.transform.SetParent(canvas.transform, true);
                    createImage.SetActive(true);
                    var lockedObject = new LockedObjects();
                    lockedObject.crosshair = createImage;
                    lockedObject.target = targetsInGame[i];
                    crossHairs.Add(lockedObject) ;

                }
                else if (targetsInFrame.Contains(targetsInGame[i]) && !isOnScreen)
                {
                    var tempTargetInGame = targetsInGame[i];
                    targetsInFrame.Remove(tempTargetInGame);
                    var tempCrossHair = crossHairs.Where(x => x.target == tempTargetInGame).ToList()[0];
                    tempList.Remove(tempCrossHair);
                    Destroy(tempCrossHair.crosshair);
                }
                if(targetsInFrame.Count <= 0 || crossHairs.Count <= 0) break;
            }
        }
        crossHairs = tempList;
        if(targetsInFrame.Count <= 0 || crossHairs.Count <= 0) return;
        for (int k = 0; k < targetsInFrame.Count; k++)
        {
            if(crossHairs.Count <= k || targetsInFrame.Count <= k) break;
            target = targetsInFrame[k].transform;
            crossHairs[k].crosshair.transform.position = cam.WorldToScreenPoint(target.position);
        }
    }
}
