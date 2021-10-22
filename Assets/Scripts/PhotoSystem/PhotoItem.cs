using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhotoItem : PhotoBase {
    public MeshRenderer render;
    public bool isComposite = false;

    [System.Serializable]
    public class KeyPointList {
        public PhotoKeyPoint[] list;
    }

    [ConditionalHide("isComposite", true)] [SerializeField] private KeyPointList keyPoints;
    public float gizmosScale = 5.0f;

    void Start() {
        if(isComposite) keyPoints.list = GetComponentsInChildren<PhotoKeyPoint>();
    }

    public PhotoKeyPoint[] GetKeyPoints() {
        if(!isComposite) return null;
        return keyPoints.list;
    }

    void OnValidate() {
        if(isComposite) keyPoints.list = GetComponentsInChildren<PhotoKeyPoint>();

        orientationViewCone = Mathf.Clamp(orientationViewCone, 0, 180);
    }

    void OnDrawGizmosSelected() {
        if(!isComposite) {
            Gizmos.color = Color.green;
            Gizmos.DrawCube(transform.position, Vector3.one * gizmosScale * transform.localScale.x);
        }

        if(!specificOrientation || isComposite) return;
   
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
}
