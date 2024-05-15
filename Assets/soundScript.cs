using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class soundScript : MonoBehaviour
{
    Rigidbody rb;
    AudioSource audioSource;
    PlayerControllerTestScript playerControllerTestScript;

    public List<AudioClip> footSteps = new List<AudioClip>();



    private bool canPlayFootstep = true;

    [SerializeField] private float timeBetweenSteps; 

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

        var currentFootStep = Random.Range(0, footSteps.Count);
        if ((rb.velocity.x != 0) && canPlayFootstep == true && playerControllerTestScript.grounded == true)
        {
            canPlayFootstep = false;
            audioSource.PlayOneShot(footSteps[currentFootStep]); 
            StartCoroutine(FootStepDelay());
        }
    }

    IEnumerator FootStepDelay()
    {
        yield return new WaitForSeconds(timeBetweenSteps);
        canPlayFootstep = true; 
    }
}
