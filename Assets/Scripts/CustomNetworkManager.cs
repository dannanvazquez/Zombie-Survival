using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CustomNetworkManager : NetworkManager {
    public List<GameObject> players;
    public static CustomNetworkManager Instance { get; private set; }

    public override void Awake() {
        // If there is an instance, and it's not me, delete myself.
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn) {
        GameObject player = Instantiate(playerPrefab, FindObjectOfType<NetworkStartPosition>().transform.position, Quaternion.identity);
        player.GetComponent<PlayerController>().playerName = $"Player_{numPlayers + 1}";
        NetworkServer.AddPlayerForConnection(conn, player);
        players.Add(player);
    }
}
