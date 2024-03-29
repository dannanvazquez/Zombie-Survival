using UnityEngine;
using Mirror;
using System.Collections;
using System;

public class Health : NetworkBehaviour {
    [SerializeField] private int maxHealth = 100;
    private int currentHealth = 100;
    [SerializeField] private int healthRegen = 0;

    [SerializeField] public Armor topPieceArmor;
    [SerializeField] public Armor bottomPieceArmor;

    private void Start() {
        currentHealth = maxHealth;
        StartCoroutine(RegenHealth());
    }

    // How much damage to deal and destroy self when below 0 health. Gives player gold based on damage and increases kill count if killed.
    public void TakeDamage(int damage, int gold = 0, PlayerController killedByPlayer = null) {
        if (currentHealth <= 0) return;
        if (killedByPlayer != null) {
            killedByPlayer.gold += gold;
            UIManager.Instance.TargetGoldUI(killedByPlayer.GetComponent<PlayerController>().connectionToClient, killedByPlayer.gold);
        }

        float damageReduc = 1;
        if (topPieceArmor != null) damageReduc -= topPieceArmor.reductionPerc;
        if (bottomPieceArmor != null) damageReduc -= bottomPieceArmor.reductionPerc;
        damage = (int)Math.Round(damage * damageReduc, 0);

        currentHealth = (currentHealth - damage < 0 ? 0 : currentHealth - damage);

        if (GetComponent<PlayerController>() != null) {
            TargetHealthUI(GetComponent<NetworkIdentity>().connectionToClient, currentHealth);
        }

        if (currentHealth == 0) {
            if (killedByPlayer != null) killedByPlayer.kills++;
            if (GetComponent<PlayerController>() != null) {
                StartCoroutine(KillPlayer());
            } else {
                GameManager.Instance.enemiesLeft--;
                UIManager.Instance.RpcUpdateRemainingEnemiesUI(GameManager.Instance.enemiesLeft);

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
        UIManager.Instance.healthBarUI.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, health * 3);
        UIManager.Instance.healthText.text = $"{health}/{maxHealth}";
    }

    // Stop the player from moving and after a few seconds, allow them to move with full health.
    IEnumerator KillPlayer() {
        GetComponent<PlayerController>().RpcCanMove(false);
        yield return new WaitForSeconds(10f);
        GetComponent<PlayerController>().RpcCanMove(true);
        Heal(maxHealth);
    }

    // Regens health by healthRegen every second.
    IEnumerator RegenHealth() {
        yield return new WaitForSeconds(1f);

        if (currentHealth + healthRegen > maxHealth) {
            currentHealth = maxHealth;
        } else {
            currentHealth += healthRegen;
        }

        if (GetComponent<PlayerController>() != null) {
            TargetHealthUI(GetComponent<NetworkIdentity>().connectionToClient, currentHealth);
        }

        StartCoroutine(RegenHealth());
    }

    public void AddArmor(Armor armor) {
        if (armor.isTopPiece) {
            topPieceArmor = armor;
        } else {
            bottomPieceArmor = armor;
        }
    }

    public bool IsArmorUpgrade(Armor armor) {
        if (armor.isTopPiece) {
            if (topPieceArmor == null || armor.reductionPerc > topPieceArmor.reductionPerc) {
                return true;
            }
        } else {
            if (bottomPieceArmor == null || armor.reductionPerc > bottomPieceArmor.reductionPerc) {
                return true;
            }
        }

        return false;
    }
}
