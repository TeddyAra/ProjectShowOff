using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControllerTestScript : MonoBehaviour {
    [SerializeField] private float moveForce;
    [SerializeField] private float moveDrag;
    [SerializeField] private float jumpForce;
    [SerializeField] private float maxSpeed;
    [SerializeField] private bool usingKeyboard;

    private Rigidbody rb;
    private Vector3 velocity;

    private void Start() {
        rb = GetComponent<Rigidbody>();
    }

    private void Update() {
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

        rb.velocity = velocity;
    }
}