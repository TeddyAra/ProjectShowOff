using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class StartManager : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip startSound;

    [SerializeField] Animator sceneTransition; 

    private bool canLoadScene = true; 

    void Update()
    {
        for (int i = 0; i < Gamepad.current.allControls.Count; i++) {
            if (Gamepad.current.allControls[i].IsPressed() && canLoadScene == true) {
                canLoadScene = false;
                StartCoroutine(LoadLevel()); 
            }
        }
    }

    IEnumerator LoadLevel()
    {
        audioSource.PlayOneShot(startSound);
        sceneTransition.SetTrigger("Start"); 
        yield return new WaitForSeconds(2);
        SceneManager.LoadScene("CharacterSelectorScene");
        canLoadScene = true; 
    }
}
