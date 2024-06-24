using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DanceScript : MonoBehaviour {
    [SerializeField] private float jumpSpeed;
    [SerializeField] private float jumpHeight;
    [SerializeField] private float rotateStrength;

    private float originalHeight;
    private float originalAngle;

    private void Start() {
        originalHeight = transform.position.y;
        originalAngle = transform.rotation.eulerAngles.y;
    }

    private void Update() {
        float value = Mathf.Sin(Time.time * Mathf.PI * jumpSpeed);

        Vector3 euler = transform.eulerAngles;
        euler.y = originalAngle + value * rotateStrength;
        transform.rotation = Quaternion.Euler(euler);

        if (value < 0) value *= -1;
        Vector3 position = transform.position;
        position.y = originalHeight + value * jumpHeight;
        transform.position = position;
    }
}