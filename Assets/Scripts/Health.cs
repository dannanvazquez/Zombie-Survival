using UnityEngine;
using Mirror;

public class Health : MonoBehaviour {
    [SerializeField] private int maxHealth = 100;
    private int currentHealth = 100;

    private void Start() {
        currentHealth = maxHealth;
    }

    // How much damage to deal and destroy self when below 0 health. Gives player gold based on damage and increases kill count if killed.
    public void TakeDamage(int damage, PlayerController player = null) {
        if (player != null) player.gold += damage;
        currentHealth -= damage;
        if (currentHealth <= 0) {
            if (player != null) player.kills++;
            NetworkServer.Destroy(gameObject);
        }
    }
}
