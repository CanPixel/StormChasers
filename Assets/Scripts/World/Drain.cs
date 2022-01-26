using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drain : MonoBehaviour {
    public Transform target;
    public GameObject blastMesh;
    public PhotoItem drainPhoto;

    public float powOffset = 2;
    [ReadOnly] public bool shouldNudgeAI = false;

    [Header("References")]
    public GameObject zibra;
    public ParticleSystem powParticle;
    public ParticleSystem rubbleParticle;
    public AudioSource blastSrc;
    public AINudger nudger;
    public GameObject waterGeyser;
    public Cinemachine.CinemachineImpulseSource impulseSource;

    private GameObject player;

    private Vector3 powBasePos;

    private bool triggered = false;

    public void TriggerShark() {
        waterGeyser.SetActive(true);
        rubbleParticle.Play();
        impulseSource.GenerateImpulseAt(transform.position, Vector3.one * 2);
        powParticle.Play();
        blastSrc.Play();
        triggered = true;
    }

    void Start() {
        player = GameObject.FindGameObjectWithTag("Player");
        powBasePos = powParticle.transform.position;
    }

    void Update() {
        if (player == null) return; 
        var playerDir = (transform.position - player.transform.position).normalized;
        playerDir.y = 0;
        powParticle.transform.position = powBasePos + playerDir * powOffset;

        nudger.enabled = shouldNudgeAI;
        drainPhoto.tag = ((triggered) ? "flyingdrain" : "");
    }

    public void Detrigger() {
        waterGeyser.SetActive(false);
        target = null;
        rubbleParticle.Stop();
        triggered = false;
    }

    void OnTriggerEnter(Collider col) {
        if(col.tag == "CarCivilian") target = col.transform;
    }

    void OnTriggerExit(Collider col) {
        if(col.tag == "CarCivilian") target = null;
    }
}
