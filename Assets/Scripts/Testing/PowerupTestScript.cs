using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.InputSystem;
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

    [Header("Fireball")]

    [Tooltip("The prefab of the fireball")]
    [SerializeField] private GameObject fireballPrefab;

    [Tooltip("The direction that the fireball should go once it gets spawned")]
    [SerializeField] private Vector2 spawnDirection;

    [Tooltip("The force that the fireball should start out with")]
    [SerializeField] private float spawnForce;

    [Tooltip("For how long a player should be stunned if they're hit")]
    [SerializeField] private float burnTime;

    [Tooltip("After how many bounces a fireball should get destroyed")]
    [SerializeField] private int maxBounces;

    [Tooltip("The gravity applied onto the fireball")]
    [SerializeField] private float fireballGravity;

    [Tooltip("For how long the player can throw fireballs")]
    [SerializeField] private float fireballTime;

    [Tooltip("How long the player has to wait before they can spawn another fireball")]
    [SerializeField] private float fireballCooldown;

    [Tooltip("The speed of the character while they're using the fireball ability")]
    [SerializeField] private float fireballMovementSpeed;

    // ----------------------------------------------------------------------------------

    [Header("Extra")]

    [Tooltip("The maximum amount of points a character needs to have to use their ultimate ability")]
    [SerializeField] private int ultimatePoints;

    [Tooltip("How much time should pass before getting a point for staying alive")]
    [SerializeField] private int pointTime;

    [Tooltip("How many points you should get for staying alive")]
    [SerializeField] private int lifePoints;

    // ----------------------------------------------------------------------------------

    [Header("Audio")]

    SFXManager sfxManager; 

    [SerializeField] private TrailRenderer trailRenderer;
    

    private float maxSpeed;
    [SerializeField] private int abilityPoints;
    [SerializeField] private int racePoints;

    // ---------------------------------------------------------------------------------

    // VFX 

    [SerializeField] private GameObject fartVFX;
    [SerializeField] private GameObject windBlastVFX;
    [SerializeField] private GameObject snowFlightVFX;
    [SerializeField] private GameObject scareVFX;
    [SerializeField] private GameObject speedBoostVFX; 

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

    [SerializeField] private GameObject animatedBody; 
    [SerializeField] private Animator animator; 
    public bool isCastingWindBlast; 
    

    [Serializable]
    public enum Powerup {
        None,
        Speedboost,
        SleepBomb,
        Fartboost,
        Scare,
        Windblast,
        SnowFlight,
        Fireball
    }

    [Serializable]
    public struct Ultimate { 
        public PlayerControllerTestScript.Character character;
        public Powerup powerup;
    }

    [SerializeField] private List<Powerup> powerups;
    [SerializeField] private List<Ultimate> ultimates;

    private Powerup currentPowerup;
    private float pointTimer;

    private Gamepad gamepad;
    private PlayerControllerTestScript playerControllerScript;
    private PlayerControllerTestScript.Character character;

    private AudioSource audioSource;  

    private void Start() {
        playerControllerScript = GetComponent<PlayerControllerTestScript>();

        audioSource = GetComponent<AudioSource>();  
        sfxManager = FindObjectOfType<SFXManager>(); 

        throwDirection.Normalize();
        spawnDirection.Normalize();
    }

    private void Update() {
        pointTimer += Time.deltaTime;

        if (pointTimer >= pointTime) {
            pointTimer = 0;
            OnPoints(character, lifePoints);
        }

    }

    public Powerup GetCurrentPowerup() {
        return currentPowerup;
    }

    public void ApplyVariables(float maxSpeed, PlayerControllerTestScript.Character character, Gamepad gamepad) {
        this.maxSpeed = maxSpeed;
        this.character = character;
        this.gamepad = gamepad;
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
                break;

            case Powerup.SnowFlight:
                SnowFlight();
                break;

            case Powerup.Fireball:
                StartCoroutine(Fireball());
                break;
        }

        foreach (Ultimate ultimate in ultimates) {
            if (ultimate.powerup == currentPowerup) {
                abilityPoints = 0;
                break;
            }
        }

        currentPowerup = Powerup.None;
        abilityBubble.SetActive(false);
    }

    private IEnumerator Fireball() {
        playerControllerScript.ChangePlayerSpeed(fireballMovementSpeed);
        float timer = fireballTime;
        float cooldownTimer = 0;

        while (timer > 0 && gamepad.buttonWest.isPressed) {
            timer -= Time.deltaTime;
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0) {
                cooldownTimer = fireballCooldown;
                SpawnFireball();
            }
            yield return null;
        }

        timer = slowDownTime;
        while (timer > 0) {
            timer -= Time.deltaTime;
            playerControllerScript.ChangePlayerSpeed(maxSpeed + (fireballMovementSpeed - maxSpeed) * (timer / slowDownTime));
            yield return null;
        }
    }

    private void SpawnFireball() {
        audioSource.PlayOneShot(fireballSpawn);

        FireballScript fireball = Instantiate(fireballPrefab, sleepBombSpawnPoint.position, Quaternion.Euler(0, 90, 0)).GetComponent<FireballScript>();
        fireball.ApplyVariables(maxBounces, burnTime, fireballGravity);

        Rigidbody rb = fireball.GetComponent<Rigidbody>();
        rb.AddForce(spawnDirection * spawnForce);
    }



    private void SnowFlight() {
        StartCoroutine(playerControllerScript.Fly(flyDuration, maxFlySpeed, flyForce, iceDuration));
        snowFlightVFX.SetActive(true);
        sfxManager.Play("SnowFlight"); 
        StartCoroutine(SnowFlightDelay()); 
    }

    private void Windblast() {
        isCastingWindBlast = true; 
        animator.SetTrigger("PinguinoAbility"); 
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players) {
            if (player == gameObject) continue;

            if ((player.transform.position - transform.position).magnitude < blastRange &&
                player.transform.position.x < transform.position.x) {

                player.GetComponent<PlayerControllerTestScript>().AddForce(Vector3.left, blastForce);
            }
        }

        GetComponent<PlayerControllerTestScript>().AddForce(Vector3.right, blastBoost);
        windBlastVFX.SetActive(true);
        sfxManager.Play("WindBlast"); 
        StartCoroutine(WindBlastVFXDelay());
    }


    private void Scare() {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        scareVFX.SetActive(true);
        sfxManager.Play("Scare");
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
        sfxManager.Play("Fart"); 
        StartCoroutine(FartVFXDelay()); 
    }

    private void SpawnSleepBomb() {
        sfxManager.Play("Throw"); 

        Rigidbody bomb = Instantiate(sleepBombPrefab, sleepBombSpawnPoint.position, Quaternion.identity).GetComponent<Rigidbody>();
        bomb.AddForce(new Vector3(throwDirection.x, throwDirection.y, 0) * throwForce);
        SleepBombTestScript bombScript = bomb.GetComponent<SleepBombTestScript>();
        bombScript.ApplyVariables(explosionRange, minStun, maxStun); 

    }

    private IEnumerator SpeedUp() {
        sfxManager.Play("SpeedBoost"); 
        speedBoostVFX.SetActive(true);

        // Speed the player up
        playerControllerScript.ChangePlayerSpeed(speedboostSpeed);
        yield return new WaitForSeconds(speedboostTime);
        speedBoostVFX.SetActive(false); 

        // Slowly make the player slow down again
        float timer = slowDownTime;
        while (timer > 0) {
            timer -= Time.deltaTime;
            playerControllerScript.ChangePlayerSpeed(maxSpeed + (speedboostSpeed - maxSpeed) * (timer / slowDownTime));
            yield return null;
        }
    }

    public int GetPoints() {
        return racePoints;
    }

    public string GetRandomPowerup() {
        abilityBubble.SetActive(true);

        if (abilityPoints >= ultimatePoints) {
            foreach (Ultimate ultimate in ultimates) {
                if (character == ultimate.character) {
                    currentPowerup = ultimate.powerup;

                    switch (currentPowerup) {
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
                        case Powerup.Fireball: 
                            currentAbilityIcon.sprite = fireBallSprite; 
                            break; 
            
                    }
                    return ultimate.powerup.ToString();
                }
            }

            Debug.LogError($"There is no ultimate powerup for character {character}");
            return null;
        }

        int num = UnityEngine.Random.Range(0, powerups.Count);
        currentPowerup = powerups[num];

        switch (currentPowerup) {
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
    IEnumerator FartVFXDelay() {
        yield return new WaitForSeconds(2); 
        fartVFX.SetActive(false);
    }
    IEnumerator ScareVFXDelay() {
        yield return new WaitForSeconds(2);
        scareVFX.SetActive(false);
    }

    IEnumerator WindBlastVFXDelay() {

        while (animatedBody.transform.eulerAngles.y < 270)
        {
            animatedBody.transform.Rotate(Vector3.up, 15); 
            yield return null; 
        }

        yield return new WaitForSeconds(0.5f);

        windBlastVFX.SetActive(false);

        while (animatedBody.transform.eulerAngles.y > 90)
        {
            animatedBody.transform.Rotate(Vector3.up, -15); 
            yield return null; 
        }
        
        if (playerControllerScript.isFacingRight == false)
        {
            playerControllerScript.isFacingRight = true; 
        }

        isCastingWindBlast = false; 
    }

    IEnumerator SnowFlightDelay() {
        yield return new WaitForSeconds(4);
        snowFlightVFX.SetActive(false);
    }

    private void OnPoints(PlayerControllerTestScript.Character character, int points) {
        if (this.character == character) {
            abilityPoints += points;
            racePoints += points;
        }
    }

    public void GivePoints(int points) { 
        OnPoints(character, points);
    }

    private void OnEnable() {
        PlayerControllerTestScript.onPoints += OnPoints;
    }

    private void OnDisable() {
        PlayerControllerTestScript.onPoints -= OnPoints;
    }
}