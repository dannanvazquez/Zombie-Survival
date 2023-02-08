using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomData", menuName = "ScriptableObjects/RoomData", order = 1)]
public class RoomScriptableObject : ScriptableObject {
    public string roomName;
    public int openDoorPrice;
    public Vector3[] enemySpawnPoints;
}