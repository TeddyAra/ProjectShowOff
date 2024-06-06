using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class SleepBombTestScript : MonoBehaviour {
    private float explosionRange;
    private float minStun;
    private float maxStun;

    // VFX
    [SerializeField] private GameObject bombVfx; 


    // Audio
    public AudioSource audioSource;
    [SerializeField] AudioClip sleepBombExplode; 
    [SerializeField] AudioClip sleepBombNoHit; 

    

    public void ApplyVariables(float explosionRange, float minStun, float maxStun) { 
        this.explosionRange = explosionRange;
        this.minStun = minStun;
        this.maxStun = maxStun;
    }

    private void OnCollisionEnter(Collision collision) {
        audioSource.PlayOneShot(sleepBombNoHit);
        Instantiate(bombVfx, transform.position, transform.rotation);
        Explode();
    }

    private void OnTriggerEnter(Collider other) {
        if (other.tag == "PlayerTrigger") {
            audioSource.PlayOneShot(sleepBombExplode);
            Instantiate(bombVfx, transform.position, transform.rotation);
            Explode();
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