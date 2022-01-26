using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningPhoto : MonoBehaviour {
    public ParticleSystem particle;
    public PhotoItem pi;
    private int baseSensation;
    private string baseName;

    void Start() {
        baseSensation = pi.sensation;
        baseName = pi.tag;
    }

    void Update() {
        var cond = particle.particleCount > 0;
        pi.sensation = cond ? baseSensation : 0;
        pi.tag = cond ? baseName : "";
    }
}
