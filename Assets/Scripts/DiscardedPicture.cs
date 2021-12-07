using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscardedPicture : MonoBehaviour {
    private Material[] materials = new Material[3];
    public SpriteRenderer backside;
    public MeshRenderer cube;
    private SpriteRenderer front;

    private float timer = 0;
    public float FadeAfter = 10f;

    void Start() {
        front = GetComponent<SpriteRenderer>();
        materials[0] = front.material;
        materials[1] = backside.material;
        materials[2] = cube.material;
        backside.sprite = front.sprite;
    }

    void Update() {
        timer += Time.deltaTime;

        if(timer > FadeAfter) {
            foreach(var i in materials) {
                i.color = Color.Lerp(i.color, new Color(i.color.r, i.color.g, i.color.b, -0.1f), Time.deltaTime * 4f);
                if(i.color.a <= 0) {
                    Destroy(backside.sprite.texture);
                    Destroy(gameObject);
                }
            }
        }
    }
}
