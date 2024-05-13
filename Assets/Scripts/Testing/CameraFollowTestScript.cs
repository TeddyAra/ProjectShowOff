using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CameraFollowTestScript : MonoBehaviour {
    [Tooltip("The player this camera should follow")]
    [SerializeField] private Transform player;

    [Tooltip("The offset the camera should have relative to the player")]
    [SerializeField] private Vector3 offset;

    [Tooltip("Whether the camera should rotate to look at the player")]
    [SerializeField] private bool lookAt;

    [Tooltip("The y position the camera should have if lookAt is true")]
    [SerializeField] private float yPosition;

    private void Update() { 
        Vector3 position = player.position + offset;
        if (lookAt) position.y = yPosition;
        transform.position = position;

        if (lookAt) transform.LookAt(player.position);
    }
}