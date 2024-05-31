using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(PowerupTestScript))]
public class PlayerControllerTestScript : MonoBehaviour {
    [Header("Movement")]

    [Tooltip("The force that should be applied to the player when they move")]
    [SerializeField] private float moveForce;

    [Tooltip("The force that should be applied to the player when they stop moving")]
    [SerializeField] private float moveDrag;

    [Tooltip("The maximum speed the player can reach")]
    [SerializeField] private float maxSpeed;

    [Tooltip("The maximum speed the player can fall")]
    [SerializeField] private float maxFallSpeed;

    [Tooltip("Whether the player should listen to input or not")]
    [SerializeField] private bool ignoreInput;

    // ----------------------------------------------------------------------------------
    [Header("Jumping")]

    [Tooltip("The force that should be applied to the player when they jump")]
    [SerializeField] private float jumpForce;

    [Tooltip("The extra force given every frame for holding the button")]
    [SerializeField] private float jumpBoost;

    [Tooltip("The maximum amount of time the player can hold the jump button to jump higher")]
    [SerializeField] private float jumpTime;

    [Tooltip("The amount of time in seconds the player is allowed to jump, despite not being grounded")]
    [SerializeField] private float coyoteTime;

    [Tooltip("The force that should be applied to the player when they hit a bounce pad")]
    [SerializeField] private float bouncePadForce; 

    [Tooltip("Gravity")]
    [SerializeField] private float gravity;

    // ----------------------------------------------------------------------------------
    [Header("Ground checking")]

    [Tooltip("The point where the ground should be checked")]
    [SerializeField] private Transform checkPoint;

    [Tooltip("The size of the ground check")]
    [SerializeField] private float groundCheckSize;

    [Tooltip("The name of the ground mask layer")]
    [SerializeField] private string groundMask;

    [Tooltip("The name of the bounce pad mask layer")]
    [SerializeField] private string bouncePadMask;

    [Tooltip("The size of the bounce pad Check")]
    [SerializeField] private float bounceCheckSize; 

    // ----------------------------------------------------------------------------------
    [Header("Extra")]

    [Tooltip("The amount of time the player is invincible after they're respawned")]
    [SerializeField] private float invincibilityTime;

    [Tooltip("The layer mask of the players")]
    [SerializeField] private LayerMask playerLayer;

    [SerializeField] private float playerDistance;

    [SerializeField] private AudioClip jumpLanding;

    // ----------------------------------------------------------------------------------

    // Movement variables
    private Rigidbody rb;
    private Vector3 velocity;
    public bool grounded;
    private int groundMaskInt;
    private int bouncePadMaskInt;
    private bool canBounce = true; 
    //private bool ignoreMaxSpeed = false; 

    // Input variables
    private Vector2 move;
    private bool jump;
    private bool holdingJump;
    private float coyoteTimer;
    private float jumpTimer;
    private Gamepad gamepad;
    private bool powerup;
    private bool reversed;

    // Death variables
    private bool frozen;
    private CapsuleCollider col;
    private float invincibilityTimer;
    private bool invincible;

    // Powerup script
    private PowerupTestScript powerupScript;

    // When a player reaches a checkpoint
    public delegate void OnCheckpoint();
    public static event OnCheckpoint onCheckpoint;

    // When a player needs the position of the next checkpoint
    public delegate Vector3 GetCheckpoint();
    public static event GetCheckpoint getCheckpoint;

    // When a player wins
    public delegate void OnFinish();
    public static event OnFinish onFinish;

    // When a player gets or uses a powerup
    public delegate void OnPowerup(PlayerControllerTestScript player, string powerup);
    public static event OnPowerup onPowerup;

    public delegate void OnReady(PlayerControllerTestScript player);
    public static event OnReady onReady;

    private float playerSpeed;
    private bool isColliding;
    private int playerNum;

    [SerializeField] private Image readyImage;
    [SerializeField] private TMP_Text readyText;
    private bool isStarting;

    // Sound stuff
    private AudioSource audioSource;

    
    private void Start() {
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();
        groundMaskInt = LayerMask.GetMask(groundMask);
        bouncePadMaskInt = LayerMask.GetMask(bouncePadMask); 

        col = GetComponent<CapsuleCollider>();
        playerSpeed = maxSpeed;

        powerupScript = GetComponent<PowerupTestScript>();
        powerupScript.ApplyVariables(maxSpeed);
    }

