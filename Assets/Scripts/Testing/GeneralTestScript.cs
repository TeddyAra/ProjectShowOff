using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GeneralTestScript : MonoBehaviour {
    private Gamepad gamepad;
    [SerializeField] private bool resetWithController;

    private void Update() {
        if (Gamepad.all.Count != 0) gamepad = Gamepad.current;

        if (Input.GetKeyDown(KeyCode.R)) 
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        if (!resetWithController) return;

        if (gamepad != null && gamepad.buttonNorth.wasPressedThisFrame)
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);

    }
}