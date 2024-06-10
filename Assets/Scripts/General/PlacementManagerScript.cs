using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlacementManagerScript : MonoBehaviour {
    [Serializable]
    public struct PlayerPoint {
        public RectTransform gameObject;
        public TMP_Text name;
        public TMP_Text points;
        public TMP_Text placement;

        public void SetInformation(int position, int points) {
            gameObject.anchoredPosition = new Vector2(gameObject.anchoredPosition.x, 550 - position * 200);
            this.points.text = points.ToString();
            placement.text = "#" + position.ToString(); ;
        }
    }

    [SerializeField] private List<PlayerPoint> playerPoints;

    [SerializeField] private GameObject background;
    [SerializeField] private float waitTime;
    [SerializeField] private float barWidth;
    [SerializeField] private RectTransform bar;

    private float waitTimer;
    private List<int> playerIndices;
    private List<PowerupTestScript> players;
    private int playerNum;
    private Gamepad current;

    public delegate void OnRespawn();
    public static event OnRespawn onRespawn;

    public void Apply(List<PowerupTestScript> players) {
        playerIndices = new List<int>();
        this.players = players;
        playerNum = players.Count;

        for (int i = 0; i < playerNum; i++) {
            playerIndices.Add(i);

            playerPoints[i].gameObject.gameObject.SetActive(true);
            playerPoints[i].name.text = players[i].GetComponent<PlayerControllerTestScript>().character.ToString();
        }
    }

    private void Update() {
        if (current == null) {
            if (Gamepad.current.buttonSouth.isPressed) {
                current = Gamepad.current;
            }
        }

        if (current != null) {
            if (current.buttonSouth.isPressed) {
                waitTimer += Time.deltaTime;
                float width = Mathf.Clamp(barWidth * (waitTimer / waitTime), 0, barWidth);
                bar.sizeDelta = new Vector2(width, bar.sizeDelta.y);

                if (waitTimer >= waitTime) {
                    gameObject.SetActive(false);
                    onRespawn?.Invoke();
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                    return;
                }
            } else {
                current = null;
                waitTimer = 0;
                bar.sizeDelta = new Vector2(0, bar.sizeDelta.y);
            }
        }
    }

    private void OnShowUI() {
        background.SetActive(true);
        playerIndices.OrderBy(x => players[x].GetPoints());

        for (int i = 0; i < playerNum; i++) {
            playerPoints[i].gameObject.gameObject.SetActive(true);
            Debug.Log(playerIndices.IndexOf(i) * -1 + playerIndices.Count);
            playerPoints[i].SetInformation(playerIndices.IndexOf(i) * -1 + playerIndices.Count, players[i].GetPoints());
        }
    }

    private void OnEnable() {
        GameManager.onShowUI += OnShowUI;
    }

    private void OnDisable() {
        GameManager.onShowUI -= OnShowUI;
    }
}