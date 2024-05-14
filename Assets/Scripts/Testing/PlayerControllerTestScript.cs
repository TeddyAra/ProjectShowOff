using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControllerTestScript : MonoBehaviour {
    [Tooltip("The force that should be applied to the player when they move")]
    [SerializeField] private float moveForce;

    [Tooltip("The force that should be applied to the player when they stop moving")]
    [SerializeField] private float moveDrag;

    [Tooltip("The force that should be applied to the player when they jump")]
    [SerializeField] private float jumpForce;

    [Tooltip("The extra force given every frame for holding the button")]
    [SerializeField] private float jumpBoost;

    [Tooltip("The maximum amount of time the player can hold the jump button to jump higher")]
    [SerializeField] private float jumpTime;

    [Tooltip("The maximum speed the player can reach")]
    [SerializeField] private float maxSpeed;

    [Tooltip("The amount of time in seconds the player is allowed to jump, despite not being grounded")]
    [SerializeField] private float coyoteTime;

    [Tooltip("Whether you're using the keyboard or not")]
    [SerializeField] private bool usingKeyboard;

    [Tooltip("The point where the ground should be checked")]
    [SerializeField] private Transform checkPoint;

    [Tooltip("The size of the ground check")]
    [SerializeField] private float groundCheckSize;

    [Tooltip("The name of the ground mask layer")]
    [SerializeField] private string groundMask;

    private Rigidbody rb;
    private Vector3 velocity;
    private bool grounded;
    private int groundMaskInt;
    private float coyoteTimer;
    private float jumpTimer;

    private void Start() {
        rb = GetComponent<Rigidbody>();
        groundMaskInt = LayerMask.GetMask(groundMask);
    }

    private void Update() {
        // Get the rigid body's velocity
        velocity = rb.velocity;

        // Check if the player is grounded or not
        if (Physics.CheckSphere(checkPoint.position, groundCheckSize, groundMaskInt)) {
            grounded = true;
        } else {
            if (grounded)
                coyoteTimer = coyoteTime;

            grounded = false;
        }

        // Update timers
        coyoteTimer -= Time.deltaTime;
        jumpTimer -= Time.deltaTime;

        if (usingKeyboard) {
            // Get input
            bool left = Input.GetKey(KeyCode.A);
            bool right = Input.GetKey(KeyCode.D);
            bool jump = Input.GetKeyDown(KeyCode.Space);
            bool holdingJump = Input.GetKey(KeyCode.Space);

            // Apply input
            if (grounded && (left || right)) {
                RaycastHit hit;
                if (Physics.Raycast(checkPoint.position, Vector3.down, out hit, groundCheckSize * 100, groundMaskInt)) {
                    Vector3 normal = hit.normal;
                    Vector3 acc = right ? new Vector3(normal.y, -normal.x, 0) : new Vector3(-normal.y, normal.x, 0);
                    Debug.Log(acc);

                    velocity += acc * moveForce;
                }
            }

            // If there's no input and we're still moving
            if (!left && !right && velocity.x != 0 && grounded) {
                // Check which direction the player is going
                if (velocity.x < 0) {
                    // Snap velocity to 0 if needed, otherwise add drag
                    if (velocity.x + moveDrag > 0) velocity.x = 0;
                    else velocity.x += moveDrag;
                } else if (velocity.x > 0) {
                    if (velocity.x - moveDrag < 0) velocity.x = 0;
                    else velocity.x -= moveDrag;
                }
            }

            // Make player jump
            if (jump && (grounded || coyoteTimer >= 0)) {
                jumpTimer = jumpTime;
                rb.AddForce(Vector3.up * jumpForce);
            }

            // Give the player a boost if they're holding the button
            if (holdingJump && jumpTimer > 0) {
                rb.AddForce(Vector3.up * jumpBoost);
            }
        }

        // Make sure players aren't going too fast
        velocity = Mathf.Clamp(velocity.magnitude, 0, maxSpeed) * velocity.normalized;

        // Apply the velocity
        rb.velocity = velocity;
    }
}