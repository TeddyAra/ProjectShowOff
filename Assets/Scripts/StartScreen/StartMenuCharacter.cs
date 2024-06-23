using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class StartMenuCharacter : MonoBehaviour
{
    Rigidbody rb;
    AudioSource audioSource; 
    [SerializeField] private float moveSpeed; 
    [SerializeField] private float stepSoundInterval; 
    [SerializeField] List<AudioClip> footSteps = new List<AudioClip>();
    [SerializeField] List<AudioClip> slimeFootSteps = new List<AudioClip>();

    private Vector3 startPosition; 
    private bool canPlayStep = true; 

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>(); 
        startPosition = transform.position; 
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        rb.velocity = new Vector3 (moveSpeed * Time.deltaTime, 0, 0);
    }

    private void Update()
    {

        var randomStep = Random.Range(0, footSteps.Count);
        var randomJellyStep = Random.Range(0, slimeFootSteps.Count);

        // Play Normal FootSteps

        if (!canPlayStep)
            return; 

        audioSource.PlayOneShot(footSteps[randomStep]); 
        canPlayStep = false; 
        StartCoroutine(StepSoundCooldown()); 

        if (transform.position.x - startPosition.x > 600)
        {
            transform.position = startPosition; 
        }
    }



    IEnumerator StepSoundCooldown()
    {
        yield return new WaitForSeconds(stepSoundInterval); 
        canPlayStep = true; 
    }
}
