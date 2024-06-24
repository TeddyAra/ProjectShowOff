using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class GameManager : MonoBehaviour {
    [SerializeField] private Transform checkpoint;
    [SerializeField] private float spawnDistance;
    [SerializeField] private TMP_Text countdown;

    private List<Vector3> checkpoints;
    private int currentCheckpoint;
    private int spawnNum;
    private int playerNum;
    private int playerCount;

    public delegate void OnFreeze();
    public static event OnFreeze onFreeze;

    public delegate void OnUnfreeze();
    public static event OnUnfreeze onUnfreeze;

    public delegate void OnStart();
    public static event OnStart onStart;

    public delegate void OnShowUI();
    public static event OnShowUI onShowUI;

    SFXManager sfxManager; 

    private void Start() {
        // Get all checkpoints
        GameObject[] list = GameObject.FindGameObjectsWithTag("CheckpointPosition");
        checkpoints = new List<Vector3>();

        foreach (GameObject checkpoint in list)
            checkpoints.Add(checkpoint.transform.position);

        // Order them by their x position
        checkpoints = checkpoints.OrderBy(x => x.x).ToList();
        Vector3 last = checkpoints.First();
        checkpoints.RemoveAt(0);
        checkpoints.Add(last);

        StartCoroutine(GetPlayerNum());
        StartCoroutine(GetSFXManager()); 
        StartCoroutine(Countdown());

        checkpoint.position = checkpoints[0];

        UnityEngine.Rendering.DebugManager.instance.enableRuntimeUI = false;
        UnityEngine.Rendering.DebugManager.instance.displayRuntimeUI = false;
    }

    private IEnumerator GetPlayerNum() {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        while (players.Length == 0) {
            players = GameObject.FindGameObjectsWithTag("Player");
            yield return new WaitForSeconds(1.0f);
        }
        playerNum = players.Length;
    }

    IEnumerator GetSFXManager()
    {
        sfxManager = FindObjectOfType<SFXManager>();  
        while (sfxManager == null)
        {
            sfxManager = FindObjectOfType<SFXManager>();
            yield return new WaitForSeconds(1.0f);
        }
    }
    IEnumerator Countdown() {
        onFreeze?.Invoke();

        float growSpeed = 0.25f;

        string[] messages = { 
            "Get ready!",
            "3",
            "2",
            "1",
            "Go!"
        };

        float[] delays = {
            3.0f,
            1.0f,
            1.0f,
            1.0f,
            2.0f
        };

        for (int i = 0; i < messages.Length; i++) {
            if (i == messages.Length - 1) {
                onStart?.Invoke();
                onUnfreeze?.Invoke();
            }

            countdown.text = messages[i];
            float timer = 0;
            while (timer <= growSpeed) {
                timer += Time.deltaTime;
                countdown.fontSize = Mathf.Sin(timer * (0.5f / growSpeed) * Mathf.PI) * (60 + i * 10);
                yield return null;
            }

            yield return new WaitForSeconds(delays[i] - growSpeed * 2);

            timer = 0;
            while (timer <= growSpeed) {
                timer += Time.deltaTime;
                countdown.fontSize = Mathf.Sin((timer * (0.5f / growSpeed) + 0.5f) * Mathf.PI) * (60 + i * 10);
                yield return null;
            }
        }

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
        if (playerCount == 0) {
            CameraTestScript camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraTestScript>();
            camera.finished = true;
        }

        playerCount++;
        if (playerCount >= playerNum - spawnNum) StartCoroutine(Finish());
    }

    IEnumerator Finish() {
        // Freeze players and show someone has finished
        //onFreeze?.Invoke();
        countdown.text = "Finish!"; 
        sfxManager.Play("VictorySound"); 
        float timer = 0;
        while (timer <= 0.25f) {
            timer += Time.deltaTime;
            countdown.fontSize = Mathf.Sin(timer * 2 * Mathf.PI) * 60;
            yield return null;
        }

        yield return new WaitForSeconds(1.75f);

        onShowUI?.Invoke();
        countdown.text = "";
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