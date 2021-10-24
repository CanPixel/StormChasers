using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpPoint : MonoBehaviour
{
    public bool hasPickUp;
    public Transform currentPickUp; 

    private void Update()
    {
        
    }

    public void ReleasePickUp()
    {
        currentPickUp.transform.position = new Vector3(transform.position.x, transform.position.y - 2, transform.position.z - 1.2f);
        currentPickUp.GetComponent<PickUp>().hasBeenPickedUp = false; 
    }

    public void DestroyPickUp()
    {
        Debug.Log("Destroyouyousefllllfff"); 
        Destroy(currentPickUp.gameObject);
        hasPickUp = false; 
    }
}
