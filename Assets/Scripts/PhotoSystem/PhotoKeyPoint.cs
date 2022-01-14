using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhotoKeyPoint : PhotoBase {
    public PhotoItem hostObject;
    [Range(0, 1)]
    public float weight = 0.5f;

    void OnValidate() {
        orientationViewCone = Mathf.Clamp(orientationViewCone, 0, 180);
    }

    void OnDrawGizmosSelected() {
        if(hostObject.isComposite) {
            Gizmos.color = Color.yellow;
            Gizmos.DrawCube(transform.position, Vector3.one * hostObject.gizmosScale * weight * transform.localScale.x);
        } else return;

        if(!specificOrientation) return;
   
        Quaternion leftRayRotation = Quaternion.AngleAxis(-orientationViewCone, Vector3.up );
        Quaternion rightRayRotation = Quaternion.AngleAxis(orientationViewCone, Vector3.up );
        Gizmos.color = Color.white;

        var forwardDir = (transform.forward + transform.TransformVector(axisOffset)).normalized;

        Vector3 leftRayDirection = leftRayRotation * forwardDir;
        Vector3 rightRayDirection = rightRayRotation * forwardDir;
        Gizmos.DrawRay( transform.position, leftRayDirection * rayRange);
        Gizmos.DrawRay( transform.position, rightRayDirection * rayRange); 

        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, forwardDir * rayRange);
    }

    public new void Start() {
        base.Start();
        isKeyPoint = true;
        if(!hostObject.isComposite) return;
        gameObject.name = hostObject.name + " (" + (transform.GetSiblingIndex() + 1) + "/" + hostObject.GetKeyPoints().Length + ")";
    }
}

public abstract class PhotoBase : MonoBehaviour {
    [ReadOnly] public bool active = true;
    [ReadOnly] public string staticTags;
    public bool specificOrientation = false; 
    [HideInInspector] public bool isKeyPoint = false;
    public string baseTag;
    public string tags {
        get; private set;
    }
    public int sensation = 0;

    [ConditionalHide("specificOrientation", true)] public Vector3 axisOffset = new Vector3(0, 0, 0);
    [ConditionalHide("specificOrientation", true)] public float orientationViewCone = 70f;
    [ConditionalHide("specificOrientation", true)] public float rayRange = 10.0f;

    public void Start() {
        tags = tags = staticTags = baseTag;
    }

    public void AddTag(string i) {
        string add = i.Trim();
        tags += add + " ";
    }

    public void OverwriteTag(string i) {
        tags = i;
    }

    public bool InOrientation(Vector3 pos) {
        var forwardDir = (transform.forward + transform.TransformVector(axisOffset)).normalized;

        Vector3 relativeNormalizedPos = (pos - transform.position).normalized;
        float dot = Vector3.Dot(relativeNormalizedPos, forwardDir);
        float angle = Mathf.Acos(dot) * Mathf.Rad2Deg;

        return angle < orientationViewCone;
    }
}
