using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PodiumScript : MonoBehaviour {
    [Serializable]
    private struct Character {
        public GameObject prefab;
        public PlayerControllerTestScript.Character character;
    }

    [SerializeField] private float cameraMoveSpeed;
    [SerializeField] private float cameraTurnSpeed;
    [SerializeField] private List<Character> characters;

    private Dictionary<PlayerControllerTestScript.Character, GameObject> characterPrefabs;
    private List<GameObject> players;
    private Transform cam;
    private bool instantiated;
    private bool done;

    private void Start() {
        UnityEngine.Rendering.DebugManager.instance.enableRuntimeUI = false;
        UnityEngine.Rendering.DebugManager.instance.displayRuntimeUI = false;
    }

    private void Update() {
        if (!done) return;

        for (int i = 0; i < Gamepad.current.allControls.Count; i++) {
            if (Gamepad.current.allControls[i].IsPressed()) {
                StartCoroutine(RemovePlayers());
            }
        }
    }

    private IEnumerator RemovePlayers() {
        SceneManager.LoadScene("StartScreen");

        while (!SceneManager.GetSceneByName("StartScreen").isLoaded) {
            yield return null;    
        }

        PlayerControllerTestScript[] plrs = FindObjectsOfType<PlayerControllerTestScript>();
        foreach (PlayerControllerTestScript plr in plrs) {
            Destroy(plr.gameObject);
        }

        Destroy(gameObject);
    }

    public void AddPlayer(PlayerControllerTestScript.Character character) {
        if (!instantiated) {
            players = new List<GameObject>();
            characterPrefabs = new Dictionary<PlayerControllerTestScript.Character, GameObject>();
            foreach (Character chr in characters) {
                characterPrefabs.Add(chr.character, chr.prefab);
            }
            instantiated = true;
        }

        players.Add(characterPrefabs[character]);
    }

    public void ShowPlayers() {
        StartCoroutine(ShowPlayersCoroutine());
    }

    private IEnumerator ShowPlayersCoroutine() {
        while (!SceneManager.GetSceneByName("PodiumScene").isLoaded) {
            yield return null;
        }

        cam = FindFirstObjectByType<Camera>().transform;

        for (int i = 0; i < players.Count; i++) {
            PlayerControllerTestScript script = Instantiate(players[i], GameObject.Find("p" + (i + 1).ToString()).transform.position, Quaternion.Euler(0, 45, 0)).GetComponent<PlayerControllerTestScript>();
            script.OnFreeze();
        }

        while (cam.rotation.x >= 0) {
            cam.Rotate(Vector3.right, -Time.deltaTime * cameraTurnSpeed);
            cam.position += Vector3.forward * cameraMoveSpeed;

            yield return null;
        }

        done = true;
    }
}