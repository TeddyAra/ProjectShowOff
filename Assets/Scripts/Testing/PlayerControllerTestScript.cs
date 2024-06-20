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

    [Tooltip("Speed of player when they enter draft")]
    [SerializeField] private float draftSpeed;

    [Tooltip("How long does it take for the player to slow down again")]
    [SerializeField] private float slowDownTime;

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

    [Header("Points")]

    [Tooltip("How many points should be deducted for dying")]
    [SerializeField] private int deathPoints;

    [Tooltip("How many points you get when you reach a checkpoint")]
    [SerializeField] private int checkpointPoints;

    // ----------------------------------------------------------------------------------

    [Header("Extra")]

    [Tooltip("The amount of time the player is invincible after they're respawned")]
    [SerializeField] private float invincibilityTime;

    [Tooltip("The layer mask of the players")]
    [SerializeField] private LayerMask playerLayer;

    [Tooltip("The prefab of the ice")]
    [SerializeField] private GameObject icePrefab;

    [SerializeField] private float playerDistance;


    // ----------------------------------------------------------------------------------

    // Movement variables
    private Rigidbody rb;
    private Vector3 velocity;
    public bool grounded;
    private int groundMaskInt;
    private int bouncePadMaskInt;
    private bool canBounce = true;
    private bool finished;
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

    // When a player has done something to get or lose points
    public delegate void OnPoints(Character character, int points, bool ignoreUI = false);
    public static event OnPoints onPoints;

    private float playerSpeed;
    private float baseSpeed;
    private bool isColliding;
    private int playerNum;

    [Serializable]
    public enum Character { 
        Iceage,
        Catfire,
        Catnap,
        Stinkozila,
        Pinguino
    }

    [Header("Extra")]

    public Character character;
    [SerializeField] private TMP_Text readyText;
    private bool isStarting;

    private bool flying;
    private Transform lastIcePlatform;
    private Vector3 firstIcePosition;
    private Vector3 lastIcePosition;
    private Transform ice;
    private List<Transform> icePlatforms;

    // Sound stuff

    SFXManager sfxManager; 
    private bool windDraft;
    private bool canPlayStun = true; 

    // Animation stuff
    [SerializeField] Animator animator;
    [SerializeField] GameObject characterVisualBody;
    public bool isFacingRight = true;
    [SerializeField] private float minRunAnimSpeed; 

    // Stun States (for animations and VFX) 

    public enum StunState {
        None, 
        Slept,
        Frozen, 
        Burnt
    }

    public StunState currentStunState; 

    [SerializeField] private GameObject sleepVFX;
    [SerializeField] private GameObject freezeVFX;
    [SerializeField] private GameObject burnVFX; 

    private void Start() {
        DontDestroyOnLoad(gameObject);

        sfxManager = FindObjectOfType<SFXManager>();

        rb = GetComponent<Rigidbody>();
        groundMaskInt = LayerMask.GetMask(groundMask);
        bouncePadMaskInt = LayerMask.GetMask(bouncePadMask); 

        col = GetComponent<CapsuleCollider>();
        playerSpeed = maxSpeed;

        powerupScript = GetComponent<PowerupTestScript>();
        powerupScript.ApplyVariables(maxSpeed, character, gamepad);

        icePlatforms = new List<Transform>();
    }

    private void Update() {
        if ((frozen || ignoreInput) && isStarting) return;

        // Controller input
        if (gamepad != null) {
            move = gamepad.leftStick.ReadValue();
            holdingJump = gamepad.buttonSouth.isPressed;
            if (gamepad.buttonSouth.wasPressedThisFrame && !ignoreInput) jump = true;
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

        // Animator Controller Stuff
        animator.SetFloat("Speed", minRunAnimSpeed + Mathf.Abs(rb.velocity.x / maxSpeed)); 
        animator.SetFloat("FallSpeed", rb.velocity.y); 
        
        if (grounded) {
            animator.SetBool("Grounded", true); 
        } else {
            animator.SetBool("Grounded", false); 
        }

    }

    private void FixedUpdate() {
        if (!grounded && !flying) {
            rb.AddForce(Vector3.down * gravity);
        }

        if (isStarting || flying) return;

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
                sfxManager.Play("JumpLanding"); 
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
            animator.ResetTrigger("Jumping"); 
            animator.SetTrigger("Jumping"); 
            jumpTimer = jumpTime * 60;
            rb.AddForce(Vector3.up * jumpForce);

            switch (character)
            {
                case Character.Catfire:
                    sfxManager.Play("catfireJump"); 
                    break; 
                case Character.Pinguino:
                    sfxManager.Play("pinguinoJump"); 
                    break; 
                case Character.Catnap:
                    sfxManager.Play("catnapJump"); 
                    break; 
                case Character.Stinkozila:
                    sfxManager.Play("stinkozilaJump");
                    break; 
                case Character.Iceage:
                    sfxManager.Play("iceageJump"); 
                    break; 
            }
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

        Flip(); 
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
        if (canPlayStun) {
            StartCoroutine(StunCoroutine(stunTime));
        }
    }

    public IEnumerator StunCoroutine(float stunTime) {
        canPlayStun = false; 

        // Character VFX stun states
        switch (currentStunState) {
            case StunState.None: 
                break; 
            case StunState.Slept: 
                sleepVFX.SetActive(true); 
                break; 
            case StunState.Burnt: 
                burnVFX.SetActive(true);
                break; 
            case StunState.Frozen: 
                freezeVFX.SetActive(true);
                break; 
        }

        // Character stun voicelines
        switch (character) {
            case Character.Catfire:
                sfxManager.Play("catfireHit"); 
                break;
            case Character.Pinguino:
                sfxManager.Play("pinguinoHit"); 
                break; 
            case Character.Catnap:
                sfxManager.Play("catnapHit"); 
                break; 
            case Character.Stinkozila:
                sfxManager.Play("stinkozilaHit"); 
                break; 
            case Character.Iceage:
                sfxManager.Play("iceageOuch"); 
                break;
        }

        animator.SetBool("Stunned", true); 
        
        ignoreInput = true;
        yield return new WaitForSeconds(stunTime);
        ignoreInput = false;

        animator.SetBool("Stunned", false); 

        // Character VFX stun states

        switch (currentStunState) {
            case StunState.None: 
                break; 
            case StunState.Slept: 
                sleepVFX.SetActive(false); 
                currentStunState = StunState.None;
                break; 
            case StunState.Burnt: 
                burnVFX.SetActive(false);
                currentStunState = StunState.None;
                break; 
            case StunState.Frozen: 
                freezeVFX.SetActive(false);
                currentStunState = StunState.None;
                break; 
        }

        canPlayStun = true; 
    }

    private void OnTriggerStay(Collider other) {
        if (other.tag == "Ice" && character != Character.Iceage) {
            currentStunState = StunState.Frozen; 
            Stun(1f);
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (frozen || finished) return;

        switch (other.tag) {
            // The player went too far left
            case "Death":
                onPoints?.Invoke(character, deathPoints);
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
                    //onPoints?.Invoke(character, checkpointPoints);
                    onCheckpoint?.Invoke();
                    isColliding = true;
                    powerupScript.GivePoints(checkpointPoints);
                }
                break;

            // The player got a powerup
            case "Powerup":
                if (powerupScript.GetCurrentPowerup() != PowerupTestScript.Powerup.None || powerupScript.UsingPowerup()) return;
                string powerup = powerupScript.GetRandomPowerup();
                onPowerup?.Invoke(this, powerup);
                Destroy(other.gameObject);
                sfxManager.Play("PowerupPickup"); 
                break;

            // The player reached the finish line
            case "Finish":
                StartCoroutine(FinishLine());
                onFinish?.Invoke();
                finished = true;
                ignoreInput = true;
                break;
            case "Lever":
                sfxManager.Play("Lever"); 
                break; 
        }

        //Player enters a wind draft
        if (other.gameObject.CompareTag("WindDraft") && !windDraft) {
            playerSpeed = draftSpeed;
            windDraft = true;

            switch (character)
            {
                case Character.Catfire:
                    sfxManager.Play("catfireSpeedboost"); 
                    break; 
                case Character.Catnap:
                    sfxManager.Play("catnapSpeedboost"); 
                    break; 
                case Character.Pinguino:
                    sfxManager.Play("pinguinoSpeedboost"); 
                    break; 
                case Character.Iceage:
                    sfxManager.Play("iceageSpeedboost"); 
                    break; 
                case Character.Stinkozila:
                    sfxManager.Play("stinkozilaSpeedboost"); 
                    break; 
            }

            StartCoroutine(ResetWindraft(1));
        }
    }

    IEnumerator FinishLine() {
        switch (character) {
            case Character.Catfire: 
                sfxManager.Play("catfireWin"); 
                break;
            case Character.Catnap: 
                sfxManager.Play("catnapWin"); 
                break;
            case Character.Pinguino: 
                sfxManager.Play("pinguinoIdidit"); 
                break;
            case Character.Stinkozila: 
                sfxManager.Play("stinkozilaWin"); 
                break;
            case Character.Iceage: 
                sfxManager.Play("iceageWin"); 
                break;
        }
        yield return new WaitForSeconds(0.5f);
        sfxManager.Play("VictorySound"); 
    }
    IEnumerator ResetWindraft(float resetTime) {
        yield return new WaitForSeconds(resetTime);
        ChangePlayerSpeed(baseSpeed);
        windDraft = false;

        // Slowly make the player slow down again
        float timer = slowDownTime;
        while (timer > 0) {
            timer -= Time.deltaTime;
            ChangePlayerSpeed(maxSpeed + (draftSpeed - maxSpeed) * (timer / slowDownTime));
            yield return null;
        }
    }

    private void OnCollisionStay(Collision collision) {
        if (collision.gameObject.CompareTag("BouncePad")) {
            animator.ResetTrigger("Jumping"); 
            animator.SetTrigger("Jumping"); 
            if (canBounce && checkPoint.position.y > collision.transform.position.y && 
                (checkPoint.position.x > collision.transform.position.x - collision.transform.localScale.x / 2) && 
                (checkPoint.position.x < collision.transform.position.x + collision.transform.localScale.x / 2)) {
                //StartCoroutine(DisableMaxSpeed());
                rb.AddForce(Vector3.up * bouncePadForce); 
                sfxManager.Play("BounceLeaf"); 
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

            animator.SetFloat("Speed", 0); 

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
            jump = false;

            // Include the player again
            tag = "Player";

            // Make the player invincible
            invincibilityTimer = invincibilityTime * 60;
            invincible = true;
        }
    }

    public void OnRespawn(List<PlayerControllerTestScript> positions) {
        Vector3 spawnPoint = GameObject.FindGameObjectWithTag("SpawnPoint").transform.position + Vector3.left * playerDistance * playerNum;//positions.FindIndex(x => x == this); 
        transform.position = spawnPoint;
        ignoreInput = false;
    }

    public void AddForce(Vector3 direction, float force) {
        direction.Normalize();
        rb.AddForce(direction * force);
    }

    public IEnumerator Fly(float flyDuration, float maxFlySpeed, float flyForce, float iceDuration) {
        float timer = flyDuration;
        flying = true;
        rb.velocity = Vector3.zero;

        RaycastHit hit;
        if (Physics.Raycast(checkPoint.position, Vector3.down, out hit, 1000f, groundMaskInt)) { 
            firstIcePosition = hit.point;
        }

        while (timer > 0) {
            timer -= Time.deltaTime;

            if (Physics.Raycast(checkPoint.position, Vector3.down, out hit, 1000f, groundMaskInt)) {
                if (rb.velocity.x > 0 && hit.transform.tag != "IgnoreIce") {
                    if (hit.transform != lastIcePlatform || firstIcePosition == null || ice == null) {
                        firstIcePosition = hit.point;
                        lastIcePlatform = hit.transform;
                        ice = Instantiate(icePrefab, Vector3.zero, hit.transform.rotation).transform;
                        icePlatforms.Add(ice);
                    }

                    lastIcePosition = hit.point;
                    ice.position = (firstIcePosition + lastIcePosition) / 2;
                    Vector3 size = ice.localScale;
                    size.x = (lastIcePosition - firstIcePosition).magnitude;
                    ice.localScale = size;
                } else {
                    lastIcePlatform = null;
                }
            } else {
                lastIcePlatform = null;
            }

            rb.AddForce(new Vector3(move.x, move.y, 0) * flyForce);
            if (rb.velocity.magnitude > maxFlySpeed) {
                rb.velocity = rb.velocity.normalized * maxFlySpeed;
            }
            yield return null;
        }

        lastIcePlatform = null;
        lastIcePosition = Vector3.zero;
        firstIcePosition = Vector3.zero;
        flying = false;

        yield return new WaitForSeconds(iceDuration);

        for (int i = icePlatforms.ToList().Count - 1; i >= 0; i--) {
            Destroy(icePlatforms[i].gameObject);
            icePlatforms.RemoveAt(i);
        }
    }

    private void Flip() {
        if (powerupScript.isCastingWindBlast == true)
            return; 

        if (isFacingRight && rb.velocity.x < 0 || !isFacingRight && rb.velocity.x > 0) {
            isFacingRight = !isFacingRight;
            characterVisualBody.transform.Rotate(Vector3.up, 180);
        }
    }

    private void OnEnable() {
        GameManager.onFreeze += OnFreeze;
        GameManager.onUnfreeze += OnUnfreeze;
        PlacementManagerScript.onRespawn += OnRespawn;

        rb = GetComponent<Rigidbody>();
        groundMaskInt = LayerMask.GetMask(groundMask);

        col = GetComponent<CapsuleCollider>();
        playerSpeed = maxSpeed;

        powerupScript = GetComponent<PowerupTestScript>();
        powerupScript.ApplyVariables(maxSpeed, character, gamepad);
    }

    private void OnDisable() {
        GameManager.onFreeze -= OnFreeze;
        GameManager.onUnfreeze -= OnUnfreeze;
        PlacementManagerScript.onRespawn -= OnRespawn;
    }
}   