    private void Update() {
        if ((frozen || ignoreInput) && isStarting) return;

        // Controller input
        if (gamepad != null) {
            move = gamepad.leftStick.ReadValue();
            holdingJump = gamepad.buttonSouth.isPressed;
            if (gamepad.buttonSouth.wasPressedThisFrame) jump = true;
            if (gamepad.buttonWest.wasPressedThisFrame) powerup = true;

        // Keyboard input
        } else {
            bool left = Input.GetKey(KeyCode.A);
            bool right = Input.GetKey(KeyCode.D);
            move = left && !right ? Vector2.left : !left && right ? Vector2.right : Vector2.zero;

            holdingJump = Input.GetKey(KeyCode.Space);
            if (Input.GetKeyDown(KeyCode.Space)) jump = true;
            if (Input.GetKeyDown(KeyCode.E)) powerup = true;
        }

        if (reversed) move.x *= -1;

        isColliding = false;
    }

    private void FixedUpdate() {
        if (!grounded) {
            rb.AddForce(Vector3.down * gravity);
        }

        /*if (!isStarting) {
            if (holdingJump) {
                if (!isReady) {
                    isReady = true;
                    onReady?.Invoke(this);
                    readyText.text = "Ready!";
                    Debug.Log("Ready!");
                }

                readyImage.color = Color.white;
            } else {
                readyImage.color = new Color(1.0f, 1.0f, 1.0f, 0.25f);
            }

            jump = false;
        }

        if (!isReady || !isStarting) return;*/

        if (isStarting) return;

        // Update timers
        coyoteTimer--;
        jumpTimer--;

        if (invincible) {
            invincibilityTimer--;
            if (invincibilityTimer <= 0) {
                invincible = false;
                col.excludeLayers = LayerMask.GetMask("Nothing");
            }
        }

        // Check if the player is grounded or not
        if (Physics.CheckSphere(checkPoint.position, groundCheckSize, groundMaskInt)) {
            if (grounded == false) {
                audioSource.PlayOneShot(jumpLanding);
            }
            grounded = true;
        } else {
            if (grounded)
                coyoteTimer = coyoteTime * 60;

            grounded = false;
        }

        if (ignoreInput || frozen) return;

        // Get the rigid body's velocity
        velocity = rb.velocity;

        // Apply input
        if (move != Vector2.zero) {
            RaycastHit hit;
            Vector3 normal = Vector3.up;

            if (Physics.Raycast(checkPoint.position, Vector3.down, out hit, bounceCheckSize, groundMaskInt)) {
                normal = hit.normal;
            }

            Vector3 acc = move.x > 0 ? new Vector3(normal.y, -normal.x, 0) : new Vector3(-normal.y, normal.x, 0);

            if (velocity.x < 0 && acc.x > 0 || velocity.x > 0 && acc.x < 0) {
                velocity += acc * moveDrag;
            } else {
                velocity += acc * moveForce;
            }
        }

        // If there's no input and we're still moving
        if (move == Vector2.zero && velocity.x != 0 && grounded) {
            // Check which direction the player is going
            if (velocity.x < 0) {
                // Snap velocity to 0 if needed, otherwise add drag
                if (velocity.x + moveDrag > 0) velocity.x = 0;
                else velocity.x += moveDrag;
            }
            else if (velocity.x > 0) {
                if (velocity.x - moveDrag < 0) velocity.x = 0;
                else velocity.x -= moveDrag;
            }
        }

        // Make player jump
        if (jump && (grounded || coyoteTimer >= 0)) {
            jumpTimer = jumpTime * 60;
            rb.AddForce(Vector3.up * jumpForce);
        }


        // Give the player a boost if they're holding the button
        if (holdingJump && jumpTimer > 0) {
            rb.AddForce(Vector3.up * jumpBoost);
        }

        // Make sure players aren't going too fast
        Vector2 tempVelocity = velocity;
        tempVelocity.x = Mathf.Clamp(tempVelocity.x, -playerSpeed, playerSpeed);
        /*if (!ignoreMaxSpeed)*/ tempVelocity.y = Mathf.Clamp(tempVelocity.y, -maxFallSpeed, maxFallSpeed);
        velocity = tempVelocity;

        if (powerup) {
            powerupScript.UsePowerup();
            onPowerup?.Invoke(this, "");
        }

        // Apply the velocity
        rb.velocity = velocity;

        // Reset input variables
        jump = false;
        powerup = false;
    }

    private IEnumerator BouncePadDelay() {
        yield return new WaitForSeconds(0.8f); 
        canBounce = true; 
    }

    public IEnumerator Scare(float scareTime) {
        reversed = true;
        yield return new WaitForSeconds(scareTime);
        reversed = false;
    }

