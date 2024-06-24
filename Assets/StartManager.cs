using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class StartManager : MonoBehaviour {
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip startSound;

    [SerializeField] Animator sceneTransition; 

    private bool canLoadScene = true;

    private void Start() {
        UnityEngine.Rendering.DebugManager.instance.enableRuntimeUI = false;
        UnityEngine.Rendering.DebugManager.instance.displayRuntimeUI = false;
    }

    void Update() {
        for (int i = 0; i < Gamepad.all.Count; i++) {
            for (int j = 0; j < Gamepad.all[i].allControls.Count; j++) {
                if (Gamepad.all[i].allControls[j].IsPressed() && canLoadScene == true) {
                    canLoadScene = false;
                    StartCoroutine(LoadLevel());
                }
            }
        }
    }

    IEnumerator LoadLevel() {
        audioSource.PlayOneShot(startSound);
        sceneTransition.SetTrigger("Start"); 
        yield return new WaitForSeconds(2);
        SceneManager.LoadScene("CharacterSelectorScene");
        canLoadScene = true; 
    }
}
