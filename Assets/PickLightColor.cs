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
    public MeshRenderer lampMesh; 

    // Update is called once per frame
    void Update()
    {
        if (!isPink && !isGreen)
        {
            lampLight.enabled = false;
            lampBolbMat.DisableKeyword("_EMISSION");
        }
        else
        {
            lampBolbMat.EnableKeyword("_EMISSION"); 
            lampLight.enabled = true;
        }

        if (isPink)
        {
            lampLight.color = pinkLight.color;
            //lampMesh.material.SetColor("_EmissionColor", pinkLight.color * 2f);
            isGreen = false;
    
        }

        if (isGreen)
        {
            lampLight.color = greenLight.color;
          //  lampMesh.material.SetColor("_EmissionColor", greenLight.color);
            isPink = false; 
        }

   

      




    }
}
