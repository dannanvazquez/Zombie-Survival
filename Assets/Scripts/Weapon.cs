using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public abstract class Weapon : MonoBehaviour
{
    public string weaponName = "";
    public int baseDamage = 10;
    public float attackCooldown = 1f;
    public float maxDistance = 25f;

    public PlayerController playerController;

    public abstract void Attack();
}
