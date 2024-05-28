using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Unity.VisualScripting;
using System.Linq;
using System;

public class PlayerManagerScript : MonoBehaviour {
    [Serializable]
    struct CharacterPicker {
        [HideInInspector] public bool isPlaying;                // Whether the character picker is being used
        [SerializeField] private GameObject playing;            // The UI for if someone is using the character picker
        [SerializeField] private GameObject notPlaying;         // The UI for if someone is not using the character picker
        [SerializeField] private GameObject ready;              // The UI for if someone is ready to play
        [SerializeField] private Image character;               // The UI for the character

        private int index;                                      // The index of the controller
        private int currentCharacter;                           // The index of the character
        private Dictionary<Material, Vector2> characterSizes;   // The size of each character

        // Apply the dictionary and instantiate the character
        public void ApplyCharacterSizes(Dictionary<Material, Vector2> characterSizes) { 
            this.characterSizes = characterSizes;
        }

        // Change the character by either moving back or forth in the list
        public void ChangeCharacter(bool right, List<int> taken) {
            do {
                currentCharacter += right ? 1 : -1;

                // If it has reached the beginning or end of the list, bring it over to the other side
                if (currentCharacter > 4) {
                    currentCharacter = 0;
                } else if (currentCharacter < 0) {
                    currentCharacter = 4;
                }
            } while (taken.Contains(currentCharacter));

            // Apply the image material and size
            character.material = characterSizes.ElementAt(currentCharacter).Key;
            character.rectTransform.sizeDelta = characterSizes.ElementAt(currentCharacter).Value;
        }

        public int GetCharacter() { 
            return currentCharacter;
        }

        // Set the controller index
        public void SetIndex(int index) {
            this.index = index;
        }

        // Get the controller index
        public int GetIndex() {
            return index;
        }

        // Switch the character picker UI
        public void Play() {
            isPlaying = true;

            playing.SetActive(true);
            notPlaying.SetActive(false);
            ready.SetActive(false);
        }

        public void Ready() {
            isPlaying = true;

            playing.SetActive(false);
            notPlaying.SetActive(false);
            ready.SetActive(true);
        }

        public void NotPlay() {
            isPlaying = false;

            playing.SetActive(false);
            notPlaying.SetActive(true);
            ready.SetActive(false);
        }
    }

    [SerializeField] private List<CharacterPicker> characterPickers;

    [Serializable]
    struct CharacterSize {
        public Material character;
        public Vector2 size;
    }

    [SerializeField] private List<CharacterSize> characterSizes;
    [SerializeField] private RectTransform startBar;
    [SerializeField] private float waitTime;
    [SerializeField] private float barWidth;

    private Dictionary<Gamepad, bool> gamepads;
    private Dictionary<Gamepad, bool> ready;
    private List<int> taken;
    private Gamepad firstPlayer;
    private float waitTimer;

    private void Start() {
        Dictionary<Material, Vector2> dict = new Dictionary<Material, Vector2>();
        foreach (CharacterSize characterSize in characterSizes) {
            dict.Add(characterSize.character, characterSize.size);
        }

        for (int i = 0; i < characterPickers.Count; i++) {
            CharacterPicker picker = characterPickers[i];
            picker.SetIndex(-1);
            picker.ApplyCharacterSizes(dict);
            characterPickers[i] = picker;
        }

        gamepads = new Dictionary<Gamepad, bool>();
        ready = new Dictionary<Gamepad, bool>();
        foreach (Gamepad gamepad in Gamepad.all) { 
            gamepads.Add(gamepad, false);
        }

        taken = new List<int>();
    }

    private void Update() {
        if (firstPlayer != null) {
            if (firstPlayer.buttonSouth.wasReleasedThisFrame) {
                waitTimer = 0;
                startBar.sizeDelta = new Vector2(0, startBar.sizeDelta.y);
            }

            bool ignore = false;
            foreach (bool state in ready.Values) {
                if (!state) {
                    ignore = true;
                    startBar.sizeDelta = new Vector2(0, startBar.sizeDelta.y);
                    break;
                }
            }

            if (firstPlayer.buttonSouth.isPressed && !ignore) {
                waitTimer += Time.deltaTime;
                float width = Mathf.Clamp(barWidth * (waitTimer / waitTime), 0, barWidth);
                startBar.sizeDelta = new Vector2(width, startBar.sizeDelta.y);

                if (waitTimer >= waitTime) {
                    Debug.Log("Ready!");
                }
            } 
        }

        for (int i = 0; i < gamepads.Count; i++) {
            KeyValuePair<Gamepad, bool> gamepad = gamepads.ElementAt(i);

            // If the controller is part of the game
            if (gamepad.Value) {
                // Update the first player 
                if (firstPlayer == null) {
                    firstPlayer = gamepad.Key;
                }

                // If the player wants to switch characters
                if (gamepad.Key.dpad.left.wasPressedThisFrame || gamepad.Key.dpad.right.wasPressedThisFrame) {
                    bool right = gamepad.Key.dpad.right.wasPressedThisFrame;

                    for (int j = 0; j < characterPickers.Count; j++) {
                        if (characterPickers[j].GetIndex() == i) {
                            CharacterPicker picker = characterPickers[j];
                            picker.ChangeCharacter(right, taken);
                            characterPickers[j] = picker;
                        }
                    }
                }

                // If the player wants to confirm their character
                if (gamepad.Key.buttonSouth.wasPressedThisFrame) {
                    for (int j = 0; j < characterPickers.Count; j++) {
                        if (characterPickers[j].GetIndex() == i) {
                            CharacterPicker picker = characterPickers[j];
                            taken.Add(picker.GetCharacter());
                            Debug.Log(picker.GetCharacter());
                            picker.Ready();

                            characterPickers[j] = picker;
                            ready[gamepad.Key] = true;
                        }
                    }
                }
            // If the controller is not part of the game
            } else {
                bool picked = false;
                if (gamepad.Key.buttonSouth.wasPressedThisFrame) {
                    for (int j = 0; j < characterPickers.Count; j++) {
                        CharacterPicker picker = characterPickers[j];
                        if (!picker.isPlaying && !picked) {
                            picker.Play();
                            picker.SetIndex(i);
                            picked = true;
                            picker.ChangeCharacter(true, taken);

                            gamepads[gamepad.Key] = true;
                            ready.Add(gamepad.Key, false);
                        }

                        characterPickers[j] = picker;
                    }
                }
            }
        }
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change) {
        if (change == InputDeviceChange.Added) {
            if (!gamepads.ContainsKey((Gamepad)device)) gamepads.Add((Gamepad)device, false);
        }

        if (change == InputDeviceChange.Removed) {
            int index = -2;

            for (int i = 0; i < gamepads.Count; ++i) {
                if (gamepads.ElementAt(i).Key == (Gamepad)device) {
                    index = i;
                    break;
                }
            }

            for (int i = 0; i < characterPickers.Count; i++) {
                CharacterPicker picker = characterPickers[i];
                if (picker.GetIndex() == index) {
                    if (taken.Contains(picker.GetCharacter())) taken.Remove(picker.GetCharacter());
                    picker.NotPlay();
                    picker.SetIndex(-1);
                }
                characterPickers[i] = picker;
            }

            if (gamepads.ContainsKey((Gamepad)device)) gamepads[(Gamepad)device] = false;
            if (ready.ContainsKey((Gamepad)device)) ready.Remove((Gamepad)device);
        }
    }

    private void OnEnable() {
        InputSystem.onDeviceChange += OnDeviceChange;
    }

    private void OnDisable() {
        InputSystem.onDeviceChange -= OnDeviceChange;
    }
}