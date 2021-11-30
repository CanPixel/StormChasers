using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine.PostFX;
using UnityEngine.Rendering.PostProcessing;

[CreateAssetMenu(menuName = "DIALOG/CHARACTER")]
public class DialogCharacter : ScriptableObject {
    public string characterName;
    public Vector3 posOffset, rotOffset;

    public PostProcessProfile postProcessProfile;

    [HideInInspector] public Transform target;

    public Dictionary<Emotion, Voice> voiceByEmotion;

    [System.Serializable]
    public enum Emotion {
        NEUTRAL = 0, ANGRY, SAD, HAPPY, SCARED
    }
    [System.Serializable]
    public class Voice {
        [HideInInspector] public string name;
        public Emotion emotion;
        public AudioClip sample;
    }
    public Voice[] voices;

    public void LoadCharacter(Transform trans) {
        this.target = trans;
        if(voiceByEmotion != null) voiceByEmotion.Clear();
        voiceByEmotion = new Dictionary<Emotion, Voice>();
        foreach(var i in voices) voiceByEmotion.Add(i.emotion, i);
    }

    void OnValidate() {
        foreach(var i in voices) i.name = i.emotion.ToString();
    }
}
