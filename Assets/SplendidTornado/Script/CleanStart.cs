using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CleanStart : MonoBehaviour {
    public GameObject ParticleObj; 

    void Start() {
        StartCoroutine(Clean());
    }

    IEnumerator Clean() {
        yield return new WaitForSeconds(0.01f);
        ParticleObj.SetActive(false);
        yield return new WaitForSeconds(0.01f);
        ParticleObj.SetActive(true);
    }
}
