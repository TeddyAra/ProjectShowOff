using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;

public class GamepadManagerTestScript : MonoBehaviour {
    struct GamepadPlayer {
        InputDeviceDescription desc;        // Identifier
        Gamepad gamepad;                    // Controller
        PlayerControllerTestScript script;  // Player
        int playerNum;                      // Player index

        public GamepadPlayer(InputDeviceDescription desc, Gamepad gamepad, PlayerControllerTestScript script, int playerNum) {
            this.desc = desc;
            this.gamepad = gamepad;
            this.script = script;
            this.playerNum = playerNum;
        }

        // Apply the controller to the player
        public void EnablePlayer() {
            script.ChangeGamepad(gamepad);
        }

        // Remove the controller from the player
        public void DisablePlayer() {
            script.ChangeGamepad(null);
        }

        // Get the controller's identifier
        public InputDeviceDescription GetDescription() {
            return desc;
        }

        // Set the controller
        public void SetGamepad(Gamepad gamepad) {
            this.gamepad = gamepad;
        }

        // Get the player index
        public int GetPlayerNum() { 
            return playerNum;
        }
    }

    private List<GamepadPlayer> gamepads;
    private List<PlayerControllerTestScript> players;
    private List<int> missingPlayers;

    private void Start() {
        gamepads = new List<GamepadPlayer>();
        players = FindObjectsOfType<PlayerControllerTestScript>().ToList();
        missingPlayers = new List<int>();

        // Add all players to missing players
        for (int num = 0; num < players.Count; num++) {
            missingPlayers.Add(num);
            Debug.Log("Added player " + num);
        }

        // Go through all current controllers
        for (int i = 0; i < Gamepad.all.Count; i++) {
            // Don't continue if all players have a controller assigned to them
            if (missingPlayers.Count == 0) return;

            Debug.Log("Added controller " + i);

            // Make the GamepadPlayer
            GamepadPlayer gamepadPlayer = new GamepadPlayer(Gamepad.all[i].description, 
                                                            Gamepad.all[i], 
                                                            players[missingPlayers.First()],
                                                            missingPlayers.First());

            gamepads.Add(gamepadPlayer);

            // Remove the player from the missing players
            missingPlayers.Remove(missingPlayers.First());
            gamepadPlayer.EnablePlayer();
        }
    }

    // Called when something changed to a device
    private void OnDeviceChange(InputDevice device, InputDeviceChange change) {
        // If a controller was added
        if (change == InputDeviceChange.Added) {
            // If the controller was previously assigned to a player
            if (gamepads.Where(x => x.GetDescription() == device.description).Count() > 0) {
                if (missingPlayers.Count == 0) return;

                // Update the controller of that player
                GamepadPlayer gamepadPlayer = gamepads.Where(x => x.GetDescription() == device.description).First();
                gamepadPlayer.SetGamepad((Gamepad)device);
                Debug.Log("Updated device");

                gamepadPlayer.EnablePlayer();
                missingPlayers.Remove(missingPlayers.First());
            // If it's a new controller
            } else {
                if (missingPlayers.Count == 0) return;

                GamepadPlayer gamepadPlayer = new GamepadPlayer(device.description,
                                                            (Gamepad)device,
                                                            players[missingPlayers.First()],
                                                            missingPlayers.First());

                gamepads.Add(gamepadPlayer);
                Debug.Log("Added device");

                gamepadPlayer.EnablePlayer();
                missingPlayers.Remove(missingPlayers.First());
            }
        }

        // If a controller was removed
        if (change == InputDeviceChange.Removed) {
            // If the controller was assigned to a player
            if (gamepads.Where(x => x.GetDescription() == device.description).Count() > 0) {
                GamepadPlayer gamepadPlayer = gamepads.Where(x => x.GetDescription() == device.description).First();
                missingPlayers.Add(gamepadPlayer.GetPlayerNum());

                gamepadPlayer.SetGamepad(null);
                gamepadPlayer.DisablePlayer();

                Debug.Log("Removed device");
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