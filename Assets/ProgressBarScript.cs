using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBarScript : MonoBehaviour {
    [Serializable]
    private struct CharacterHead {
        public PlayerControllerTestScript.Character character;
        public Sprite characterHead;
    }

    [SerializeField] private GameObject playerIconPrefab;
    [SerializeField] private float totalLength;
    [SerializeField] private List<CharacterHead> characterHeads;

    private Dictionary<RectTransform, Transform> playerPositions;
    private Dictionary<PlayerControllerTestScript.Character, Sprite> characterSprites;
    private float start;
    private float totalDistance;

    private void Start() {
        playerPositions = new Dictionary<RectTransform, Transform>();
        characterSprites = new Dictionary<PlayerControllerTestScript.Character, Sprite>();

        foreach (CharacterHead characterHead in characterHeads) {
            characterSprites.Add(characterHead.character, characterHead.characterHead);
        }

        Vector3 spawnPosition = GameObject.FindGameObjectWithTag("SpawnPoint").transform.position;
        Vector3 finishPosition = GameObject.FindGameObjectWithTag("Finish").transform.position;

        totalDistance = finishPosition.x - spawnPosition.x;
        start = spawnPosition.x;
        StartCoroutine(FindPlayers());
    }

    private IEnumerator FindPlayers() {
        PlayerControllerTestScript[] players;
        do {
            players = FindObjectsOfType<PlayerControllerTestScript>();
            Debug.Log(players.Length);
            yield return new WaitForSeconds(0.1f);
        } while (players.Length == 0);

        foreach (PlayerControllerTestScript player in players) {
            RectTransform rect = Instantiate(playerIconPrefab, transform.parent).GetComponent<RectTransform>();
            playerPositions.Add(rect, player.transform);

            Image img = rect.transform.GetChild(0).GetComponent<Image>();
            img.sprite = characterSprites[player.character];
        }
    }

    private void Update() {
        foreach (var position in playerPositions) {
            float distance = Mathf.Clamp((position.Value.position.x - start) / totalDistance, 0, 1) * totalLength;
            position.Key.anchoredPosition = new Vector2(-totalLength / 2 + distance, position.Key.anchoredPosition.y);
        }
    }
}