using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManagerTestScript : MonoBehaviour {
    [Serializable]
    struct PlayerBar {
        private Image progressionBar;
        private Image playerIcon;
        private Image powerup;
        private Text place;
        private PlayerControllerTestScript player;

        public void ApplyVariables(Transform bar, PlayerControllerTestScript player) {
            progressionBar = bar.Find("Bar").GetComponent<Image>();
            playerIcon = bar.Find("Icon").GetComponent<Image>();
            powerup = bar.Find("Powerup").GetComponent<Image>();
            place = bar.Find("Place").GetComponent<Text>();

            this.player = player;
        }
    }

    [SerializeField] private float playerBarDistance;
    [SerializeField] private float extraBarDistance;
    [SerializeField] private List<Transform> playerBars;

    private void Start() {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (playerBars.Count() < players.Count()) Debug.LogError("[ERROR] There aren't enough playerBars");

        playerBarDistance += (-players.Count() + 4) * extraBarDistance;

        for (int i = 0; i < players.Count(); i++) {
            PlayerBar playerBar = new PlayerBar();
            playerBar.ApplyVariables(playerBars[i], players[i].GetComponent<PlayerControllerTestScript>());

            RectTransform rect = playerBars[i].GetComponent<RectTransform>();
            Vector2 barPosition = rect.anchoredPosition;
            barPosition.x = (i + 0.5f) * playerBarDistance - players.Count() * playerBarDistance / 2;
            rect.anchoredPosition = barPosition;
        }
    }
}