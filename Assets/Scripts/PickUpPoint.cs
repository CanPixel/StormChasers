using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpPoint : MonoBehaviour {
    [ReadOnly] public bool hasPickUp;
    public PickUp currentPickUp; 

    /* private void Update() {
        
    } */

    public void ReleasePickUp() {
        currentPickUp.transform.position = new Vector3(transform.position.x, transform.position.y - 2, transform.position.z - 1.2f);
        currentPickUp.hasBeenPickedUp = false; 
    }

    public void DestroyPickUp() {
        Debug.Log("Destroyouyousefllllfff"); 
        if(currentPickUp.gameObject != null) Destroy(currentPickUp.gameObject);
        hasPickUp = false; 
    }
}
