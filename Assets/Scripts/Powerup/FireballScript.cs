using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireballScript : MonoBehaviour {
    private int maxBounces;
    private int bounceCount;
    private float stunTime;
    private float gravity;
    private Rigidbody rb;

    private void Start() {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate() {
        rb.AddForce(Vector3.down * gravity);
    }

    public void ApplyVariables(int maxBounces, float stunTime, float gravity) { 
        this.maxBounces = maxBounces;
        this.stunTime = stunTime;
        this.gravity = gravity;
    }

    private void OnCollisionEnter(Collision collision) {
        bounceCount++;
        if (bounceCount >= maxBounces) { 
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.tag == "PlayerTrigger") {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            float distance = float.MaxValue;
            GameObject closest = null;

            foreach (GameObject p in players) {
                float newDistance = (p.transform.position - transform.position).magnitude;
                if (newDistance < distance) {
                    distance = newDistance;
                    closest = p;
                }
            }

            if (closest != null) closest.GetComponent<PlayerControllerTestScript>().Stun(stunTime);
            Destroy(gameObject);
        }
    }
}