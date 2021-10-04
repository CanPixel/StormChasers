using KartGame.KartSystems;
using UnityEngine;
using UnityEngine.Events;

public class StatBoost : MonoBehaviour {
    private Cinemachine.CinemachineVirtualCamera fl;
    public GameObject doorLeftStatic, doorRightStatic, doorLeft, doorRight;

    public ArcadeKart kart;

    public ArcadeKart.StatPowerup boostStats = new ArcadeKart.StatPowerup
    {
        MaxTime = 5
    };

    public bool isCoolingDown { get; private set; }
    public float lastActivatedTimestamp { get; private set; }

    public float cooldown = 5f;

    public const float fovChangeSpeed = 5f;

    public UnityEvent onPowerupActivated;
    public UnityEvent onPowerupFinishCooldown;

    private float fovTarget;

    private void Awake() {
        lastActivatedTimestamp = -9999f;
        fl = GameObject.FindGameObjectWithTag("Player").GetComponent<ArcadeKart>().look;
        fovTarget = fl.m_Lens.FieldOfView;
        SetHingesStatic(true);
    }

    private void Update() {
        doorLeftStatic.transform.localRotation = Quaternion.Lerp(doorLeftStatic.transform.localRotation, Quaternion.identity, Time.deltaTime * 3f);
        doorRightStatic.transform.localRotation = Quaternion.Lerp(doorRightStatic.transform.localRotation, Quaternion.identity, Time.deltaTime * 3f);

        fl.m_Lens.FieldOfView = Mathf.Lerp(fl.m_Lens.FieldOfView, fovTarget, Time.deltaTime * fovChangeSpeed);

        if (isCoolingDown) { 
            if (Time.time - lastActivatedTimestamp > cooldown) {
                //finished cooldown!
                isCoolingDown = false;
                onPowerupFinishCooldown.Invoke();
            }
        }
    }

    public void TriggerStatBoost() {
        if (isCoolingDown) return;
        lastActivatedTimestamp = Time.time;
        kart.AddPowerup(this.boostStats);
        onPowerupActivated.Invoke();
        isCoolingDown = true;
        SetHingesStatic(false);
    }

    public void IncreaseFOV(float fov) {
        fovTarget = fov;
        //fl.m_Lens.FieldOfView = fov;
    }

    public void SetHingesStatic(bool enabled) {
        doorLeftStatic.SetActive(enabled);
        doorRightStatic.SetActive(enabled);
        doorLeft.SetActive(!enabled);
        doorRight.SetActive(!enabled);
        doorLeftStatic.transform.localEulerAngles = doorLeft.transform.localEulerAngles;
        doorRightStatic.transform.localEulerAngles = doorRight.transform.localEulerAngles;
        doorLeft.transform.localEulerAngles = doorRight.transform.localEulerAngles = Vector3.zero;
    }
}
