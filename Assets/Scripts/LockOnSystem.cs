using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class LockedObjects
{
    public GameObject target;
    public GameObject crosshair;
}


public class LockOnSystem : MonoBehaviour
{
    bool locked;
    public GameObject crossHair;
    public GameObject canvas;
    Camera cam;

    Transform target;

    List<GameObject> targetsInGame = new List<GameObject>();
    public List<GameObject> targetsInFrame = new List<GameObject>();
    List<LockedObjects> crossHairs = new List<LockedObjects>();


    void Start()
    {
        cam = GetComponent<Camera>();
        GameObject[] allTargets = GameObject.FindGameObjectsWithTag("Target");

        foreach (GameObject t in allTargets)
        {
            targetsInGame.Add(t);
        }
    }

    void Update()
    {
        var tempList = crossHairs;
        if (targetsInGame.Count > 0)
        {
            for (int i = 0; i < targetsInGame.Count; i++)
            {
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
                    Debug.Log("Kill M e");
                }
            }
        }

        crossHairs = tempList;
        for (int k = 0; k < targetsInFrame.Count; k++)
        {
            target = targetsInFrame[k].transform;
            crossHairs[k].crosshair.transform.position = cam.WorldToScreenPoint(target.position);
        }
    }

}
