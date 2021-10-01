using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour {
    public static SoundManager instance;

    public SoundBit[] sounds;
    public Dictionary<string, SoundBit> soundBits = new Dictionary<string, SoundBit>();

    private AudioSource source;

    [System.Serializable]
    public class SoundBit {
        public string key;
        public AudioClip clip;
    }
    
    void Start() {
        source = GetComponent<AudioSource>();
        instance = this;
        foreach(var i in sounds) if(!soundBits.ContainsKey(i.key.ToLower().Trim())) soundBits.Add(i.key.ToLower().Trim(), i);
    }

    public static void PlaySound(string key, float vol = 1f) {
        if(instance == null) {
            Debug.LogError("[!] SoundManager is not loaded at the time PlaySound() gets called!");
            return;
        }
        if(instance.soundBits.ContainsKey(key.ToLower().Trim())) instance.source.PlayOneShot(instance.soundBits[key.ToLower().Trim()].clip, vol);
    }
}
