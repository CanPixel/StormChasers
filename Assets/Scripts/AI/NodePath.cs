using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class NodePath : MonoBehaviour {
    public float gizmoRadius = 1f;

    [ReadOnly] public List<Transform> pathNodes;
    
    void OnValidate() {
        pathNodes = transform.GetComponentsInChildren<Transform>().ToList();
        pathNodes.RemoveAt(0);
    }

    void Start() {
        pathNodes = transform.GetComponentsInChildren<Transform>().ToList();
        pathNodes.RemoveAt(0);
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.red;
        foreach(var i in pathNodes) Gizmos.DrawSphere(i.position, gizmoRadius);
    
        Gizmos.color = Color.green;
        for(int i = 1; i < pathNodes.Count; i++) Gizmos.DrawLine(pathNodes[i].position, pathNodes[i - 1].position);
    
        Gizmos.DrawLine(pathNodes[0].position, pathNodes[pathNodes.Count - 1].position);
    }
}