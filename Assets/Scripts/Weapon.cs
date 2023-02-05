using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    public string weaponName = "";
    public int baseDamage = 10;
    public float attackCooldown = 1f;
    public float maxDistance = 25f;
    public int price = 500;
    public int goldPerHit = 10;

    public PlayerController playerController;

    public abstract void Attack();
}
