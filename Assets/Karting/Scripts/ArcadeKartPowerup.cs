using KartGame.KartSystems;
using UnityEngine;
using UnityEngine.Events;

public class ArcadeKartPowerup : MonoBehaviour {
    private Cinemachine.CinemachineVirtualCamera fl;

    public ArcadeKart.StatPowerup boostStats = new ArcadeKart.StatPowerup
    {
        MaxTime = 5
    };

    public bool isCoolingDown { get; private set; }
    public float lastActivatedTimestamp { get; private set; }

    public float cooldown = 5f;

    public bool disableGameObjectWhenActivated;
    public UnityEvent onPowerupActivated;
    public UnityEvent onPowerupFinishCooldown;

    private Vector3 baseScale;
    public float animateSpeed = 4f, animateRange = 0.1f;

    private void Awake() {
        baseScale = transform.localScale;
        lastActivatedTimestamp = -9999f;

        fl = GameObject.FindGameObjectWithTag("Player").GetComponent<ArcadeKart>().look;
    }

    private void Update()
    {
        transform.localScale = baseScale * (1f + Mathf.Sin(Time.time * animateSpeed) * animateRange);

        if (isCoolingDown) { 

            if (Time.time - lastActivatedTimestamp > cooldown) {
                //finished cooldown!
                isCoolingDown = false;
                onPowerupFinishCooldown.Invoke();
            }

        }
    }

    private void OnTriggerEnter(Collider other) {
        if (isCoolingDown) return;

        var rb = other.attachedRigidbody;
        if (rb) {
            var kart = rb.GetComponent<ArcadeKart>();
            if (kart && rb.velocity != Vector3.zero)
            { 
                lastActivatedTimestamp = Time.time;
                kart.AddPowerup(this.boostStats);
                onPowerupActivated.Invoke();
                isCoolingDown = true;

                if (disableGameObjectWhenActivated) this.gameObject.SetActive(false);
            }
        }
    }

    public void IncreaseFOV(float fov) {
        fl.m_Lens.FieldOfView = fov;
    }
}
