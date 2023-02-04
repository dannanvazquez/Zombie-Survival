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
        for (int i = 0; i < spawnsPerInterval; i++) {
            GameObject zombie = Instantiate(zombiePrefab, currentEnemySpawnPoints[Random.Range(0, currentEnemySpawnPoints.Count)], Quaternion.identity);
            NetworkServer.Spawn(zombie);
        }
        yield return new WaitForSeconds(spawnIntervals);
        StartCoroutine(SpawnEnemies());
    }
}
