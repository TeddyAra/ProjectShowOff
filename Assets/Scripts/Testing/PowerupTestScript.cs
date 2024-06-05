using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PlayerControllerTestScript))]
public class PowerupTestScript : MonoBehaviour {
    [Header("Speed boost")]

    [Tooltip("The speed of a speed boost")]
    [SerializeField] private float speedboostSpeed;

    [Tooltip("For how long the speed boost should last")]
    [SerializeField] private float speedboostTime;

    [Tooltip("How long does it take for the player to slow down again")]
    [SerializeField] private float slowDownTime;

    // ----------------------------------------------------------------------------------

    [Header("Sleep bomb")]

    [Tooltip("The prefab for the sleep bomb")]
    [SerializeField] private GameObject sleepBombPrefab;

    [Tooltip("The point where the bomb should be spawned")]
    [SerializeField] private Transform sleepBombSpawnPoint;

    [Tooltip("The direction that the bomb is thrown to")]
    [SerializeField] private Vector2 throwDirection;

    [Tooltip("The force that the bomb is thrown with")]
    [SerializeField] private float throwForce;

    [Tooltip("The range of the explosion")]
    [SerializeField] private float explosionRange;

    [Tooltip("The minimum amount of time to be stunned")]
    [SerializeField] private float minStun;

    [Tooltip("The maximum amount of time to be stunned")]
    [SerializeField] private float maxStun;

    // ----------------------------------------------------------------------------------

    [Header("Fart boost")]

    [Tooltip("The prefab for the fart cloud")]
    [SerializeField] private GameObject fartCloudPrefab;

    [Tooltip("The amount of time the fart cloud will stay behind")]
    [SerializeField] private float fartCloudTime;

    [Tooltip("The amount of time players should be stunned when inside the fart cloud")]
    [SerializeField] private float stunTime;

    [Tooltip("The amount of time before the fart cloud gets active")] 
    [SerializeField] private float startupTime;

    [Tooltip("The direction the player should be pushed towards")]
    [SerializeField] private Vector2 forceDirection;

    [Tooltip("The amount of force to push the player with")]
    [SerializeField] private float force;

    // ----------------------------------------------------------------------------------

    [Header("Scare")]

    [Tooltip("The range of the scare")]
    [SerializeField] private float scareRange;

    [Tooltip("The amount of time to scare the players")]
    [SerializeField] private float scareTime;

    // ----------------------------------------------------------------------------------

    [Header("Windblast")]

    [Tooltip("How close people have to be to be blast back")]
    [SerializeField] private float blastRange;

    [Tooltip("With how much force others should be pushed back")]
    [SerializeField] private float blastForce;

    [Tooltip("With how much force the player should be pushed forward")]
    [SerializeField] private float blastBoost;

    // ----------------------------------------------------------------------------------

    [Header("Snow flight")]

    [Tooltip("For how long the player will fly")]
    [SerializeField] private float flyDuration;

    [Tooltip("The max velocity while flying")]
    [SerializeField] private float maxFlySpeed;

    [Tooltip("The force of the player while they fly")]
    [SerializeField] private float flyForce;

    [Tooltip("For how long the ice will last after the player stops flying")]
    [SerializeField] private float iceDuration;

    // ----------------------------------------------------------------------------------

    [Header("Audio")]

    [SerializeField] private AudioClip speedBoostSound;
    [SerializeField] private AudioClip throwSound;
    [SerializeField] private TrailRenderer trailRenderer;

    private float maxSpeed;

    // ---------------------------------------------------------------------------------

    // VFX 

    [SerializeField] private GameObject fartVFX;
    [SerializeField] private GameObject windBlastVFX;
    [SerializeField] private GameObject snowFlightVFX;
    [SerializeField] private GameObject scareVFX;

    // Ability UI Tooltip

    [SerializeField] private GameObject abilityBubble; 
    [SerializeField] private Image currentAbilityIcon;
    [SerializeField] private Sprite speedBoostSprite;
    [SerializeField] private Sprite sleepBombSprite;
    [SerializeField] private Sprite fireBallSprite;
    [SerializeField] private Sprite windBlastSprite; 
    [SerializeField] private Sprite fartSprite;
    [SerializeField] private Sprite scareSprite;
    [SerializeField] private Sprite iceSprite;

    // Animations

    [SerializeField] private Animator animator; 
    private float windBlastTimer; 


    public enum Powerup {
        None,
        Speedboost,
        SleepBomb,
        Fartboost,
        Scare,
        Windblast,
        SnowFlight
    }

    private List<Powerup> powerups;
    private Powerup currentPowerup;

    private PlayerControllerTestScript playerControllerScript;
    private PlayerControllerTestScript.Character character;

    private AudioSource audioSource;  

    private void Start() {
        powerups = Enum.GetValues(typeof(Powerup)).Cast<Powerup>().ToList();
        playerControllerScript = GetComponent<PlayerControllerTestScript>();

        audioSource = GetComponent<AudioSource>();  

        throwDirection.Normalize();
    }

    private void Update()
    {
        Debug.Log("windblast timer " + windBlastTimer); 
    }

    public Powerup GetCurrentPowerup() {
        return currentPowerup;
    }

    public void ApplyVariables(float maxSpeed, PlayerControllerTestScript.Character character) {
        this.maxSpeed = maxSpeed;
        this.character = character;
    }

    public void UsePowerup() {
        switch (currentPowerup) {
            case Powerup.None:
                break;

            case Powerup.Speedboost:
                StartCoroutine(SpeedUp());
                break;

            case Powerup.SleepBomb:
                SpawnSleepBomb();
                break;

            case Powerup.Fartboost:
                Fart();
                break;

            case Powerup.Scare:
                Scare();
                break;

            case Powerup.Windblast:
                Windblast();
                //StartCoroutine(Windblast());
                break;

            case Powerup.SnowFlight:
                SnowFlight();
                break;
        }

        currentPowerup = Powerup.None;
        abilityBubble.SetActive(false);
    }

