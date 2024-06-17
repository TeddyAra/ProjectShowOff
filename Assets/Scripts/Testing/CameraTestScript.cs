using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CameraTestScript : MonoBehaviour {
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

    private Transform firstPlayer;
    private Transform lastPlayer;
    private bool transitioning;
    private float moveTimer;
    private Vector3 oldPosition;
    private Vector3 offset;

    private List<Transform> players;
    private bool starting = true;

    private void Start () {
        DontDestroyOnLoad(gameObject);
    }

    private void Update() {
        if (starting) return;

        if (!transitioning) {
            Vector3 averagePosition = GetAveragePosition();
            Vector3 displacement = averagePosition - transform.position;

            if (displacement.magnitude > maxSnapDistance) {
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

    public void SetOffset(Vector3 offset) {
        this.offset = offset;
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

        averagePosition.z = (minCamDistance + delta * (maxCamDistance - minCamDistance)) * -1;

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

    private void OnStart() {
        starting = false;
    }

    private void StopRace() {
        starting = true;
    }

    private void OnGetPlayers(List<Transform> players) { 
        this.players = players;
    }

    private void OnRespawn(List<PlayerControllerTestScript> positions) {
        transform.position = GameObject.FindGameObjectWithTag("SpawnPoint").transform.position + offset;
    }

    private void OnEnable() {
        GameManager.onStart += OnStart;
        PlayerManagerScript.onGetPlayers += OnGetPlayers;
        PlacementManagerScript.onRespawn += OnRespawn;
        GameManager.onFinish += StopRace;
        PlacementManagerScript.onRespawn += OnRespawn;
    }

    private void OnDisable() {
        GameManager.onStart -= OnStart;
        PlayerManagerScript.onGetPlayers -= OnGetPlayers;
        PlacementManagerScript.onRespawn -= OnRespawn;
        GameManager.onFinish -= StopRace;
        PlacementManagerScript.onRespawn -= OnRespawn;
    }
}