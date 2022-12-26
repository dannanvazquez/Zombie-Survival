using UnityEngine;
using Mirror;

public class Health : MonoBehaviour {
    [SerializeField] private int maxHealth = 100;
    private int currentHealth = 100;

    private void Start() {
        currentHealth = maxHealth;
    }

    // How much damage to deal and destroy self when below 0 health.
    public void TakeDamage(int damage) {
        currentHealth -= damage;
        if (currentHealth <= 0) {
            NetworkServer.Destroy(gameObject);
        }
    }
}
