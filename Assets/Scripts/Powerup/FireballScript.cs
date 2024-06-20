using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireballScript : MonoBehaviour {
    private int maxBounces;
    private int bounceCount;
    private float stunTime;
    private float gravity;
    private Rigidbody rb;

    SFXManager sfxManager; 


    private void Start() {
        rb = GetComponent<Rigidbody>();

        sfxManager = FindObjectOfType<SFXManager>();
    }

    private void FixedUpdate() {
        rb.AddForce(Vector3.down * gravity);
        transform.LookAt(transform.position + rb.velocity);
        if (rb.velocity.x < 0) {
            Vector3 euler = transform.eulerAngles;
            euler.z = 180;
            transform.eulerAngles = euler;
        }
    }

    public void ApplyVariables(int maxBounces, float stunTime, float gravity) { 
        this.maxBounces = maxBounces;
        this.stunTime = stunTime;
        this.gravity = gravity;
    }

    private void OnCollisionEnter(Collision collision) {
        //sfxManager.Play("FireballBounce"); 

        bounceCount++;
        if (bounceCount >= maxBounces) {
            sfxManager.Play("FireballSizzle"); 
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.tag == "PlayerTrigger") {
            PlayerControllerTestScript controllerScript = other.transform.parent.GetComponent<PlayerControllerTestScript>();
            if (controllerScript.character == PlayerControllerTestScript.Character.Catfire)
                return;
            controllerScript.currentStunState = PlayerControllerTestScript.StunState.Burnt; 
            controllerScript.Stun(stunTime);
            
            Destroy(gameObject);
        }
    }
}