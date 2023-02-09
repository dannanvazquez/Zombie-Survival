using System.Collections;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(PlayerController))]
public class WeaponController : NetworkBehaviour {
    public GameObject weaponHolder;
    public GameObject[] weapons;
    [HideInInspector] public int holdingWeapon = 1;

    private bool canAttack = true;
    private PlayerController playerController;

    private void Start() {
        playerController = GetComponent<PlayerController>();

        for (int i = 0; i < weapons.Length; i++) {
            if (weapons[i] != null) {
                UIManager.Instance.weaponNames[i].text = weapons[i].GetComponent<Weapon>().weaponName;
            } else {
                UIManager.Instance.weaponNames[i].text = "Empty";
            }
        }
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

        if (Input.GetKeyDown(KeyCode.R)) CmdReload();
    }

    // Tell the server the player wants to hit
    [Command]
    private void CmdHit() {
        RangedWeapon rangedWeapon = weapons[holdingWeapon].GetComponent<RangedWeapon>();
        if (rangedWeapon != null && rangedWeapon.currentClip > 0 && !rangedWeapon.isReloading) {
            rangedWeapon.currentClip--;
            UIManager.Instance.TargetUpdateAmmoUI(GetComponent<NetworkIdentity>().connectionToClient, rangedWeapon.currentClip, rangedWeapon.currentAmmo);
            weapons[holdingWeapon].GetComponent<Weapon>().Attack();
            
            if (rangedWeapon.currentClip == 0) {
                StartCoroutine(weapons[holdingWeapon].GetComponent<RangedWeapon>().Reload());
            }
        }
    }

    public void SwitchWeapon(int weapon) {
        if (weapons[weapon] == null || !isLocalPlayer) return;

        UIManager.Instance.weaponsUI[holdingWeapon].offsetMin = new Vector2(0, UIManager.Instance.weaponsUI[holdingWeapon].offsetMin.y);
        UIManager.Instance.weaponsUI[weapon].offsetMin = new Vector2(-50, UIManager.Instance.weaponsUI[weapon].offsetMin.y);

        CmdSwitchWeapon(weapon);
    }

    // Tell the server the player wants to switch weapons
    [Command]
    private void CmdSwitchWeapon(int weapon) {
        RangedWeapon rangedWeapon = weapons[weapon].GetComponent<RangedWeapon>();
        if (rangedWeapon != null) {
            UIManager.Instance.TargetUpdateAmmoUI(GetComponent<NetworkIdentity>().connectionToClient, rangedWeapon.currentClip, rangedWeapon.currentAmmo);
        } else {
            UIManager.Instance.TargetDisableAmmoUI(GetComponent<NetworkIdentity>().connectionToClient);
        }
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

    public bool HasWeapon(string weaponName) {
        for (int i = 0; i < weapons.Length; i++) {
            if (weapons[i] != null && weapons[i].GetComponent<Weapon>().weaponName == weaponName) {
                return true;
            }
        }
        return false;
    }

    [Command]
    private void CmdReload() {
        StartCoroutine(weapons[holdingWeapon].GetComponent<RangedWeapon>().Reload());
    }
}