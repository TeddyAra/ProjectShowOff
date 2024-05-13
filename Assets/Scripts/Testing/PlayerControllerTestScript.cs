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

    [Tooltip("The maximum speed the player can reach")]
    [SerializeField] private float maxSpeed;

    [Tooltip("Whether you're using the keyboard or not")]
    [SerializeField] private bool usingKeyboard;

    private Rigidbody rb;
    private Vector3 velocity;

    private void Start() {
        rb = GetComponent<Rigidbody>();
    }

    private void Update() {
        // Get the rigid body's velocity
        velocity = rb.velocity;

        if (usingKeyboard) {
            // Get input
            bool left = Input.GetKey(KeyCode.A);
            bool right = Input.GetKey(KeyCode.D);
            bool jump = Input.GetKeyDown(KeyCode.Space);

            // Apply input
            velocity.x += right ? moveForce : (left ? -moveForce : 0);

            // If there's no input and we're still moving
            if (!left && !right && velocity.x != 0) {
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
            if (jump) rb.AddForce(Vector3.up * jumpForce);
        }

        // Make sure players aren't going too fast
        Vector3 movementVelocity = new Vector3(velocity.x, 0, 0);
        if (movementVelocity.magnitude > maxSpeed) movementVelocity = velocity.normalized * maxSpeed;
        velocity.x = movementVelocity.x;

        // Apply the velocity
        rb.velocity = velocity;
    }
}