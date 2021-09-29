using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockOnSystem : MonoBehaviour
{
    bool locked;
    public GameObject crossHair;
    Camera cam;

    Transform target;

    List<GameObject> targetsInGame = new List<GameObject>();
    public List<GameObject> targetsInFrame = new List<GameObject>();

    int index = 0;

    void Start()
    {
        cam = GetComponent<Camera>();
        crossHair.SetActive(false);
        GameObject[] allTargets = GameObject.FindGameObjectsWithTag("Target");

        foreach (GameObject t in allTargets)
        {
            targetsInGame.Add(t);
        }
    }

    void Update()
    {
        if (targetsInGame.Count > 0)
        {
            for (int i = 0; i < targetsInGame.Count; i++)
            {
                Vector3 targetPos = cam.WorldToViewportPoint(targetsInGame[i].transform.position);

                bool isOnScreen = (targetPos.z > 0 && targetPos.x > 0 && targetPos.x < 1 && targetPos.y > 0 && targetPos.y < 1) ? true : false;

                if (isOnScreen && !targetsInFrame.Contains(targetsInGame[i]) && index < targetsInGame.Count)
                {
                    targetsInFrame.Add(targetsInGame[i]);
                    //index++;
                }
                else if (targetsInFrame.Contains(targetsInGame[i]) && !isOnScreen)
                {
                    index = 0;
                    locked = false;
                    target = null;
                    crossHair.SetActive(false);
                    targetsInFrame.Remove(targetsInGame[i]);
                }
            }
        }
        if (!locked && targetsInFrame.Count > 0)
        {
            locked = true;
            crossHair.SetActive(true);
        }
        else if (locked && Input.GetKeyDown(KeyCode.X))
        {
            locked = false;
            target = null;
            crossHair.SetActive(false);
        }
        if (locked)
        {
            target = targetsInFrame[index].transform;
            crossHair.transform.position = cam.WorldToScreenPoint(target.position);
        }


    }
}
