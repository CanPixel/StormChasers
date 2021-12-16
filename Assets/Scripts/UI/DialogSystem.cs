using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Cinemachine.PostFX;
using UnityEngine.Events;

public class DialogSystem : MonoBehaviour {
    [ReadOnly] public DialogCharacter current;
    [ReadOnly] public bool triggered = false;

    private DialogChar[] dialogCharacters;
    public static Dictionary<string, DialogChar> dialogChars = new Dictionary<string, DialogChar>();

    public bool horizontal = true;

    public float AutoSkipAfter = 3f;
    public float typewriterSpeed = 2f;
    public float narrationVolume = 1f;

    public float splitSpeed = 8f;
    public float splitRange = 0.5f;

    private string targetText = "";

    private Dialog currentDialog;
    public List<Dialog> dialogs = new List<Dialog>();
    private Dictionary<string, Dialog> dialogByName = new Dictionary<string, Dialog>();

    [System.Serializable]
    public class Dialog {
        public string dialogName;
        public DialogCharacter host;

        [System.Serializable]
        public enum Orientation {
            LEFT = 0, RIGHT, UP, DOWN
        }
        public Orientation orientation;

        public Content[] content;

        [System.Serializable]
        public class Content {
            public string text;
            public DialogCharacter.Emotion emotion;
            public UnityEvent onSpeak;
        }
        [ReadOnly] public bool displayed = false;
    }

    [Space(10)]
    public Text typewriterText;
    public Text characterNameText;
    public Text skipText;
    public GameObject dialogObj;
    public InputActionReference skipBinding;
    public Camera mainCamera, dialogCamera;
    public AudioSource src;
    public Canvas cameraCanvas;
    public Cinemachine.CinemachineVirtualCamera virtualCamera;
    private Cinemachine.CinemachineOrbitalTransposer orbitalCam;
    private Cinemachine.CinemachineComposer composerCam;
    public CinemachinePostProcessing dialogPostProcess;
    private Vector3 baseFollowOffset, baseTrackedOffset;

    private float displayTime = 0, charIncreaseTime = 0, timeUntilNextDialog = 0;
    private int characterIndex = 0, contentIndex = 0;

    private float baseY, baseScale;
    private bool split = false;

    private bool runAudio = false;

    private Dialog.Orientation previousOrientation = Dialog.Orientation.UP;

    private DialogCharacter.Emotion emotion;

    void Start() {
        cameraCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        dialogByName.Clear();
        dialogChars.Clear();
        triggered = false;
        baseY = transform.localPosition.y;
        baseScale = transform.localScale.x;
        transform.localPosition = new Vector3(transform.localPosition.x, baseY * 2, transform.localPosition.z);
        foreach(var i in dialogs) {
            i.displayed = false;
            dialogByName.Add(i.dialogName.ToLower().Trim(), i); 
        }

        orbitalCam = virtualCamera.GetCinemachineComponent<Cinemachine.CinemachineOrbitalTransposer>();
        composerCam = virtualCamera.GetCinemachineComponent<Cinemachine.CinemachineComposer>();
        baseFollowOffset = orbitalCam.m_FollowOffset;
        baseTrackedOffset = composerCam.m_TrackedObjectOffset;

        mainCamera.rect = dialogCamera.rect = new Rect(0, 0, 1, 1);
    }
    
