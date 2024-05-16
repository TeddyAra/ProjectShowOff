using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour {
    [SerializeField] private Transform checkpoint;
    [SerializeField] private float spawnDistance;

    private List<Vector3> checkpoints;
    private int currentCheckpoint;
    private int spawnNum;

    // When all dead players are allowed to move again
    public delegate void OnUnfreeze();
    public static event OnUnfreeze onUnfreeze;

    private void Start() {
        // Get all checkpoints
        GameObject[] list = GameObject.FindGameObjectsWithTag("CheckpointPosition");
        checkpoints = new List<Vector3>();

        foreach (GameObject checkpoint in list)
            checkpoints.Add(checkpoint.transform.position);

        // Order them by their x position
        checkpoints = checkpoints.OrderBy(x => x.x).ToList();
    }

    private void OnCheckpoint() {
        // Ignore this if it's the last checkpoint
        if (currentCheckpoint == checkpoints.Count) return;

        // Go to the next checkpoint
        checkpoint.position = checkpoints[currentCheckpoint];
        currentCheckpoint++;
        spawnNum = 0;

        // Unfreeze players if needed
        onUnfreeze?.Invoke();
        Debug.Log("Checkpoint reached");
    }

    private Vector3 GetCheckpoint() {
        spawnNum++;
        return checkpoint.position + Vector3.left * spawnNum * spawnDistance;
    }

    private void OnEnable() {
        PlayerControllerTestScript.onCheckpoint += OnCheckpoint;
        PlayerControllerTestScript.getCheckpoint += GetCheckpoint;
    }

    private void OnDisable() {
        PlayerControllerTestScript.onCheckpoint -= OnCheckpoint;
        PlayerControllerTestScript.getCheckpoint -= GetCheckpoint;
    }
}