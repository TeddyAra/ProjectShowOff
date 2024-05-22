using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class SleepBombTestScript : MonoBehaviour {
    private float explosionRange;
    private float minStun;
    private float maxStun;

    public void ApplyVariables(float explosionRange, float minStun, float maxStun) { 
        this.explosionRange = explosionRange;
        this.minStun = minStun;
        this.maxStun = maxStun;
    }

    private void OnCollisionEnter(Collision collision) {
        Debug.Log("Collider!");

        Explode();
    }

    private void OnTriggerEnter(Collider other) {
        Debug.Log("Trigger!");

        if (other.tag == "PlayerTrigger") 
            Explode();
    }

    private void Explode() {
        if (explosionRange == 0) return;

        PlayerControllerTestScript[] players = FindObjectsOfType<PlayerControllerTestScript>();
        Debug.Log("Explosion!");

        foreach (PlayerControllerTestScript player in players) {
            float distance = (player.transform.position - transform.position).magnitude;
            if (distance <= explosionRange) {
                float stunTime = Mathf.Lerp(minStun, maxStun, distance / explosionRange);
                player.Stun(stunTime);
            }
        }

        Destroy(gameObject);
    }
}