    public void ChangePlayerSpeed(float speed) {
        playerSpeed = speed;
    }

    public void ChangeGamepad(Gamepad gamepad, int playerNum) {
        this.gamepad = gamepad;
        this.playerNum = playerNum;
    }

    public void Stun(float stunTime) {
        StartCoroutine(StunCoroutine(stunTime));
    }

    public IEnumerator StunCoroutine(float stunTime) {
        Debug.Log($"{gameObject.name} stunned for " + stunTime);
        ignoreInput = true;
        yield return new WaitForSeconds(stunTime);
        Debug.Log($"{gameObject.name} no longer stunned");
        ignoreInput = false;
    }

    private void OnTriggerEnter(Collider other) {
        if (frozen) return;

        switch (other.tag) {
            // The player went too far left
            case "Death":
                frozen = true;
                transform.position = (Vector3)getCheckpoint?.Invoke();
                tag = "Untagged";
                rb.velocity = Vector3.zero;
                col.excludeLayers = playerLayer;
                rb.useGravity = false;
                break;

            // The player reached a checkpoint
            case "Checkpoint":
                if (!isColliding) {
                    onCheckpoint?.Invoke();
                    isColliding = true;
                }
                break;

            // The player got a powerup
            case "Powerup":
                if (powerupScript.GetCurrentPowerup() != PowerupTestScript.Powerup.None) return;
                string powerup = powerupScript.GetRandomPowerup();
                onPowerup?.Invoke(this, powerup);
                Destroy(other.gameObject);
                break;

            // The player reached the finish line
            case "Finish":
                onFinish?.Invoke();
                break;
        }
    }

    private void OnCollisionStay(Collision collision) {
        if (collision.gameObject.CompareTag("BouncePad")) {
            
            if (canBounce && checkPoint.position.y > collision.transform.position.y && 
                (checkPoint.position.x > collision.transform.position.x - collision.transform.localScale.x / 2) && 
                (checkPoint.position.x < collision.transform.position.x + collision.transform.localScale.x / 2)) {
                //StartCoroutine(DisableMaxSpeed());
                rb.AddForce(Vector3.up * bouncePadForce); 
                Debug.Log("Bouncing"); 
                canBounce = false; 
                StartCoroutine(BouncePadDelay()); 
            }
        }
    }

    /*IEnumerator DisableMaxSpeed() {
        ignoreMaxSpeed = true; 

        while (!grounded) {
            yield return null; 
        }

        ignoreMaxSpeed = false; 
    }*/

    public void OnFreeze() {
        if (!frozen) {
            // Freeze the player
            frozen = true;

            if (!rb) rb = GetComponent<Rigidbody>();

            rb.velocity = Vector3.zero;
            rb.useGravity = false;

            // Make sure the player is excluded from collisions
            tag = "Untagged";
            col.excludeLayers = playerLayer;
        }
    }

    private void OnUnfreeze() {
        if (frozen) {
            // Unfreeze the player
            frozen = false;
            rb.useGravity = true;

            // Include the player again
            tag = "Player";

            // Make the player invincible
            invincibilityTimer = invincibilityTime * 60;
            invincible = true;
        }
    }

    private void OnStart() {
        readyImage.transform.parent.gameObject.SetActive(false);
    }

    public void OnRespawn() {
        Vector3 spawnPoint = GameObject.FindGameObjectWithTag("SpawnPoint").transform.position + Vector3.left * playerDistance * playerNum; 
        transform.position = spawnPoint; 
    }

    public void AddForce(Vector3 direction, float force) {
        Debug.Log("Added force");
        direction.Normalize();
        rb.AddForce(direction * force);
    }

    private void OnEnable() {
        GameManager.onFreeze += OnFreeze;
        GameManager.onUnfreeze += OnUnfreeze;
        GameManager.onStart += OnStart;
        GameManager.onRespawn += OnRespawn; 

        rb = GetComponent<Rigidbody>();
        groundMaskInt = LayerMask.GetMask(groundMask);

        col = GetComponent<CapsuleCollider>();
        playerSpeed = maxSpeed;

        powerupScript = GetComponent<PowerupTestScript>();
        powerupScript.ApplyVariables(maxSpeed);
    }

    private void OnDisable() {
        GameManager.onFreeze -= OnFreeze;
        GameManager.onUnfreeze -= OnUnfreeze;
        GameManager.onStart -= OnStart;
        GameManager.onRespawn -= OnRespawn; 
    }
}