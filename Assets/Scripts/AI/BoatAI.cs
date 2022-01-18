using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatAI : MonoBehaviour
{
    public NodePath boatPath;
    public int nodePoint;

    public Vector3 rotOffs;
    public float speed = 100f, turnSpeed = 1f;

    private Transform currentNodeTarget;

    void OnValidate()
    {
        if (Application.isPlaying) return;

        if (boatPath != null)
        {
            nodePoint = Mathf.Clamp(nodePoint, 0, boatPath.pathNodes.Count - 1);
            SetPos();
        }
    }

    void Start()
    {
        SetPos();
    }

    void FixedUpdate()
    {
        if (currentNodeTarget == null) return;

        var dir = (transform.position - currentNodeTarget.position);
        dir.y = 0;
        transform.position -= dir.normalized * speed * Time.deltaTime;
        transform.rotation = Quaternion.Lerp(transform.rotation, GetAngleTo(currentNodeTarget), Time.deltaTime * turnSpeed);
    }

    protected void SetPos()
    {
        if (boatPath == null) return;

        transform.position = boatPath.pathNodes[nodePoint].position;
        var next = nodePoint + 1;
        if (next >= boatPath.pathNodes.Count) next = 0;
        currentNodeTarget = boatPath.pathNodes[next];
        transform.rotation = GetAngleTo(currentNodeTarget);
    }

    public Quaternion GetAngleTo(Transform targetPos)
    {
        var dir = (transform.position - targetPos.position);
        dir.y = 0;
        return Quaternion.LookRotation(dir) * Quaternion.Euler(rotOffs);
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "NodePath") SetNextNode(col.transform);
    }

    protected void SetNextNode(Transform coll)
    {
        for (int i = 0; i < boatPath.pathNodes.Count; i++) if (boatPath.pathNodes[i] == coll)
            {
                var next = i + 1;
                if (next >= boatPath.pathNodes.Count) next = 0;
                currentNodeTarget = boatPath.pathNodes[next];
                nodePoint = next;
                return;
            }
    }
}