using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PickLightColor : MonoBehaviour
{
    // Start is called before the first frame update

    public bool isGreen;
    public bool isPink;

    public Light pinkLight;
    public Light greenLight;
    public Light lampLight;
    public Material lampBolbMat; 

    // Update is called once per frame
    void Update()
    {
        if (!isPink && !isGreen)
        {
            lampLight.enabled = false;
            //lampBolbMat.SetColor("_EmissionColor", Color.grey);
            lampBolbMat.DisableKeyword("_EMISSION");
        }
        else
        {
            lampBolbMat.EnableKeyword("_EMISSION"); 
            lampLight.enabled = true;
        }

        if (isGreen)
        {
            //greenBolb.SetActive(true);
            // greenLight.enabled = true;
            lampLight.color = greenLight.color;
            //lampBolb.material.color = greenLight.color; 
            lampBolbMat.SetColor("_EmissionColor", greenLight.color); 
           
           // pinkBolb.SetActive(false);

            //pinkLight.enabled = false;

            isPink = false; 
        }

        if (isPink)
        {
            lampLight.color = pinkLight.color;
            lampBolbMat.SetColor("_EmissionColor", pinkLight.color * 2f);
            isGreen = false;
        }

      




    }
}
