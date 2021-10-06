using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class UIMeshRenderer : Graphic
{
    Camera cam;

    Transform target;

    List<GameObject> targetsInGame = new List<GameObject>();
    public List<GameObject> targetsInFrame = new List<GameObject>();
    List<LockedObjects> crossHairs = new List<LockedObjects>();

    void Update()
    {
        var tempList = crossHairs;
        if (targetsInGame.Count > 0)
        {
        }
        crossHairs = tempList;
        for (int k = 0; k < targetsInFrame.Count; k++)
        {
            target = targetsInFrame[k].transform;
            crossHairs[k].crosshair.transform.position = cam.WorldToScreenPoint(target.position);
        }
    }

}

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();




    }
}
