using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpObjectScript : MonoBehaviour
{
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float distanceUpAndDown; 
    private Vector3 startPosition; 

    private void Start()
    {
        startPosition = transform.position; 
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.Rotate(Vector3.up, rotationSpeed); 

        float newY = distanceUpAndDown * Mathf.Sin(moveSpeed * Time.time);
        transform.position = new Vector3(transform.position.x, startPosition.y + newY, transform.position.z); 
    }
}
