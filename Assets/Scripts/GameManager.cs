using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }

    public int minPlayers = 2;
    [SerializeField] private GameObject zombiePrefab;
    private List<Vector3> currentEnemySpawnPoints = new();
    [SerializeField] private float spawnIntervals;
    [SerializeField] private float spawnsPerInterval;
    [SerializeField] private List<RoomScriptableObject> roomsData = new();
    private int round = 1;

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
    }

    // Spawns enemies at random spawns and once spawnIntervals is over, continue looping.
    IEnumerator SpawnEnemies() {
        if (currentEnemySpawnPoints.Count == 0) {
            Debug.LogError("Trying to spawn enemies when currentEnemySpawnPoints is empty.", transform);
            yield break;
        }

        int spawnIterator = 0;
        for (int i = 0; i < spawnsPerInterval * round; i++) {
            GameObject zombie = Instantiate(zombiePrefab, currentEnemySpawnPoints[spawnIterator], Quaternion.identity);
            NetworkServer.Spawn(zombie);

            if (spawnIterator + 1 < currentEnemySpawnPoints.Count) {
                spawnIterator++;
            } else {
                spawnIterator = 0;
                // Debatable if there should be cooldown before an enemy spawns again in the same window. Decide later.
                //yield return new WaitForSeconds(1);
            }
        }

        yield return new WaitForSeconds(spawnIntervals);
        round++;
        StartCoroutine(SpawnEnemies());
    }
}
