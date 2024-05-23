using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
    [SerializeField] private Transform checkpoint;
    [SerializeField] private float spawnDistance;
    [SerializeField] private TMP_Text countdown;

    private List<Vector3> checkpoints;
    private int currentCheckpoint;
    private int spawnNum;

    public delegate void OnFreeze();
    public static event OnFreeze onFreeze;

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

        StartCoroutine(Countdown());

        checkpoint.position = checkpoints[0];
    }

    IEnumerator Countdown() {
        onFreeze?.Invoke();

        countdown.text = "3";
        yield return new WaitForSeconds(1);

        countdown.text = "2";
        yield return new WaitForSeconds(1);

        countdown.text = "1";
        yield return new WaitForSeconds(1);

        countdown.text = "Go!";
        onUnfreeze?.Invoke();
        yield return new WaitForSeconds(1);

        countdown.text = "";
    }

    private void OnCheckpoint() {
        // Ignore this if it's the last checkpoint
        if (currentCheckpoint >= checkpoints.Count) return;

        // Go to the next checkpoint
        currentCheckpoint++;
        checkpoint.position = checkpoints[currentCheckpoint];
        spawnNum = 0;

        // Unfreeze players if needed
        onUnfreeze?.Invoke();
    }

    private Vector3 GetCheckpoint() {
        spawnNum++;
        return checkpoint.position + Vector3.left * spawnNum * spawnDistance;
    }

    private void OnFinish() {
        StartCoroutine(Finish());
    }

    IEnumerator Finish() {
        // Freeze players and show someone has finished
        onFreeze?.Invoke();
        countdown.text = "Finish!";
        yield return new WaitForSeconds(3);

        // Restart the race
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnEnable() {
        PlayerControllerTestScript.onCheckpoint += OnCheckpoint;
        PlayerControllerTestScript.getCheckpoint += GetCheckpoint;
        PlayerControllerTestScript.onFinish += OnFinish;
    }

    private void OnDisable() {
        PlayerControllerTestScript.onCheckpoint -= OnCheckpoint;
        PlayerControllerTestScript.getCheckpoint -= GetCheckpoint;
        PlayerControllerTestScript.onFinish -= OnFinish;
    }
}