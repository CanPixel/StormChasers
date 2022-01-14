using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HapticManager : MonoBehaviour {
    public static HapticManager self;

    public CarMovement car;

    public Dictionary<string, HapticFeedback> hapticDict = new Dictionary<string, HapticFeedback>();
    public HapticFeedback[] haptics;

    public static HapticFeedback ERROR = new HapticFeedback("error");

    [System.Serializable]
    public class HapticFeedback {
        public string action;
        public float lowFreq = 0.25f, hiFreq = 0.75f, duration = 1;
    
        public HapticFeedback(string action) {
            this.action = action;
        }

        public HapticFeedback SetHaptic(float lowFreq, float hiFreq, float duration) {
            this.lowFreq = lowFreq;
            this.hiFreq = hiFreq;
            this.duration = duration;
            return this;
        }
    }

    void OnApplicationQuit() {
        InputSystem.ResetHaptics();
    }

    void Start() {
        self = this;    
        foreach(var i in haptics) hapticDict.Add(i.action.ToLower().Trim(), i);
    }

    public static HapticFeedback GetHaptics(string action) {
        if(self.hapticDict.ContainsKey(action.ToLower().Trim())) return self.hapticDict[action.ToLower().Trim()];
        Debug.LogError("HapticFeedback action: " + action + " not found!");
        return ERROR;
    }

    public static void Haptics(string action) {
        var hap = GetHaptics(action);
        self.car.HapticFeedback(hap);
    }

    public static void ManualHaptics(float lowFreq, float hiFreq, float duration) {
        self.car.HapticFeedback(lowFreq, hiFreq, duration);
    }
}
