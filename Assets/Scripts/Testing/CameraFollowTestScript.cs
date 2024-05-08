using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CameraFollowTestScript : MonoBehaviour {
    [SerializeField] private Transform player;
    [SerializeField] private Vector3 offset;
    [SerializeField] private bool lookAt;
    [SerializeField] private float yPosition;

    private void Update() { 
        Vector3 position = player.position + offset;
        if (lookAt) position.y = yPosition;
        transform.position = position;

        if (lookAt) transform.LookAt(player.position);
    }
}