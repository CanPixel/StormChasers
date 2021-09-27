using KartGame.KartSystems;
using UnityEngine;
using UnityEngine.Events;

public class StatBoost : MonoBehaviour {
    private Cinemachine.CinemachineFreeLook fl;

    private ArcadeKart kart;

    public ArcadeKart.StatPowerup boostStats = new ArcadeKart.StatPowerup
    {
        MaxTime = 5
    };

    public bool isCoolingDown { get; private set; }
    public float lastActivatedTimestamp { get; private set; }

    public float cooldown = 5f;

    public UnityEvent onPowerupActivated;
    public UnityEvent onPowerupFinishCooldown;

    private void Awake()
    {
        kart = GetComponent<ArcadeKart>();
        lastActivatedTimestamp = -9999f;

        fl = GameObject.FindGameObjectWithTag("Player").GetComponent<ArcadeKart>().look;
    }

    private void Update() {
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
    }

    public void IncreaseFOV(float fov) {
        fl.m_Lens.FieldOfView = fov;
    }
}
