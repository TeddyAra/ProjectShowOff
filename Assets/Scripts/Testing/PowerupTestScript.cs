using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;

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

    private float maxSpeed;

    private enum Powerup {
        None,
        Speedboost,
        SleepBomb
    }

    private List<Powerup> powerups;
    private Powerup currentPowerup;

    private PlayerControllerTestScript playerControllerScript;

    // Audio Stuff

    AudioSource audioSource;  
    [SerializeField] AudioClip speedBoostSound;
    [SerializeField] AudioClip throwSound;


    [SerializeField] private TrailRenderer trailRenderer; 


    private void Start() {
        powerups = Enum.GetValues(typeof(Powerup)).Cast<Powerup>().ToList();
        playerControllerScript = GetComponent<PlayerControllerTestScript>();

        audioSource = GetComponent<AudioSource>();  

        throwDirection.Normalize();
    }

    public void ApplyVariables(float maxSpeed) {
        this.maxSpeed = maxSpeed;
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
        }

        currentPowerup = Powerup.None;
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

    public void GetRandomPowerup() {
        int num = UnityEngine.Random.Range(1, powerups.Count);
        currentPowerup = powerups[num];
    } 
}