using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;

public class GamepadManagerTestScript : MonoBehaviour {
    struct GamepadPlayer {
        InputDeviceDescription desc;
        Gamepad gamepad;
        PlayerControllerTestScript script;
        int playerNum;

        public GamepadPlayer(InputDeviceDescription desc, Gamepad gamepad, PlayerControllerTestScript script, int playerNum) {
            this.desc = desc;
            this.gamepad = gamepad;
            this.script = script;
            this.playerNum = playerNum;
        }

        public void EnablePlayer() {
            script.ChangeGamepad(gamepad);
        }

        public void DisablePlayer() {
            script.ChangeGamepad(null);
        }

        public InputDeviceDescription GetDescription() {
            return desc;
        }

        public void SetGamepad(Gamepad gamepad) {
            this.gamepad = gamepad;
        }

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

        for (int num = 0; num < players.Count; num++) {
            missingPlayers.Add(num);
        }

        for (int i = 0; i < Gamepad.all.Count; i++) {
            if (missingPlayers.Count == 0) continue;

            GamepadPlayer gamepadPlayer = new GamepadPlayer(Gamepad.all[i].description, 
                                                            Gamepad.all[i], 
                                                            players[missingPlayers.First()],
                                                            missingPlayers.First());

            gamepads.Add(gamepadPlayer);
            missingPlayers.Remove(missingPlayers.First());
            Debug.Log("Added device");

            gamepadPlayer.EnablePlayer();
        }
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change) {
        if (change == InputDeviceChange.Added) {
            if (gamepads.Where(x => x.GetDescription() == device.description).Count() > 0) {
                if (missingPlayers.Count == 0) return;

                GamepadPlayer gamepadPlayer = gamepads.Where(x => x.GetDescription() == device.description).First();
                gamepadPlayer.SetGamepad((Gamepad)device);
                Debug.Log("Updated device");

                gamepadPlayer.EnablePlayer();
                missingPlayers.Remove(missingPlayers.First());
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

        if (change == InputDeviceChange.Removed) {
            if (gamepads.Where(x => x.GetDescription() == device.description).Count() > 0) {
                GamepadPlayer gamepadPlayer = gamepads.Where(x => x.GetDescription() == device.description).First();
                missingPlayers.Add(gamepadPlayer.GetPlayerNum());
                gamepadPlayer.SetGamepad(null);
                Debug.Log("Removed device");

                gamepadPlayer.DisablePlayer();
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