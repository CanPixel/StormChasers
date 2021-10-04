using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour {
    public static SoundManager instance;

    public AudioMixer unscaledMixer;
    public SoundBit[] unscaledSounds;

    public SoundBit[] sounds;
    public Dictionary<string, SoundBit> soundBits = new Dictionary<string, SoundBit>();
    public Dictionary<string, SoundBit> unscaledSoundBits = new Dictionary<string, SoundBit>();

    private AudioSource source;

    public AudioMixer[] auds;

    private static float pitchTarget;

    [System.Serializable]
    public class SoundBit {
        public string key;
        public AudioClip clip;
        [HideInInspector] public AudioSource source;
    }
    
    void Start() {
        source = GetComponent<AudioSource>();
        instance = this;
        foreach(var i in sounds) if(!soundBits.ContainsKey(i.key.ToLower().Trim())) soundBits.Add(i.key.ToLower().Trim(), i);

        foreach(var i in unscaledSounds) {
            var obj = new GameObject(i.key);
            obj.transform.SetParent(transform);
            var src = obj.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.clip = i.clip;
            src.outputAudioMixerGroup = unscaledMixer.outputAudioMixerGroup;
            i.source = src;
            unscaledSoundBits.Add(i.key.ToLower().Trim(), i);
        }
    }

    public static void PlaySound(string key, float vol = 1f) {
        if(instance == null) {
            Debug.LogError("[!] SoundManager is not loaded at the time PlaySound() gets called!");
            return;
        }
        if(instance.soundBits.ContainsKey(key.ToLower().Trim())) instance.source.PlayOneShot(instance.soundBits[key.ToLower().Trim()].clip, vol);
    }

    public static void PlayUnscaledSound(string key, float vol = 1f) {
        if(instance == null) {
            Debug.LogError("[!] SoundManager is not loaded at the time PlaySound() gets called!");
            return;
        }
        if(instance.unscaledSoundBits.ContainsKey(key.ToLower().Trim())) {
            instance.unscaledSoundBits[key.ToLower().Trim()].source.volume = vol;
            instance.unscaledSoundBits[key.ToLower().Trim()].source.Play();
        }
    }

    public static void SlowMo() {
        var val = Mathf.Clamp(Time.timeScale * 3f, 0.55f, 1);
        pitchTarget = val;
    }

    protected void UpdatePitch(float val) {
        source.pitch = val;
        foreach(var i in instance.auds) i.SetFloat("Pitch", val);
    }

    void Update() {
        UpdatePitch(Mathf.Lerp(source.pitch, pitchTarget, Time.unscaledDeltaTime * 8f));
    }
}
