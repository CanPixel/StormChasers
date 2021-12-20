using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogChar : MonoBehaviour {
    public string characterName;
    public UIFloat talkAnimation, hypeAnimation;
    public DialogCharacter characterInfo;
    public CarInteraction location;
    public DialogLine begin, end;

    public bool cycleModelThroughChildren = false;
    [ConditionalHide("cycleModelThroughChildren", true)] public GameObject[] characters;

    void Start() {
        if(cycleModelThroughChildren && characters != null) {
            foreach(var t in characters) t.SetActive(false);
            var randoChild = characters[Random.Range(0, characters.Length)];
            randoChild.SetActive(true);
        }
    }
}
