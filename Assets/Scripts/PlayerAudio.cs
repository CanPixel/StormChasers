using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAudio : MonoBehaviour {
    public AudioSource splashSound, knockableSound;
    public AudioClip[] splashSounds, knockableSounds;

    void OnTriggerEnter(Collider col) {
        if(col.transform.tag == "SplashSound") {
            if(splashSound.isPlaying) return;
            splashSound.clip = splashSounds[Random.Range(0, splashSounds.Length)];
            splashSound.Play();
        } else if(col.transform.tag == "Knockable") {
            if(knockableSound.isPlaying) return;
            knockableSound.pitch = Random.Range(0.95f, 1.15f);
            knockableSound.clip = knockableSounds[Random.Range(0, knockableSounds.Length)];
            knockableSound.Play();
        }
    }

    void OnTriggerExit(Collider col) {
        if(col.transform.tag == "SplashSound") {
            if(splashSound.isPlaying) return;
            splashSound.clip = splashSounds[Random.Range(0, splashSounds.Length)];
            splashSound.Play();
        } 
    }
}
