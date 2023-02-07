using Mirror;
using TMPro;
using UnityEngine;

public class WeaponShopController : NetworkBehaviour {
    [SerializeField] private GameObject weaponPrefab;
    private Weapon weapon;

    [SerializeField] private TextMeshProUGUI infoText;

    private void Awake() {
        weapon = weaponPrefab.GetComponent<Weapon>();
        infoText.text = $"{weapon.weaponName}\n{weapon.price}";
    }

    private void OnTriggerEnter(Collider other) {
        if (!other.gameObject.CompareTag("Player") || !isServer) return;
        PlayerController player = other.GetComponent<PlayerController>();

        if (!IsBuyingAmmo(other.GetComponent<WeaponController>())) {
            if (player.gold >= weapon.price) {
                UIManager.Instance.TargetInteractUI(player.GetComponent<NetworkIdentity>().connectionToClient, $"Press E to buy {weapon.weaponName} for {weapon.price} gold");
            } else {
                UIManager.Instance.TargetInteractUI(player.GetComponent<NetworkIdentity>().connectionToClient, $"You need {weapon.price} gold to buy {weapon.weaponName}");
            }
        } else {
            if (player.gold >= weapon.GetComponent<RangedWeapon>().ammoPrice) {
                UIManager.Instance.TargetInteractUI(player.GetComponent<NetworkIdentity>().connectionToClient, $"Press E to buy {weapon.weaponName} ammo for {weapon.GetComponent<RangedWeapon>().ammoPrice} gold");
            } else {
                UIManager.Instance.TargetInteractUI(player.GetComponent<NetworkIdentity>().connectionToClient, $"You need {weapon.GetComponent<RangedWeapon>().ammoPrice} gold to buy {weapon.weaponName} ammo");
            }
        }
    }

    private void OnTriggerStay(Collider other) {
        if (!other.gameObject.CompareTag("Player") || !isServer) return;
        PlayerController player = other.GetComponent<PlayerController>();

        // TODO: Make it so it's not enabling the UI every frame for better performance. We only want to change the UI once the player has sufficient gold.
        if (!IsBuyingAmmo(other.GetComponent<WeaponController>())) {
            if (player.gold >= weapon.price) {
                UIManager.Instance.TargetInteractUI(player.GetComponent<NetworkIdentity>().connectionToClient, $"Press E to buy {weapon.weaponName} for {weapon.price} gold");
            }

            if (!player.isInteracting || player.gold < weapon.price || other.GetComponent<WeaponController>().HasWeapon(weapon.weaponName)) return;

            AddWeapon(player);

            if (weapon.GetComponent<RangedWeapon>() != null) {
                TargetChangeToAmmo(player.GetComponent<NetworkIdentity>().connectionToClient);
            } else {
                UIManager.Instance.TargetDisableInteractUI(player.GetComponent<NetworkIdentity>().connectionToClient);
            }
        } else {
            if (player.gold >= weapon.GetComponent<RangedWeapon>().ammoPrice) {
                UIManager.Instance.TargetInteractUI(player.GetComponent<NetworkIdentity>().connectionToClient, $"Press E to buy {weapon.weaponName} ammo for {weapon.GetComponent<RangedWeapon>().ammoPrice} gold");
            }

            if (!player.isInteracting || player.gold < weapon.GetComponent<RangedWeapon>().ammoPrice || !other.GetComponent<WeaponController>().HasWeapon(weapon.weaponName)) return;

            BuyAmmo(player);
        }
    }

    private void OnTriggerExit(Collider other) {
        if (!other.gameObject.CompareTag("Player") || !isServer) return;
        PlayerController player = other.GetComponent<PlayerController>();

        UIManager.Instance.TargetDisableInteractUI(player.GetComponent<NetworkIdentity>().connectionToClient);
    }

