using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }

    public int minPlayers = 2;
    private List<Vector3> currentEnemySpawnPoints = new();
    [SerializeField] private List<RoomScriptableObject> roomsData = new();
    [SerializeField] private List<RoundScriptableObject> roundsData = new();
    [HideInInspector] public int roundsCompleted = 0;
    [HideInInspector] public int enemiesLeft = 0;
    [HideInInspector] public int secsElapsed = 0;

    private void Awake() {
        // If there is an instance, and it's not me, delete myself.
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;

        // For rooms already opened before the game starts, add the enemy spawn points in that room to currentEnemySpawnPoints.
        foreach (RoomScriptableObject data in roomsData) {
            foreach (Vector3 enemySpawnPoint in data.enemySpawnPoints) {
                currentEnemySpawnPoints.Add(enemySpawnPoint);
            }
        }
    }

    public void OpenRoom(RoomScriptableObject roomData) {
        foreach (RoomScriptableObject data in roomsData) {
            if (data.roomName == roomData.roomName) {
                return;
            }
        }

        foreach (Vector3 enemySpawnPoint in roomData.enemySpawnPoints) {
            currentEnemySpawnPoints.Add(enemySpawnPoint);
        }
        roomsData.Add(roomData);
    }

    public void StartGame() {
        StartCoroutine(SpawnEnemies());
        StartCoroutine(Timer());
    }

    // Spawns enemies at random spawns and once spawnIntervals is over, continue looping.
    IEnumerator SpawnEnemies() {
        if (currentEnemySpawnPoints.Count == 0) {
            Debug.LogError("Trying to spawn enemies when currentEnemySpawnPoints is empty.", transform);
            yield break;
        }

        for (roundsCompleted = 0; roundsCompleted < roundsData.Count; roundsCompleted++) {
            UIManager.Instance.RpcUpdateRoundUI(roundsCompleted + 1);

            enemiesLeft = roundsData[roundsCompleted].enemyCount;
            UIManager.Instance.RpcUpdateRemainingEnemiesUI(enemiesLeft);

            roundsData[roundsCompleted].MakeList();

            int spawnIterator = 0;
            for (int i = 0; i < roundsData[roundsCompleted].enemyCount; i++) {
                GameObject enemy = Instantiate(roundsData[roundsCompleted].spawnOrder[i], currentEnemySpawnPoints[spawnIterator], Quaternion.identity);
                NetworkServer.Spawn(enemy);

                if (spawnIterator + 1 < currentEnemySpawnPoints.Count) {
                    spawnIterator++;
                } else {
                    spawnIterator = 0;
                }
            }

            while (enemiesLeft > 0) {
                yield return new WaitForSeconds(1f);
            }

            if (roundsCompleted + 1 == roundsData.Count) {
                // TODO: Make win screen here with stats
                Debug.Log("You Won!");
                break;
            }
        }

    }

    IEnumerator Timer() {
        yield return new WaitForSeconds(1f);

        if (secsElapsed < 360000) secsElapsed++;

        UIManager.Instance.RpcUpdateTimeElapsedUI(secsElapsed);

        StartCoroutine(Timer());
    }
}
