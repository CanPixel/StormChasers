using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineRenderTest : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform a;
    public Transform b;
    public LineRenderer l; 

    void Start()
    {
        l.positionCount = 2; 
    }

    // Update is called once per frame
    void Update()
    {
        l.SetPosition(0, a.position);
        l.SetPosition(1, b.position); 
    }
}
