using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextSpawn : MonoBehaviour
{

    void ShowFloatingText(GameObject floatingTextPrefab, string s)
    {
        Debug.Log("PrintEvent: " + s + " called at: " + Time.time);
        Instantiate(floatingTextPrefab, transform.position, Quaternion.identity, transform);
    }
}
