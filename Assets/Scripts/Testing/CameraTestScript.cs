using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTestScript : MonoBehaviour {
    [Tooltip("All players in the game")]
    [SerializeField] private List<Transform> players;

    [Tooltip("The distance between the first and last player when the camera should start zooming out")]
    [SerializeField] private float minPlayerDistance;

    [Tooltip("The maximum distance between the first and last player")]
    [SerializeField] private float maxPlayerDistance;

    [Tooltip("The minumum z-position the camera should have")]
    [SerializeField] private float minCamDistance;

    [Tooltip("The maximum z-position the camera should have")]
    [SerializeField] private float maxCamDistance;

    [Tooltip("The y-position of the camera")]
    [SerializeField] private float camY;

    private Transform firstPlayer;  // The player in first place
    private Transform lastPlayer;   // The player in last place

    private void Update() {
        // Get the first and last player
        float minX = float.MaxValue;
        float maxX = float.MinValue;

        foreach (Transform player in players) {
            if (player.position.x < minX) {
                minX = player.position.x;
                lastPlayer = player;
            }

            if (player.position.x > maxX) { 
                maxX = player.position.x;
                firstPlayer = player;
            }
        }

        Vector3 added = firstPlayer.position + lastPlayer.position;
        Vector3 averagePosition = added / 2;

        Vector3 difference = firstPlayer.position - lastPlayer.position;
        difference.y = 0;
        float distance = difference.magnitude;
        float delta = 0;

        if (distance > minPlayerDistance) {
            delta = (distance - minPlayerDistance) / (maxPlayerDistance - minPlayerDistance);
            delta = Mathf.Clamp(delta, 0, 1);
        }

        if (distance >= maxPlayerDistance) {
            float fixedX = firstPlayer.position.x - maxPlayerDistance / 2;
            averagePosition.x = fixedX; 
        } 

        averagePosition.z = (minPlayerDistance + delta * (maxPlayerDistance - minPlayerDistance)) * -1;
        averagePosition.y = (firstPlayer.position.y + lastPlayer.position.y) / 2;
        transform.position = averagePosition;
    }
}