using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SensationScores : MonoBehaviour {
    [Header("Sensation Score Values")]
    public int angryHeadBoostValue = 10;
    public int angryHeadLaserValue = 20;
    public int knockableValue = 10;
    public int sharkValue = 20;
    public int flippedCarValue = 10;
    public int tornadoCarValue = 12;
    public int lightningValue = 15;
    public int explodedHydrantValue = 10;
    public int rhinoKnockBoostValue = 20;
    public int tornadoSombreroValue = 45;
    public int explosionValue = 20;

    public static SensationScores scores;

    void Start() {
        scores = this;
    }
}