    private void SnowFlight() {
        StartCoroutine(playerControllerScript.Fly(flyDuration, maxFlySpeed, flyForce, iceDuration));
        snowFlightVFX.SetActive(true);
        StartCoroutine(SnowFlightDelay()); 
    }

    private void Windblast()
    {
        windBlastTimer = 3 - Time.deltaTime; 
        animator.SetFloat("WindBlast", windBlastTimer); 
        animator.SetTrigger("PinguinoAbility"); 
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            if (player == gameObject) continue;
            if ((player.transform.position - transform.position).magnitude < blastRange &&
                player.transform.position.x < transform.position.x)
            {
                player.GetComponent<PlayerControllerTestScript>().AddForce(Vector3.left, blastForce);
            }
        }

        GetComponent<PlayerControllerTestScript>().AddForce(Vector3.right, blastBoost);
        windBlastVFX.SetActive(true);
        StartCoroutine(WindBlastVFXDelay());
        

    }

    // IEnumerator Windblast()
    //{
    //    animator.SetTrigger("PinguinoAbility"); 
    //    yield return new WaitForSeconds(1); 
    //    GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
    //    foreach (GameObject player in players) {
    //        if (player == gameObject) continue;
    //        if ((player.transform.position - transform.position).magnitude < blastRange &&
    //            player.transform.position.x < transform.position.x) {
    //            player.GetComponent<PlayerControllerTestScript>().AddForce(Vector3.left, blastForce);
    //        }
    //    }

    //    GetComponent<PlayerControllerTestScript>().AddForce(Vector3.right, blastBoost);
    //    windBlastVFX.SetActive(true);
    //    yield return new WaitForSeconds(2); 
    //    windBlastVFX.SetActive(false);
    //}

    private void Scare() {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        scareVFX.SetActive(true); 
        StartCoroutine(ScareVFXDelay()); 

        foreach (GameObject player in players) {
            if (player == gameObject) continue;
            if ((player.transform.position - transform.position).magnitude < scareRange) {
                StartCoroutine(player.GetComponent<PlayerControllerTestScript>().Scare(scareTime));
            }
        }
    }



    private void Fart() {
        FartCloudScript fartScript = Instantiate(fartCloudPrefab, transform.position, Quaternion.identity).GetComponent<FartCloudScript>();
        fartScript.ApplyVariables(stunTime, fartCloudTime, startupTime);
        playerControllerScript.AddForce(new Vector3(forceDirection.x, forceDirection.y, 0), force);
        fartVFX.SetActive(true);
        StartCoroutine(FartVFXDelay()); 
    }

    private void SpawnSleepBomb() {
        audioSource.PlayOneShot(throwSound);

        Rigidbody bomb = Instantiate(sleepBombPrefab, sleepBombSpawnPoint.position, Quaternion.identity).GetComponent<Rigidbody>();
        bomb.AddForce(new Vector3(throwDirection.x, throwDirection.y, 0) * throwForce);
        SleepBombTestScript bombScript = bomb.GetComponent<SleepBombTestScript>();
        bombScript.ApplyVariables(explosionRange, minStun, maxStun); 
        bombScript.audioSource = audioSource;
    }

    private IEnumerator SpeedUp() {
        audioSource.PlayOneShot(speedBoostSound); 
        trailRenderer.emitting = true;

        // Speed the player up
        playerControllerScript.ChangePlayerSpeed(speedboostSpeed);
        yield return new WaitForSeconds(speedboostTime);
        trailRenderer.emitting = false; 

        // Slowly make the player slow down again
        float timer = slowDownTime;
        while (timer > 0) {
            timer -= Time.deltaTime;
            playerControllerScript.ChangePlayerSpeed(maxSpeed + (speedboostSpeed - maxSpeed) * (timer / slowDownTime));
            yield return null;
        }
    }

    public string GetRandomPowerup() {
        int num = UnityEngine.Random.Range(1, powerups.Count);
        currentPowerup = powerups[num];

        // FOR DEBUGGING PURPOSES
        currentPowerup = Powerup.Windblast;

        abilityBubble.SetActive(true); 

        switch (currentPowerup)
        {
            case Powerup.Windblast:
                currentAbilityIcon.sprite = windBlastSprite; 
                break;
            case Powerup.Fartboost:
                currentAbilityIcon.sprite = fartSprite; 
                break; 
            case Powerup.Scare: 
                currentAbilityIcon.sprite = scareSprite; 
                break; 
            case Powerup.SnowFlight:
                currentAbilityIcon.sprite = iceSprite; 
                break; 
            case Powerup.Speedboost:
                currentAbilityIcon.sprite = speedBoostSprite; 
                break; 
            case Powerup.SleepBomb:
                currentAbilityIcon.sprite = sleepBombSprite;
                break; 
            
        }

        return currentPowerup.ToString();
    } 

    // VFX Timers 
    IEnumerator FartVFXDelay()
    {
        yield return new WaitForSeconds(2); 
        fartVFX.SetActive(false);
    }
    IEnumerator ScareVFXDelay()
    {
        yield return new WaitForSeconds(2);
        scareVFX.SetActive(false);
    }

    IEnumerator WindBlastVFXDelay()
    {
        yield return new WaitForSeconds(2);
        windBlastVFX.SetActive(false);
        windBlastTimer = 1; 
    }

    IEnumerator SnowFlightDelay()
    {
        yield return new WaitForSeconds(4);
        snowFlightVFX.SetActive(false);
    }
}