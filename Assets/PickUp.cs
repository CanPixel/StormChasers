using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUp : MonoBehaviour
{

    public Transform pickUpPoint;
    public PickUpPoint pickUpPointScript;
    public bool hasBeenPickedUp = false;
    public float animationSpeed;

    public float degreesPerSecond = 15.0f;
    public float amplitude = 0.5f;
    public float frequency = 1f;

    // Position Storage Variables
    Vector3 posOffset = new Vector3();
    Vector3 tempPos = new Vector3();

    private void Start()
    {
        posOffset = transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasBeenPickedUp)
        {
            if (!pickUpPointScript.hasPickUp)
            {     
                PlacePickUp();
            }
            else ReplacePickUp();                     
        }
    }

    private void Update()
    {
        if(!hasBeenPickedUp) FloatingObject(); 
    }

    void ReplacePickUp()
    {
        pickUpPointScript.ReleasePickUp();
        pickUpPointScript.currentPickUp.parent = null;
        pickUpPointScript.currentPickUp.gameObject.layer = 0; 
        PlacePickUp();
    }

    void PlacePickUp()
    {
        transform.position = pickUpPoint.transform.position;
        transform.parent = pickUpPoint;
        hasBeenPickedUp = true;
        pickUpPointScript.hasPickUp = true;
        transform.gameObject.layer = 15; 

       
        pickUpPointScript.currentPickUp = transform; 
    }

    
    void FloatingObject()
    {
        //transform.position += new Vector3(0,1,0) * animationSpeed * Time.deltaTime; 
        transform.Rotate(new Vector3(0f, Time.deltaTime * degreesPerSecond, 0f), Space.World);

        // Float up/down with a Sin()
        tempPos = posOffset;
        tempPos.y += Mathf.Sin(Time.fixedTime * Mathf.PI * frequency) * amplitude;

        transform.position = tempPos;
    }
}
