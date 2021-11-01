using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class DialogSystem : MonoBehaviour {
    [ReadOnly] public DialogCharacter current;
    [ReadOnly] public bool triggered = false;

    public float AutoSkipAfter = 3f;
    public float typewriterSpeed = 2f;

    private string targetText = "";

    private Dialog currentDialog;
    public Dialog[] dialogs;
    private Dictionary<string, Dialog> dialogByName = new Dictionary<string, Dialog>();

    [System.Serializable]
    public class Dialog {
        public string dialogName;
        public Content[] content;
        public DialogCharacter host;

        [System.Serializable]
        public class Content {
            public string text;
            public AudioClip audio;
        }

        [HideInInspector] public bool displayed = false;
    }

    [System.Serializable]
    public class DialogCharacter {
        public string name;
        public Transform target;
        public Vector3 rotOffset, posOffset;

        public DialogCharacter(string name, Transform target, Vector3 posOffs, Vector3 rotOffset) {
            this.name = name;
            this.target = target;
            this.rotOffset = rotOffset;
            this.posOffset = posOffs;
        }
        public DialogCharacter(string name, Transform target) : this(name, target, Vector3.zero, Vector3.zero) {}
        public DialogCharacter(string name, Transform target, Vector3 posOffs) : this(name, target, posOffs, Vector3.zero) {}
    }   

    [Space(10)]
    public Text typewriterText;
    public Text characterNameText;
    public Text skipText;
    public RawImage avatarImg;
    public GameObject avatarObj, dialogObj;
    public InputActionReference skipBinding;
    public Camera dialogCam;
    public Cinemachine.CinemachineVirtualCamera virtualCamera;

    private float displayTime = 0, charIncreaseTime = 0, timeUntilNextDialog = 0;
    private int characterIndex = 0, contentIndex = 0;

    private float baseY, baseScale;
    
    void Start() {
        triggered = false;
        baseY = transform.localPosition.y;
        baseScale = transform.localScale.x;
        transform.localPosition = new Vector3(transform.localPosition.x, baseY * 2, transform.localPosition.z);
        foreach(var i in dialogs) dialogByName.Add(i.dialogName.ToLower().Trim(), i); 
    }
    
    void Update() {
        skipText.text = "(<color='#ff0000'>" + skipBinding.action.GetBindingDisplayString() + "</color>)";

        transform.localPosition = new Vector3(transform.localPosition.x, Mathf.Lerp(transform.localPosition.y, (triggered) ? baseY : baseY * 2, Time.deltaTime * 4f), transform.localPosition.z);
        transform.localScale = Vector3.one * Mathf.Lerp(transform.localScale.x, (triggered) ? baseScale : 0, Time.deltaTime * 4f);

        displayTime += Time.deltaTime;
        charIncreaseTime += Time.deltaTime * typewriterSpeed;
        if(charIncreaseTime > 1) {
            characterIndex++;
            characterIndex = Mathf.Clamp(characterIndex, 0, targetText.Length);
            charIncreaseTime = 0;
        }

        if(targetText.Length > 0) {
            typewriterText.text = targetText.Substring(0, characterIndex);

            if(characterIndex >= targetText.Length) {
                timeUntilNextDialog += Time.deltaTime;

                if(timeUntilNextDialog > AutoSkipAfter) {
                    contentIndex++;
                    DisplayCurrentDialogIndex();
                }
            }
        }
    }

    public void CompleteTypewriterSentence() {
        if(characterIndex >= targetText.Length) {
            contentIndex++;
            DisplayCurrentDialogIndex();
        }
        else characterIndex = targetText.Length;
    }

    private void EndDialog() {
        currentDialog.displayed = true;
        triggered = false;
        contentIndex = 0;
        current = null;
        currentDialog = null;
    }

    public void DisplayDialog(string content, DialogCharacter character) {
        displayTime = charIncreaseTime = timeUntilNextDialog = 0;
        characterIndex = 0;
        this.current = character;
        targetText = content;
        characterNameText.text = character.name;
        virtualCamera.m_Follow = virtualCamera.m_LookAt = character.target;
        triggered = true;
    }
    public void DisplayCurrentDialogIndex() {
        if(currentDialog == null) return;
        if(contentIndex >= currentDialog.content.Length) EndDialog();
        else {
            DisplayDialog(currentDialog.content[contentIndex].text, currentDialog.host);
            //AUDIO PLAY
        }
    }

    public void TriggerDialog(string dialogName) {
        var d = dialogName.Trim().ToLower();
        if(d.Length <= 0 || !dialogByName.ContainsKey(d) || dialogByName[d].displayed) return;
        contentIndex = 0;
        currentDialog = dialogByName[d];
        DisplayCurrentDialogIndex();
    }
}
