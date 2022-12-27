using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }

    public int minPlayers = 2;
    [SerializeField] private GameObject zombiePrefab;
    public List<Transform> enemySpawnPoints = new();
    [SerializeField] private float spawnIntervals;
    [SerializeField] private float spawnsPerInterval;

    private void Awake() {
        // If there is an instance, and it's not me, delete myself.
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
    }

    public void StartGame() {
        StartCoroutine(SpawnEnemies());
    }

    // Spawns enemies at random spawns and once spawnIntervals is over, continue looping.
    IEnumerator SpawnEnemies() {
        for (int i = 0; i < spawnsPerInterval; i++) {
            GameObject zombie = Instantiate(zombiePrefab, enemySpawnPoints[Random.Range(0, enemySpawnPoints.Count)].position, Quaternion.identity);
            NetworkServer.Spawn(zombie);
        }
        yield return new WaitForSeconds(spawnIntervals);
        StartCoroutine(SpawnEnemies());
    }
}
