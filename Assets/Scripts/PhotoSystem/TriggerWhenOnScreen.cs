using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TriggerWhenOnScreen : MonoBehaviour {
    public UnityEvent onScreen;

    public void OnScreen() {
        onScreen.Invoke();
    }
}
