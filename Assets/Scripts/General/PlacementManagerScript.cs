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
            gameObject.anchoredPosition = new Vector2(gameObject.anchoredPosition.x, -800 + position * 200);
            this.points.text = points.ToString();
            placement.text = "#" + position.ToString(); ;
        }
    }

    [SerializeField] private List<PlayerPoint> playerPoints;

    [SerializeField] private float waitTime;
    [SerializeField] private float barWidth;
    [SerializeField] private RectTransform bar;

    private float waitTimer;

    private List<PowerupTestScript> players;
    private int playerNum;

    public delegate void OnRespawn();
    public static event OnRespawn onRespawn;

    private void Start() {
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        players = new List<PowerupTestScript>();

        for (int i = 0; i < playerObjects.Length; i++) {
            players.Add(playerObjects[i].GetComponent<PowerupTestScript>());
            playerNum++;

            playerPoints[i].gameObject.gameObject.SetActive(true);
            playerPoints[i].name.text = players[i].GetComponent<PlayerControllerTestScript>().character.ToString();
        }
    }

    private void Update() {
        if (Gamepad.current.buttonSouth.isPressed) {
            waitTimer += Time.deltaTime;
            float width = Mathf.Clamp(barWidth * (waitTimer / waitTime), 0, barWidth);
            bar.sizeDelta = new Vector2(width, bar.sizeDelta.y);

            if (waitTimer >= waitTime) {
                gameObject.SetActive(false);
                onRespawn?.Invoke();
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                return;
            }
        }
    }

    private void OnShowUI() { 
        for (int i = 0; i < playerNum; i++) {
            gameObject.SetActive(true);
            playerPoints[i].SetInformation(i, players[i].GetPoints());
        }
    }

    private void OnEnable() {
        GameManager.onShowUI += OnShowUI;
    }

    private void OnDisable() {
        GameManager.onShowUI -= OnShowUI;
    }
}