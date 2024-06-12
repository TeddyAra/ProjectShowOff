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

        [HideInInspector] public PowerupTestScript script;

        public void SetInformation(int position) {
            gameObject.anchoredPosition = new Vector2(gameObject.anchoredPosition.x, 550 - position * 200);
            points.text = script.GetPoints().ToString();
            placement.text = "#" + position.ToString(); ;
        }
    }

    [SerializeField] private List<PlayerPoint> playerPoints;

    [SerializeField] private GameObject background;
    [SerializeField] private float waitTime;
    [SerializeField] private float barWidth;
    [SerializeField] private RectTransform bar;

    [SerializeField] private List<int> placementPoints;

    private float waitTimer;
    private int playerNum;
    private Gamepad current;

    public delegate void OnRespawn();
    public static event OnRespawn onRespawn;

    private void Start() {
        DontDestroyOnLoad(gameObject);
    }

    public void Apply(List<PowerupTestScript> players) {
        playerNum = players.Count;

        for (int i = 0; i < playerNum; i++) {
            PlayerPoint plr = playerPoints[i];
            plr.gameObject.gameObject.SetActive(true);
            plr.name.text = players[i].GetComponent<PlayerControllerTestScript>().character.ToString();
            plr.script = players[i];
            playerPoints[i] = plr;
        }
    }

    private void Update() {
        if (!background.activeSelf) return;

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
                    background.SetActive(false);
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
        Debug.Log("WOMP WOMP");
        background.SetActive(true);

        // Give the players points based on their placement
        playerPoints = playerPoints.GetRange(0, playerNum).OrderBy(x => x.script.transform.position.x).ToList();
        for (int i = 0; i < playerPoints.Count; i++) {
            PlayerPoint plr = playerPoints[i];
            plr.script.GivePoints(placementPoints[i]);
            playerPoints[i] = plr;
        }

        // Order players by their points
        playerPoints = playerPoints.GetRange(0, playerNum).OrderBy(x => -x.script.GetPoints()).ToList();
        for (int i = 0; i < playerNum; i++) {
            PlayerPoint plr = playerPoints[i];
            plr.gameObject.gameObject.SetActive(true);
            plr.SetInformation(i + 1);
            playerPoints[i] = plr;
        }
    }

    private void OnEnable() {
        GameManager.onShowUI += OnShowUI;
    }

    private void OnDisable() {
        GameManager.onShowUI -= OnShowUI;
    }
}