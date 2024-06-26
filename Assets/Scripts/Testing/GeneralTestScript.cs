using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GeneralTestScript : MonoBehaviour {
    private Gamepad gamepad;
    [SerializeField] private bool resetWithController;
    [SerializeField] private Transform finish;

    private void Update() {
        if (Gamepad.all.Count != 0) gamepad = Gamepad.current;

        if (Input.GetKeyDown(KeyCode.R)) {
            Respawn();
        }

        if (Input.GetKeyDown(KeyCode.M)) {
            PlayerControllerTestScript[] players = FindObjectsOfType<PlayerControllerTestScript>();
            GameObject camera = FindObjectOfType<Camera>().gameObject;
            GameObject sfxManager = FindObjectOfType<SFXManager>().gameObject;

            Destroy(camera);
            Destroy(sfxManager);

            SceneManager.LoadScene("StartScreen");

            foreach (PlayerControllerTestScript script in players) {
                Destroy(script.gameObject);
            }

            GameObject placementManager = GameObject.FindObjectOfType<PlacementManagerScript>().gameObject;
            Destroy(placementManager);
        }

        if (!resetWithController) return;

        if (gamepad != null && gamepad.buttonNorth.wasPressedThisFrame) {
            Respawn();
        }
    }

    private void Respawn() { 
        Vector3 position = GameObject.FindGameObjectWithTag("Player").transform.position;
        finish.position = position + Vector3.right * 5.0f;
    }
}