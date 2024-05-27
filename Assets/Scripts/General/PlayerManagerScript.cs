using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PlayerManagerScript : MonoBehaviour {
    [System.Serializable]
    struct CharacterPicker {
        [HideInInspector] public bool isPlaying;
        [SerializeField] private GameObject playing;
        [SerializeField] private GameObject notPlaying;

        private Gamepad gamepad;

        public void SetGamepad(Gamepad gamepad) { 
            this.gamepad = gamepad;
        }

        public Gamepad GetGamepad() {
            return gamepad;
        }

        public void Switch() { 
            isPlaying = !isPlaying;

            playing.SetActive(isPlaying);
            notPlaying.SetActive(!isPlaying);
        }
    }

    [SerializeField] private List<CharacterPicker> characterPickers;

    private void Update() {
        foreach (Gamepad gamepad in Gamepad.all) {
            if (gamepad.buttonSouth.wasPressedThisFrame) {
                bool picked = false;
                foreach (CharacterPicker picker in characterPickers) {
                    if (!picker.isPlaying && !picked) {
                        picker.Switch();
                        picker.SetGamepad(gamepad);
                        picked = true;
                    }
                }
            }
        }
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change) {
        if (change == InputDeviceChange.Removed) {
            foreach (CharacterPicker picker in characterPickers) {
                if (picker.GetGamepad() == device) {
                    picker.Switch();
                    picker.SetGamepad(null);
                    break;
                }
            }
        }
    }

    private void OnEnable() {
        InputSystem.onDeviceChange += OnDeviceChange;
    }

    private void OnDisable() {
        InputSystem.onDeviceChange -= OnDeviceChange;
    }
}