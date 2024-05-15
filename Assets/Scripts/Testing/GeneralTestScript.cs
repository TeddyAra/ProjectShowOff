using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GeneralTestScript : MonoBehaviour {
    private Gamepad gamepad;

    private void Update() {
        gamepad = Gamepad.all[0];

        if (Input.GetKeyDown(KeyCode.R)) 
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        if (gamepad != null && gamepad.buttonNorth.wasPressedThisFrame)
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);

    }
}