    void Update() {
        if(runAudio && !src.isPlaying) {
            src.pitch = Random.Range(0.9f, 1.1f);
            src.Play();
        }

        if(currentDialog != null && currentDialog.host != null) {
            string charName = currentDialog.host.characterName.ToLower().Trim();
            if(dialogChars.ContainsKey(charName) && dialogChars[charName].talkAnimation != null && dialogChars[charName].hypeAnimation != null) {
                if(emotion == DialogCharacter.Emotion.NEUTRAL) {
                    dialogChars[charName].talkAnimation.enabled = IsHostTalking();
                    dialogChars[charName].hypeAnimation.enabled = false;
                }
                else {
                    dialogChars[charName].hypeAnimation.enabled = IsHostTalking();
                    dialogChars[charName].talkAnimation.enabled = false;
                }
            }

            float restSplit = 1 - splitRange;
            switch(currentDialog.orientation) {
                case Dialog.Orientation.DOWN: 
                    mainCamera.rect = new Rect(0, Mathf.Lerp(mainCamera.rect.y, (split) ? splitRange : 0, Time.deltaTime * splitSpeed), 1, Mathf.Lerp(mainCamera.rect.height, (split) ? restSplit : 1, Time.deltaTime * splitSpeed));
                    dialogCamera.rect = new Rect(0, 0, 1, Mathf.Lerp(dialogCamera.rect.height, (split) ? splitRange : 1, Time.deltaTime * splitSpeed));
                break;
                case Dialog.Orientation.UP: 
                    dialogCamera.rect = new Rect(0, Mathf.Lerp(dialogCamera.rect.y, (split) ? restSplit : 0, Time.deltaTime * splitSpeed), 1, Mathf.Lerp(dialogCamera.rect.height, (split) ? splitRange : 1, Time.deltaTime * splitSpeed));
                    mainCamera.rect = new Rect(0, 0, 1, Mathf.Lerp(mainCamera.rect.height, (split) ? restSplit : 1, Time.deltaTime * splitSpeed));
                break;
                case Dialog.Orientation.LEFT: 
                    mainCamera.rect = new Rect(Mathf.Lerp(mainCamera.rect.x, (split) ? splitRange : 0, Time.deltaTime * splitSpeed), 0, Mathf.Lerp(mainCamera.rect.width, (split) ? restSplit : 1, Time.deltaTime * splitSpeed), 1);
                    dialogCamera.rect = new Rect(0, 0, Mathf.Lerp(dialogCamera.rect.width, (split) ? splitRange : 1, Time.deltaTime * splitSpeed), 1);
                break;
                case Dialog.Orientation.RIGHT: 
                    dialogCamera.rect = new Rect(Mathf.Lerp(dialogCamera.rect.x, (split) ? restSplit : 0, Time.deltaTime * splitSpeed), 0, Mathf.Lerp(dialogCamera.rect.width, (split) ? splitRange : 1, Time.deltaTime * splitSpeed), 1);
                    mainCamera.rect = new Rect(0, 0, Mathf.Lerp(mainCamera.rect.width, (split) ? restSplit : 1, Time.deltaTime * splitSpeed), 1);
                break;
            }
        } else {
            switch(previousOrientation) {
                case Dialog.Orientation.DOWN: 
                    dialogCamera.rect = new Rect(0, 0, 1, Mathf.Lerp(dialogCamera.rect.height, 0.01f, Time.deltaTime * splitSpeed));
                    mainCamera.rect = new Rect(0, Mathf.Lerp(mainCamera.rect.y, 0, Time.deltaTime * splitSpeed), Mathf.Lerp(mainCamera.rect.width, 1, Time.deltaTime * splitSpeed), Mathf.Lerp(mainCamera.rect.height, 1, Time.deltaTime * splitSpeed));
                break;
                case Dialog.Orientation.UP: 
                    dialogCamera.rect = new Rect(0, Mathf.Lerp(dialogCamera.rect.y, 0, Time.deltaTime * splitSpeed), Mathf.Lerp(dialogCamera.rect.width, 0.01f, Time.deltaTime * splitSpeed), Mathf.Lerp(dialogCamera.rect.height, 0.01f, Time.deltaTime * splitSpeed));
                    mainCamera.rect = new Rect(0, Mathf.Lerp(mainCamera.rect.y, 0, Time.deltaTime * splitSpeed), Mathf.Lerp(mainCamera.rect.width, 1, Time.deltaTime * splitSpeed), Mathf.Lerp(mainCamera.rect.height, 1, Time.deltaTime * splitSpeed));
                break;
                case Dialog.Orientation.LEFT: 
                    dialogCamera.rect = new Rect(Mathf.Lerp(dialogCamera.rect.x, 0, Time.deltaTime * splitSpeed), 0, Mathf.Lerp(dialogCamera.rect.width, 0.01f, Time.deltaTime * splitSpeed), Mathf.Lerp(dialogCamera.rect.height, 0.01f, Time.deltaTime * splitSpeed));
                    mainCamera.rect = new Rect(Mathf.Lerp(mainCamera.rect.x, 0, Time.deltaTime * splitSpeed), 0, Mathf.Lerp(mainCamera.rect.width, 1, Time.deltaTime * splitSpeed), Mathf.Lerp(mainCamera.rect.height, 1, Time.deltaTime * splitSpeed));
                break;
                case Dialog.Orientation.RIGHT: 
                    dialogCamera.rect = new Rect(Mathf.Lerp(dialogCamera.rect.x, 1, Time.deltaTime * splitSpeed), 0, Mathf.Lerp(dialogCamera.rect.width, 0.01f, Time.deltaTime * splitSpeed), Mathf.Lerp(dialogCamera.rect.height, 0.01f, Time.deltaTime * splitSpeed));
                    mainCamera.rect = new Rect(Mathf.Lerp(mainCamera.rect.x, 0, Time.deltaTime * splitSpeed), 0, Mathf.Lerp(mainCamera.rect.width, 1, Time.deltaTime * splitSpeed), Mathf.Lerp(mainCamera.rect.height, 1, Time.deltaTime * splitSpeed));
                break;
            }
            if(dialogCamera.rect.height <= 0.02f) dialogCamera.enabled = false;
        }

        skipText.text = "(<color='#ff0000'>" + skipBinding.action.GetBindingDisplayString() + "</color>)";

        if(current != null) {
            composerCam.m_TrackedObjectOffset = current.rotOffset;
            orbitalCam.m_FollowOffset = current.posOffset;
        } else {
            composerCam.m_TrackedObjectOffset = baseTrackedOffset;
            orbitalCam.m_FollowOffset = baseFollowOffset;
        }

        transform.localPosition = new Vector3(transform.localPosition.x, Mathf.Lerp(transform.localPosition.y, (triggered) ? baseY : baseY * 2, Time.deltaTime * 4f), transform.localPosition.z);
        transform.localScale = Vector3.one * Mathf.Lerp(transform.localScale.x, (triggered) ? baseScale : 0, Time.deltaTime * 4f);

        displayTime += Time.deltaTime;
        charIncreaseTime += Time.deltaTime * typewriterSpeed;
        if(charIncreaseTime > 1) {
            characterIndex++;
            characterIndex = Mathf.Clamp(characterIndex, 0, targetText.Length);
            charIncreaseTime = 0;
        }
        if(characterIndex >= targetText.Length) runAudio = false;

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

    private static int generatedDialogCount = 0;
    public static Dialog Create(DialogChar chara, DialogLine lines) {
        var d = new Dialog();
        d.dialogName = chara.characterName + " Dialog " + generatedDialogCount.ToString();
        d.host = chara.characterInfo;
        d.orientation = Dialog.Orientation.DOWN;
        d.content = lines.content;
        generatedDialogCount++;
        return d;
    }

    public void Initialize(Dialog d) {
        dialogs.Add(d);
    }

    public bool IsHostTalking() {
        return src == null || src.isPlaying;
    }

    public void CompleteTypewriterSentence() {
        runAudio = false;
        src.Stop();
        if(characterIndex >= targetText.Length) {
            contentIndex++;
            DisplayCurrentDialogIndex();
        }
        else characterIndex = targetText.Length;
    }

    private void EndDialog() {
        if(currentDialog != null) currentDialog.displayed = true;
        Reset();
    }

    public void Reset() {
        triggered = false;
        contentIndex = 0;
        current = null;
        currentDialog = null;
        split = false;
        cameraCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        runAudio = false;
    }

    public void DisplayDialog(string content, DialogCharacter character) {
        displayTime = charIncreaseTime = timeUntilNextDialog = 0;
        characterIndex = 0;
        this.current = character;
        if(dialogChars.ContainsKey(character.characterName.ToLower().Trim())) this.current.LoadCharacter(dialogChars[character.characterName.ToLower().Trim()].transform);
        if(this.current.postProcessProfile != null) dialogPostProcess.m_Profile = this.current.postProcessProfile;
        targetText = content;
        characterNameText.text = character.name;
        virtualCamera.m_Follow = virtualCamera.m_LookAt = character.target;
        triggered = true;
    }
    public void DisplayCurrentDialogIndex() {
        if(currentDialog == null) return;
        if(contentIndex >= currentDialog.content.Length) EndDialog();
        else {
            var currentSentence = currentDialog.content[contentIndex];
            DisplayDialog(currentSentence.text, currentDialog.host);

            /* StartCoroutine(DelayedAction(*/currentSentence.onSpeak.Invoke();//));

            var em = DialogCharacter.Emotion.NEUTRAL;
            emotion = currentSentence.emotion;
            if(currentDialog.host != null && currentDialog.host.voiceByEmotion.ContainsKey(currentSentence.emotion)) emotion = currentDialog.host.voiceByEmotion[currentSentence.emotion].emotion;

            var host = currentDialog.host;
            if(host.voiceByEmotion.ContainsKey(em)) {
                var voice = host.voiceByEmotion[em];
                if(voice != null && voice.sample != null) {
                    src.volume = narrationVolume;
                    src.clip = voice.sample;
                    src.Play();
                    runAudio = true;
                }
            }
        }
    }

    IEnumerator DelayedAction(UnityEvent post) {
        yield return new WaitForSeconds(0.2f);
        post.Invoke();
    }

    public void TriggerDialog(string dialogName) {
        var d = dialogName.Trim().ToLower();
        if(d.Length <= 0 || !dialogByName.ContainsKey(d) || split) return;
        contentIndex = 0;

        if(dialogByName[d].displayed) return;

        currentDialog = dialogByName[d];
        previousOrientation = currentDialog.orientation;

        cameraCanvas.renderMode = RenderMode.ScreenSpaceCamera;
        cameraCanvas.worldCamera = mainCamera;

        switch(currentDialog.orientation) {
            case Dialog.Orientation.DOWN:
                dialogCamera.rect = new Rect(0, 0, 0.01f, 0.01f);
                break;
            case Dialog.Orientation.UP:
                dialogCamera.rect = new Rect(0, 1, 0.01f, 0.01f);
                break;
            case Dialog.Orientation.RIGHT:
                dialogCamera.rect = new Rect(1, 0, 0.01f, 0.01f);
                break;
            case Dialog.Orientation.LEFT:
                dialogCamera.rect = new Rect(0, 0, 0.01f, 0.01f);
                break;
        }
        dialogCamera.enabled = true;    

        DisplayCurrentDialogIndex();
        split = true;
    }
}
