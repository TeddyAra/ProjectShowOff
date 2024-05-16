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

    [Tooltip("The extra distance given to the camera's position")]
    [SerializeField] private Vector3 displacement;

    [Tooltip("How long it should take for the camera to transition")]
    [SerializeField] private float moveTime;

    [Tooltip("The amount of distance that is too much for the camera to snap to")]
    [SerializeField] private float maxSnapDistance;

    private Transform firstPlayer;  // The player in first place
    private Transform lastPlayer;   // The player in last place
    private bool transitioning;
    private float moveTimer;
    private Vector3 oldPosition;

    private void Update() {
        if (!transitioning) {
            Vector3 averagePosition = GetAveragePosition();
            Vector3 displacement = averagePosition - transform.position;

            if (displacement.magnitude > maxSnapDistance &&
                displacement.x > 0) {
                transitioning = true;
                moveTimer = 0;
                oldPosition = transform.position;
            } else {
                transform.position = averagePosition;
            }
        } else {
            Vector3 averagePosition = GetAveragePosition() - oldPosition;
            moveTimer += Time.deltaTime;

            if (moveTimer >= moveTime) {
                transitioning = false;
            }

            transform.position = oldPosition + averagePosition * (moveTimer / moveTime);
        }
    }

    private Vector3 GetAveragePosition() {
        // Get the first and last player
        float minX = float.MaxValue;
        float maxX = float.MinValue;

        foreach (Transform player in players) {
            if (player.tag != "Player") continue;

            if (player.position.x < minX) {
                minX = player.position.x;
                lastPlayer = player;
            }

            if (player.position.x > maxX) {
                maxX = player.position.x;
                firstPlayer = player;
            }
        }

        // Get the average position
        Vector3 added = firstPlayer.position + lastPlayer.position;
        Vector3 averagePosition = added / 2;

        Vector3 difference = firstPlayer.position - lastPlayer.position;
        difference.y = 0;
        float distance = difference.magnitude;
        float delta = 0;

        // Get how far the camera should be from the players
        if (distance > minPlayerDistance) {
            delta = (distance - minPlayerDistance) / (maxPlayerDistance - minPlayerDistance);
            delta = Mathf.Clamp(delta, 0, 1);
        }

        if (distance >= maxPlayerDistance) {
            float fixedX = firstPlayer.position.x - maxPlayerDistance / 2;
            averagePosition.x = fixedX;
        }

        averagePosition.z = (minPlayerDistance + delta * (maxPlayerDistance - minPlayerDistance)) * -1;

        // Get the average y position
        averagePosition.y = 0;

        foreach (Transform player in players) {
            if (player.tag != "Player") continue;

            averagePosition.y += player.position.y;
        }

        averagePosition.y /= players.Count;

        // Apply the position
        averagePosition += displacement;

        return averagePosition;
    }
}