using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Round", menuName = "ScriptableObjects/Rounds", order = 1)]
public class RoundScriptableObject : ScriptableObject
{
    public GameObject baseEnemy = null;
    public GameObject boss = null;

    // percentChanceSpawn is the percentage chance for which it spawns the additionalEnemies in the same order.
    public GameObject[] additionalEnemies = null;
    public float[] percentChanceSpawn = null;

    public int enemyCount = 0;

    public List<GameObject> spawnOrder = new();

    public void MakeList() {
        if (additionalEnemies.Length != percentChanceSpawn.Length) Debug.LogError($"{name}'s percentChanceSpawn should be the same size as additionalEnemies as they depend on each other.");

        float totalPercChance = 0;
        for (int i = 0; i < percentChanceSpawn.Length; i++) {
            totalPercChance += percentChanceSpawn[i];
        }
        if (totalPercChance > 100f) {
            Debug.LogError($"{name}'s percentChanceSpawn has a total of {totalPercChance}% when it should be no more than 100%.");
            return;
        }

        for (int i = 0; i < enemyCount; i++) {
            if (i + 1 == enemyCount && boss != null) {
                spawnOrder.Add(boss);
                break;
            }

            // TODO: Maybe change how this works. Instead have a set amount for each additional mob and randomly replace baseEnemy in a list.
            float randomNum = Random.Range(0f, 100f);
            float percCheck = 100f - totalPercChance;  // The percentage threshold that we are checking under for. Starts at the baseEnemy's percentage chance.

            if (randomNum < percCheck) {
                spawnOrder.Add(baseEnemy);
            } else {
                for (int j = 0; j < percentChanceSpawn.Length; j++) {
                    percCheck += percentChanceSpawn[j];
                    if (randomNum < percCheck) {
                        spawnOrder.Add(additionalEnemies[j]);
                        break;
                    }

                    if (j + 1 == percentChanceSpawn.Length) {
                        Debug.LogError($"{name} failed to assign an enemy based on randomNum({randomNum}). Assigning baseEnemy.");
                        spawnOrder.Add(baseEnemy);
                    }
                }
            }
        }
    }
}
