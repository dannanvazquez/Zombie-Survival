using UnityEngine;
using Mirror;
using UnityEngine.AI;

public class EnemyBehavior : NetworkBehaviour {
    private float moveSpeed = 2f;
    [SerializeField] private int damage;
    [SerializeField] private float damageCooldown = 2f;
    private float damageTimer = 0f;

    private void Start() {
        moveSpeed = GetComponent<NavMeshAgent>().speed;
    }

    private void Update() {
        if (!isServer) return;

        if (damageTimer > 0) damageTimer -= Time.deltaTime;

        if (CustomNetworkManager.Instance.players.Count == 0) return;

        // Find the closest player and set enemy's destination towards them.
        Vector3 dest = CustomNetworkManager.Instance.players[0].transform.position;
        foreach (GameObject player in CustomNetworkManager.Instance.players) {
            if (player != null && player.GetComponent<PlayerController>() && player.GetComponent<PlayerController>().canMove && Vector3.Distance(transform.position, player.transform.position) < Vector3.Distance(transform.position, dest)) {
                dest = player.transform.position;
            }
        }
        GetComponent<NavMeshAgent>().SetDestination(dest);
    }

    private void OnTriggerStay(Collider other) {
        if (!isServer) return;

        // When colliding with a barrier trigger that is active, call DestroyBarrier to destroy it.
        if (other.gameObject.TryGetComponent(out BarrierController barrier)) {
            if (barrier.activeBarriers > 0) {
                GetComponent<NavMeshAgent>().speed = 0f;
                barrier.DestroyBarrier();
            } else {
                GetComponent<NavMeshAgent>().speed = moveSpeed;
            }
        }

        // When colliding with a player, deal damage to them every damageCooldown seconds.
        if (other.gameObject.TryGetComponent(out PlayerController player)) {
            if (damageTimer <= 0) {
                other.GetComponent<Health>().TakeDamage(damage);
                damageTimer = damageCooldown;
            }
        }
    }
}
