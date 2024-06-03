using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManagerTestScript : MonoBehaviour {
    struct PlayerBar {
        public Image progressionBar;
        public TMP_Text powerup;
        public TMP_Text place;
        public PlayerControllerTestScript player;

        public void ApplyVariables(Transform bar, PlayerControllerTestScript player) {
            progressionBar = bar.Find("Ability bar").GetComponent<Image>();
            powerup = bar.Find("Ability").GetComponent<TMP_Text>();
            place = bar.Find("Placement").GetComponent<TMP_Text>();

            this.player = player;
        }
    }

    [SerializeField] private float playerBarDistance;
    [SerializeField] private float extraBarDistance;
    [SerializeField] private List<Transform> playerBars;

    private List<PlayerBar> bars;

    private void Start() {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (playerBars.Count() < players.Count()) Debug.LogError("[ERROR] There aren't enough playerBars");

        playerBarDistance += (-players.Count() + 4) * extraBarDistance;
        bars = new List<PlayerBar>();

        for (int i = 0; i < players.Count(); i++) {
            PlayerBar playerBar = new PlayerBar();
            playerBar.ApplyVariables(playerBars[i], players[i].GetComponent<PlayerControllerTestScript>());
            bars.Add(playerBar);

            RectTransform rect = playerBars[i].GetComponent<RectTransform>();
            Vector2 barPosition = rect.anchoredPosition;
            barPosition.x = (i + 0.5f) * playerBarDistance - players.Count() * playerBarDistance / 2;
            rect.anchoredPosition = barPosition;
        }
    }

    private void ChangePowerup(PlayerControllerTestScript player, string powerup) {
        foreach (PlayerBar bar in bars) {
            if (bar.player == player) {
                bar.powerup.text = powerup;
                return;
            }
        }
    }

    private void OnEnable() {
        PlayerControllerTestScript.onPowerup += ChangePowerup;
    }

    private void OnDisable() {
        PlayerControllerTestScript.onPowerup += ChangePowerup;
    }
}