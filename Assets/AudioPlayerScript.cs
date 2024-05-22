using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlayerScript : MonoBehaviour
{

    Rigidbody rb;
    AudioSource audioSource; 
    PlayerControllerTestScript playerControllerTestScript;
    [SerializeField] List<AudioClip> footSteps = new List<AudioClip>();
    [SerializeField] List<AudioClip> slimeFootSteps = new List<AudioClip>();
    [SerializeField] private AudioClip jumpLanding;

    [SerializeField] private float stepSoundInterval; 

    private bool canPlayStep = true; 
    //private bool isFalling = false; 

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>(); 
        playerControllerTestScript = GetComponent<PlayerControllerTestScript>();   
    }

    // Update is called once per frame
    void Update()
    {


        var randomStep = Random.Range(0, footSteps.Count);
        var randomJellyStep = Random.Range(0, slimeFootSteps.Count);

        // Play Normal FootSteps

        while ((rb.velocity.x > 0 || rb.velocity.x < 0) && playerControllerTestScript.grounded == true)
        {
            if (canPlayStep == false)
            {
                return; 
            }

            audioSource.PlayOneShot(footSteps[randomStep]); 
            canPlayStep = false; 
            StartCoroutine(StepSoundCooldown()); 

        }

        // Play Jump Landing

        //if (rb.velocity.y < -1)
        //{
        //    isFalling = true; 
        //}

        //if (isFalling == true && rb.velocity.y >= 0)
        //{
        //    audioSource.PlayOneShot(jumpLanding); 
        //    isFalling = false; 
        //}

    }

    IEnumerator StepSoundCooldown()
    {
        yield return new WaitForSeconds(stepSoundInterval); 
        canPlayStep = true; 
    }
}
