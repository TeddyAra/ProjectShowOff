using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Mathematics;
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
    [SerializeField] private string[] gameSceneNames;
    [SerializeField] private int maxRounds;

    private float waitTimer;
    private int playerNum;
    private Gamepad current;
    private System.Random random;
    private int roundCount;
    private bool respawning;
    private string gameSceneName;

    public delegate void OnRespawn(List<PlayerControllerTestScript> positions);
    public static event OnRespawn onRespawn;

    private void Start() {
        DontDestroyOnLoad(gameObject);
        random = new System.Random();
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
        if (respawning) {
            if (SceneManager.GetSceneByName(gameSceneName).isLoaded) {
                PlayerControllerTestScript[] players = FindObjectsOfType<PlayerControllerTestScript>();
                List<int> nums = new List<int>();
                for (int i = 0; i < players.Length; i++) {
                    nums.Add(i);
                }

                List<PlayerControllerTestScript> positions = new List<PlayerControllerTestScript>();
                for (int i = 0; i < nums.Count; i++) {
                    int index = random.Next(nums.Count);
                    positions.Add(players[index]);
                    nums.Remove(index);
                }

                onRespawn?.Invoke(positions);
                respawning = false;
            }
        }

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
                    roundCount++;
                    if (roundCount >= maxRounds) {
                        PlayerControllerTestScript[] players = FindObjectsOfType<PlayerControllerTestScript>();
                        GameObject camera = FindObjectOfType<Camera>().gameObject;
                        GameObject sfxManager = FindObjectOfType<SFXManager>().gameObject;

                        foreach (PlayerControllerTestScript p in players) Destroy(p.gameObject);
                        Destroy(camera);
                        Destroy(sfxManager);

                        SceneManager.LoadScene("CharacterSelectorScene");
                        Destroy(gameObject);
                    } else {
                        background.SetActive(false);
                        gameSceneName = gameSceneNames[random.Next(0, gameSceneNames.Length)];
                        SceneManager.LoadScene(gameSceneName);

                        respawning = true;
                    }
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