using UnityEngine;
using Mirror;
using Unity.VisualScripting;
using System.Collections;

public class Health : NetworkBehaviour {
    [SerializeField] private int maxHealth = 100;
    private int currentHealth = 100;

    private void Start() {
        currentHealth = maxHealth;
    }

    // How much damage to deal and destroy self when below 0 health. Gives player gold based on damage and increases kill count if killed.
    public void TakeDamage(int damage, PlayerController killedByPlayer = null) {
        if (currentHealth == 0) return;
        if (currentHealth - damage < 0) damage = currentHealth;
        if (killedByPlayer != null) killedByPlayer.gold += damage;

        currentHealth -= damage;

        if (GetComponent<PlayerController>() != null) {
            TargetHealthUI(GetComponent<NetworkIdentity>().connectionToClient, currentHealth);
        }

        if (currentHealth == 0) {
            if (killedByPlayer != null) killedByPlayer.kills++;
            if (GetComponent<PlayerController>() != null) {
                StartCoroutine(KillPlayer());
            } else {
                NetworkServer.Destroy(gameObject);
            }
        }
    }

    // Heals the player but not over its maxHealth.
    public void Heal(int healAmount) {
        if (currentHealth + healAmount > maxHealth) healAmount = maxHealth - currentHealth;

        currentHealth += healAmount;

        if (GetComponent<PlayerController>() != null) {
            TargetHealthUI(GetComponent<NetworkIdentity>().connectionToClient, currentHealth);
        }
    }

    // Updates the UI to the local player's currentHealth.
    [TargetRpc]
    private void TargetHealthUI(NetworkConnection conn, int health) {
        UIManager.Instance.healthBarUI.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, health * 2);
        UIManager.Instance.healthText.text = $"{health}/{maxHealth}";
    }

    // Stop the player from moving and after a few seconds, allow them to move with full health.
    IEnumerator KillPlayer() {
        GetComponent<PlayerController>().RpcCanMove(false);
        yield return new WaitForSeconds(10f);
        GetComponent<PlayerController>().RpcCanMove(true);
        Heal(maxHealth);
    }
}