    // Adds the weapon given to the player's inventory. If inventory is full, replaces the weapon the player is currently holding.
    private void AddWeapon(PlayerController player) {
        WeaponController weaponController = player.GetComponent<WeaponController>();

        // If there is an empty slot, auto switches hand slot to newly added weapon. Should it do this?
        for (int i = 0; i < weaponController.weapons.Length; i++) {
            if (weaponController.weapons[i] == null) {
                player.gold -= weapon.price;
                UIManager.Instance.TargetGoldUI(player.GetComponent<NetworkIdentity>().connectionToClient, player.gold);

                RpcAddWeapon(weaponController, i);
                UIManager.Instance.TargetChangeWeaponNameUI(player.GetComponent<NetworkIdentity>().connectionToClient, i, weapon.weaponName);
                return;
            }
        }

        // TODO: Clean this up to not repeat these lines or have to go through all the above process just to fail at this simple if statement.
        if (weaponPrefab.GetComponent<Weapon>().GetType() == weaponController.weapons[weaponController.holdingWeapon].GetComponent<Weapon>().GetType()) {
            player.gold -= weapon.price;
            UIManager.Instance.TargetGoldUI(player.GetComponent<NetworkIdentity>().connectionToClient, player.gold);

            RpcAddWeapon(weaponController, weaponController.holdingWeapon);
            UIManager.Instance.TargetChangeWeaponNameUI(player.GetComponent<NetworkIdentity>().connectionToClient, weaponController.holdingWeapon, weapon.weaponName);
        }
    }

    // Gives the player max ammo and updates the UI if needed.
    private void BuyAmmo(PlayerController player) {
        WeaponController weaponController = player.GetComponent<WeaponController>();

        for (int i = 0; i < weaponController.weapons.Length; i++) {
            Weapon iWeapon = weaponController.weapons[i].GetComponent<Weapon>();
            RangedWeapon iRangedWeapon = weaponController.weapons[i].GetComponent<RangedWeapon>();
            if (weaponController.weapons[i] != null && iWeapon.weaponName == weapon.weaponName) {
                player.gold -= iRangedWeapon.ammoPrice;
                UIManager.Instance.TargetGoldUI(player.GetComponent<NetworkIdentity>().connectionToClient, player.gold);

                iRangedWeapon.currentAmmo = iRangedWeapon.ammoCapacity;
                if (i == weaponController.holdingWeapon) {
                    UIManager.Instance.TargetUpdateAmmoUI(player.GetComponent<NetworkIdentity>().connectionToClient, iRangedWeapon.currentAmmo, iRangedWeapon.ammoCapacity);
                }
                
                return;
            }
        }
    }

    [ClientRpc]
    private void RpcAddWeapon(WeaponController weaponController, int holdingWeapon) {
        GameObject addedWeapon = null;
        foreach (Transform weaponTrans in weaponController.weaponHolder.transform) {
            if (weaponTrans.GetComponent<Weapon>().weaponName == weaponPrefab.GetComponent<Weapon>().weaponName) {
                addedWeapon = weaponTrans.gameObject;
            }
        }
        if (addedWeapon == null) Debug.LogError($"Failed to find the {weaponPrefab.GetComponent<Weapon>().weaponName} weapon in {weaponController.GetComponent<PlayerController>().playerName}'s weaponHolder while trying to add on client.", transform);

        weaponController.weapons[holdingWeapon] = addedWeapon;
        weaponController.SwitchWeapon(holdingWeapon);
    }

    [TargetRpc]
    private void TargetChangeToAmmo(NetworkConnection conn) {
        infoText.text = $"{weapon.weaponName} Ammo\n{weapon.GetComponent<RangedWeapon>().ammoPrice}";
    }

    // Checks if the player is trying to buy ammo
    private bool IsBuyingAmmo(WeaponController weaponController) {
        if (weapon.GetComponent<RangedWeapon>() == null) return false;
        
        for (int i = 0; i < weaponController.weapons.Length; i++) {
            if (weaponController.weapons[i] != null && weaponController.weapons[i].GetComponent<Weapon>().weaponName == weapon.weaponName) {
                return true;
            }
        }

        return false;
    }
}
