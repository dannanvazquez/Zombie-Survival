using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.AI;

public class EnemyBehavior : NetworkBehaviour {
    private float moveSpeed = 2f;

    private void Start() {
        moveSpeed = GetComponent<NavMeshAgent>().speed;
    }

    void Update() {
        if (!isServer) return;

        if (CustomNetworkManager.Instance.players.Count == 0) return;

        Vector3 dest = CustomNetworkManager.Instance.players[0].transform.position;
        foreach (GameObject player in CustomNetworkManager.Instance.players) {
            if (Vector3.Distance(transform.position, player.transform.position) < Vector3.Distance(transform.position, dest)) {
                dest = player.transform.position;
            }
        }

        GetComponent<NavMeshAgent>().SetDestination(dest);
    }

    private void OnTriggerStay(Collider other) {
        if (!isServer) return;

        if (other.gameObject.TryGetComponent(out BarrierController barrier)) {
            if (barrier.activeBarriers > 0) {
                GetComponent<NavMeshAgent>().speed = 0f;
                barrier.DestroyBarrier();
            } else {
                GetComponent<NavMeshAgent>().speed = moveSpeed;
            }
        }
    }
}
