using System.Collections;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(PlayerController))]
public class WeaponController : NetworkBehaviour {
    [SerializeField] private GameObject[] weapons;
    private int holdingWeapon = 1;

    private bool canAttack = true;
    private PlayerController playerController;

    private void Start() {
        playerController = GetComponent<PlayerController>();
    }

    private void Update() {
        if (!isLocalPlayer) return;

        // Shoot when the player presses the Fire1 button.
        if (canAttack && playerController.canMove) {
            if (weapons[holdingWeapon].GetComponent<MeleeWeapon>()) {
                if (Input.GetButtonDown("Fire1")) {
                    canAttack = false;
                    CmdHit();
                    StartCoroutine(StartCooldown());
                }
            } else {
                if (Input.GetButton("Fire1")) {
                    canAttack = false;
                    CmdHit();
                    StartCoroutine(StartCooldown());
                }
            }
        }

        // Switch weapons
        if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchWeapon(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchWeapon(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchWeapon(2);
    }

    // Tell the server the player wants to hit
    [Command]
    private void CmdHit() {
        weapons[holdingWeapon].GetComponent<Weapon>().Attack();
    }

    private void SwitchWeapon(int weapon) {
        if (weapons[weapon] == null || weapon == holdingWeapon) return;

        UIManager.Instance.weaponsUI[holdingWeapon].offsetMin = new Vector2(0, UIManager.Instance.weaponsUI[holdingWeapon].offsetMin.y);
        UIManager.Instance.weaponsUI[weapon].offsetMin = new Vector2(-50, UIManager.Instance.weaponsUI[weapon].offsetMin.y);
        CmdSwitchWeapon(weapon);
    }

    // Tell the server the player wants to switch weapons
    [Command]
    private void CmdSwitchWeapon(int weapon) {
        RpcSwitchWeapon(weapon);
    }

    // Show weapons switched for a player on all clients
    [ClientRpc]
    private void RpcSwitchWeapon(int weapon) {
        weapons[holdingWeapon].SetActive(false);
        holdingWeapon = weapon;
        weapons[holdingWeapon].SetActive(true);
    }

    // Cooldown before shooting again
    IEnumerator StartCooldown() {
        yield return new WaitForSeconds(weapons[holdingWeapon].GetComponent<Weapon>().attackCooldown);
        canAttack = true;
    }
}