using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Unity.VisualScripting;
using System.Linq;
using System;
using UnityEngine.SceneManagement;

public class PlayerManagerScript : MonoBehaviour {
    [Serializable]
    struct CharacterPicker {
        [HideInInspector] public bool isPlaying;                // Whether the character picker is being used
        [HideInInspector] public bool isReady;                // Whether the character picker is ready
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
            isReady = false;

            playing.SetActive(true);
            notPlaying.SetActive(false);
            ready.SetActive(false);
        }

        public void Ready() {
            isPlaying = true;
            isReady = true;

            playing.SetActive(false);
            notPlaying.SetActive(false);
            ready.SetActive(true);
        }

        public void NotPlay() {
            isPlaying = false;
            isReady = false;

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
        public GameObject prefab;
    }

    [SerializeField] private List<CharacterSize> characterSizes;
    [SerializeField] private RectTransform startBar;
    [SerializeField] private float waitTime;
    [SerializeField] private float barWidth;
    [SerializeField] private string gameSceneName;
    [SerializeField] private bool oneController;
    [SerializeField] private GameObject cameraPrefab;

    private Dictionary<Gamepad, bool> gamepads;
    private List<bool> lastJoysticks;
    private List<int> taken;
    private Gamepad firstPlayer;
    private float waitTimer;
    private bool done;

    public delegate void OnGetPlayers(List<Transform> players);
    public static event OnGetPlayers onGetPlayers;

    private void Start() {
        DontDestroyOnLoad(gameObject);

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
        lastJoysticks = new List<bool>();
        foreach (Gamepad gamepad in Gamepad.all) { 
            gamepads.Add(gamepad, false);
            lastJoysticks.Add(false);
        }

        taken = new List<int>();
    }

    private void Update() {
        // If the game is ready to start
        if (done) {
            // Wait for the game scene to load
            if (!SceneManager.GetSceneByName(gameSceneName).isLoaded) return;

            Vector3 position = GameObject.FindGameObjectWithTag("SpawnPoint").transform.position;
            List<Transform> players = new List<Transform>();

            Instantiate(cameraPrefab, position + Vector3.back * 14 + Vector3.up * 6, Quaternion.Euler(7, 0, 0));

            int num = 0;
            for (int i = 0; i < gamepads.Count; i++) {
                KeyValuePair<Gamepad, bool> gamepad = gamepads.ElementAt(i);
                if (!gamepad.Value) return;

                // If the controller has a character selected
                int index = GetCharacterPicker(i);
                if (index != -1) {
                    // Spawn the character
                    CharacterPicker picker = characterPickers[index];
                    GameObject character = characterSizes[picker.GetCharacter()].prefab;
                    PlayerControllerTestScript script = Instantiate(character, Vector3.zero, Quaternion.identity).GetComponent<PlayerControllerTestScript>();
                    players.Add(script.transform);

                    script.ChangeGamepad(gamepad.Key, num);
                    script.OnRespawn();
                    script.OnFreeze();
                    num++;

                    if (oneController) { 
                        script = Instantiate(character, Vector3.zero, Quaternion.identity).GetComponent<PlayerControllerTestScript>();
                        players.Add(script.transform);

                        script.ChangeGamepad(gamepad.Key, num);
                        script.OnRespawn();
                        script.OnFreeze();
                        num++;
                    }
                }
            }

            onGetPlayers?.Invoke(players);

            Destroy(gameObject);
            return;
        }

        // If there is a first player
        if (firstPlayer != null) {
            // Reset the start bar if the button is let go
            if (firstPlayer.buttonSouth.wasReleasedThisFrame) {
                waitTimer = 0;
                startBar.sizeDelta = new Vector2(0, startBar.sizeDelta.y);
            }

            // If the first player wants to start the game
            if (firstPlayer.buttonSouth.isPressed) {
                waitTimer += Time.deltaTime;
                float width = Mathf.Clamp(barWidth * (waitTimer / waitTime), 0, barWidth);
                startBar.sizeDelta = new Vector2(width, startBar.sizeDelta.y);

                // If the game has to start
                if (waitTimer >= waitTime) {
                    // Don't start the game if there is not at least two players ready
                    if (taken.Count < 2) {
                        waitTimer = 0;
                        return;
                    }

                    SceneManager.LoadScene(gameSceneName);
                    done = true;
                    return;
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
                Vector2 joystick = gamepad.Key.leftStick.ReadValue();
                bool left = gamepad.Key.dpad.left.wasPressedThisFrame || (joystick.x < -0.5f && !lastJoysticks[i]);
                bool right = gamepad.Key.dpad.right.wasPressedThisFrame || (joystick.x > 0.5f && !lastJoysticks[i]);

                if (left != right) {
                    lastJoysticks[i] = true;
                    int index = GetCharacterPicker(i);
                    if (index != -1) {
                        CharacterPicker picker = characterPickers[index];
                        picker.ChangeCharacter(right, taken);
                        characterPickers[index] = picker;
                    }
                }

                // Let the script check the joystick again if the player put it back to the center
                if (lastJoysticks[i] && joystick.x > -0.5f && joystick.x < 0.5f) {
                    lastJoysticks[i] = false;
                }

                // If the player wants to confirm their character
                if (gamepad.Key.buttonSouth.wasPressedThisFrame) {
                    int index = GetCharacterPicker(i);
                    if (index != -1) {
                        CharacterPicker picker = characterPickers[index];
                        if (picker.isReady) return;

                        taken.Add(picker.GetCharacter());
                        if (oneController) taken.Add(picker.GetCharacter());
                        picker.Ready();

                        characterPickers[index] = picker;

                        for (int k = 0; k < characterPickers.Count; k++) {
                            if (k == index) continue;

                            CharacterPicker pickerCopy = characterPickers[k];
                            if (pickerCopy.GetCharacter() == picker.GetCharacter())
                                pickerCopy.ChangeCharacter(true, taken);

                            characterPickers[k] = pickerCopy;
                        }
                    }
                }

                // If the player wants to go back to character selection
                if (gamepad.Key.buttonEast.wasPressedThisFrame) {
                    int index = GetCharacterPicker(i);
                    if (index != -1) {
                        CharacterPicker picker = characterPickers[index];
                        picker.Play();
                        taken.Remove(picker.GetCharacter());
                        characterPickers[index] = picker;
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
                        }

                        characterPickers[j] = picker;
                    }
                }
            }
        }
    }

