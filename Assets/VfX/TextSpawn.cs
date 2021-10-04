using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class TextSpawn : MonoBehaviour
{
    public GameObject floatingTextPrefab;

    void CreateText()
    {
        Debug.Log("POK!");
        Instantiate(floatingTextPrefab, transform.position, Quaternion.identity,transform);
    }
}
