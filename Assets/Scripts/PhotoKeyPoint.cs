using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhotoKeyPoint : MonoBehaviour {
    public PhotoItem hostObject;
    [Range(0, 1)]
    public float weight = 0.5f;
}
