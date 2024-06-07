using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireballScript : MonoBehaviour {
    private int maxBounces;
    private int bounceCount;
    private float stunTime;
    private float gravity;
    private Rigidbody rb;

    AudioSource playerAudio;

    [SerializeField] private AudioClip fireballBounce;
    [SerializeField] private AudioClip fireballSizzle;
    GameObject audioSourceObject;
    AudioSource audioSource;

    private void Start() {
        rb = GetComponent<Rigidbody>();

        playerAudio = GetComponent<AudioSource>();

        audioSourceObject = GameObject.FindWithTag("FireballSound");
        audioSource = audioSourceObject.GetComponent<AudioSource>();
    }

    private void FixedUpdate() {
        rb.AddForce(Vector3.down * gravity);

        //Quaternion desiredRotation = Quaternion.LookRotation(rb.velocity); 

        //rb.rotation = desiredRotation; 
    }

    public void ApplyVariables(int maxBounces, float stunTime, float gravity) { 
        this.maxBounces = maxBounces;
        this.stunTime = stunTime;
        this.gravity = gravity;
    }

    private void OnCollisionEnter(Collision collision) {
        playerAudio.PlayOneShot(fireballBounce);

        bounceCount++;
        if (bounceCount >= maxBounces) {
            audioSource.PlayOneShot(fireballSizzle);
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.tag == "PlayerTrigger") {
            other.transform.parent.GetComponent<PlayerControllerTestScript>().Stun(stunTime);
            Destroy(gameObject);
        }
    }
}