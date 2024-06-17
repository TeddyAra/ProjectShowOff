using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class SleepBombTestScript : MonoBehaviour {
    private float explosionRange;
    private float minStun;
    private float maxStun;

    private bool hitting; 

    // VFX
    [SerializeField] private GameObject bombVfx; 


    // Audio
    SFXManager sfxManager;

    private void Start()
    {
        sfxManager = FindObjectOfType<SFXManager>();
    }

    public void ApplyVariables(float explosionRange, float minStun, float maxStun) { 
        this.explosionRange = explosionRange;
        this.minStun = minStun;
        this.maxStun = maxStun;
    }

    private void OnCollisionEnter(Collision collision) {

        if (hitting)
            return; 
        sfxManager.Play("SleepBombNoHit"); 
        Debug.Log("Hitting nothing XD!");
        Instantiate(bombVfx, transform.position, transform.rotation);
        Explode();
        hitting = true;
    }

    private void OnTriggerEnter(Collider other) {
        if (other.tag == "PlayerTrigger" && !hitting) {
            sfxManager.Play("SleepBombHit"); 
            Instantiate(bombVfx, transform.position, transform.rotation);
            Explode();
            hitting = true; 
        }
    }

    private void Explode() {
        Destroy(gameObject);

        if (explosionRange == 0) return;

        PlayerControllerTestScript[] players = FindObjectsOfType<PlayerControllerTestScript>();

        foreach (PlayerControllerTestScript player in players) {
            float distance = (player.transform.position - transform.position).magnitude;
            if (distance <= explosionRange) {
                float stunTime = Mathf.Lerp(minStun, maxStun, distance / explosionRange);
                player.currentStunState = PlayerControllerTestScript.StunState.Slept; 
                player.Stun(stunTime);
            }
        }
    }
}