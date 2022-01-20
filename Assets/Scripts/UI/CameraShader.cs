using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CameraShader : MonoBehaviour {
    public Material shader;
    
    void OnRenderImage(RenderTexture src, RenderTexture dst) {
        Graphics.Blit(src, dst, shader);
    }
}
