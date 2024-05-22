using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PowerupTestScript))]
public class PlayerControllerTestScript : MonoBehaviour {
    [Header("Movement")]

    [Tooltip("The force that should be applied to the player when they move")]
    [SerializeField] private float moveForce;

    [Tooltip("The force that should be applied to the player when they stop moving")]
    [SerializeField] private float moveDrag;

    [Tooltip("The maximum speed the player can reach")]
    [SerializeField] private float maxSpeed;

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

    // ----------------------------------------------------------------------------------
    [Header("Ground checking")]

    [Tooltip("The point where the ground should be checked")]
    [SerializeField] private Transform checkPoint;

    [Tooltip("The size of the ground check")]
    [SerializeField] private float groundCheckSize;

    [Tooltip("The name of the ground mask layer")]
    [SerializeField] private string groundMask;

    // ----------------------------------------------------------------------------------
    [Header("Extra")]

    [Tooltip("The amount of time the player is invincible after they're respawned")]
    [SerializeField] private float invincibilityTime;

    [Tooltip("The layer mask of the players")]
    [SerializeField] private LayerMask playerLayer;

    // ----------------------------------------------------------------------------------

    // Movement variables
    private Rigidbody rb;
    private Vector3 velocity;
    public bool grounded;
    private int groundMaskInt;

    // Input variables
    private Vector2 move;
    private bool jump;
    private bool holdingJump;
    private float coyoteTimer;
    private float jumpTimer;
    private Gamepad gamepad;
    private bool powerup;

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

    private float playerSpeed;


    // Sound stuff

    AudioSource audioSource;
    [SerializeField] private AudioClip jumpLanding; 

    
    private void Start() {
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();
        groundMaskInt = LayerMask.GetMask(groundMask);

        col = GetComponent<CapsuleCollider>();
        playerSpeed = maxSpeed;

        powerupScript = GetComponent<PowerupTestScript>();
        powerupScript.ApplyVariables(maxSpeed);
    }

    private void Update() {
        if (frozen || ignoreInput) return;

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
    }

    private void FixedUpdate() {
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

        if (ignoreInput || frozen) return;

        // Get the rigid body's velocity
        velocity = rb.velocity;

        // Check if the player is grounded or not
        if (Physics.CheckSphere(checkPoint.position, groundCheckSize, groundMaskInt)) {

            if (grounded == false)
            {
                audioSource.PlayOneShot(jumpLanding); 
            }
            grounded = true;
        } else {
            if (grounded)
                coyoteTimer = coyoteTime * 60;

            grounded = false;
        }

        // Apply input
        if (move != Vector2.zero) {
            RaycastHit hit;
            Vector3 normal = Vector3.up;

            if (Physics.Raycast(checkPoint.position, Vector3.down, out hit, groundCheckSize, groundMaskInt)) {
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
        velocity = Mathf.Clamp(velocity.magnitude, 0, playerSpeed) * velocity.normalized;

        if (powerup) {
            powerupScript.UsePowerup();
        }

        // Apply the velocity
        rb.velocity = velocity;

        // Reset input variables
        jump = false;
        powerup = false;
    }

    public void ChangePlayerSpeed(float speed) {
        playerSpeed = speed;
    }

    public void ChangeGamepad(Gamepad gamepad) {
        this.gamepad = gamepad;
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
                onCheckpoint?.Invoke();
                break;

            // The player got a powerup
            case "Powerup":
                powerupScript.GetRandomPowerup();
                break;

            // The player reached the finish line
            case "Finish":
                onFinish?.Invoke();
                break;
        }
    }

    private void OnFreeze() {
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

    private void OnEnable() {
        GameManager.onFreeze += OnFreeze;
        GameManager.onUnfreeze += OnUnfreeze;

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
    }
}