    private int GetCharacterPicker(int index) {
        for (int j = 0; j < characterPickers.Count; j++) {
            if (characterPickers[j].GetIndex() == index) {
                return j;
            }
        }

        return -1;
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change) {
        if (change == InputDeviceChange.Added) {
            // Add the gamepad if it wasn't here before
            if (!gamepads.ContainsKey((Gamepad)device)) {
                gamepads.Add((Gamepad)device, false);
                lastJoysticks.Add(false);
            }
        }

        if (change == InputDeviceChange.Removed) {
            // Check if the controller was a part of the game (assigned to one of the character pickers)
            // -2 because -1 is the 'state' of characterpickers if they have no controller assigned to them
            int index = -2;

            for (int i = 0; i < gamepads.Count; ++i) {
                if (gamepads.ElementAt(i).Key == (Gamepad)device) {
                    index = i;
                    break;
                }
            }

            for (int i = 0; i < characterPickers.Count; i++) {
                CharacterPicker picker = characterPickers[i];

                // If the picker has this controller assigned to them
                if (picker.GetIndex() == index) {
                    // If the character had a confirmed character, make sure other players can choose that player again
                    if (taken.Contains(picker.GetCharacter())) taken.Remove(picker.GetCharacter());

                    picker.NotPlay();
                    picker.SetIndex(-1);
                }
                characterPickers[i] = picker;
            }

            // Remove the controller if it was part of gamepads
            if (gamepads.ContainsKey((Gamepad)device)) gamepads[(Gamepad)device] = false;

            // Find another first player if this was the first player
            if (firstPlayer == (Gamepad)device) firstPlayer = null;
        }
    }

    private void OnEnable() {
        InputSystem.onDeviceChange += OnDeviceChange;
    }

    private void OnDisable() {
        InputSystem.onDeviceChange -= OnDeviceChange;
    }
}