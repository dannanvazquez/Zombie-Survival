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
        if (!other.gameObject.CompareTag("Player")) return;
        PlayerController player = other.GetComponent<PlayerController>();

        if (player.gold >= weapon.price) {
            UIManager.Instance.TargetInteractUI(player.GetComponent<NetworkIdentity>().connectionToClient, $"Press E to buy {weapon.weaponName} for {weapon.price} gold");
        } else {
            UIManager.Instance.TargetInteractUI(player.GetComponent<NetworkIdentity>().connectionToClient, $"You need {weapon.price} gold to buy {weapon.weaponName}");
        }
    }

    private void OnTriggerStay(Collider other) {
        if (!other.gameObject.CompareTag("Player")) return;
        PlayerController player = other.GetComponent<PlayerController>();

        // TODO: Make it so it's not enabling the UI every frame for better performance. We only want to change the UI once the player has sufficient gold.
        if (player.gold >= weapon.price) {
            UIManager.Instance.TargetInteractUI(player.GetComponent<NetworkIdentity>().connectionToClient, $"Press E to buy {weapon.weaponName} for {weapon.price} gold");
        }

        if (!player.isInteracting || player.gold < weapon.price || other.GetComponent<WeaponController>().HasWeapon(weapon.weaponName)) return;

        AddWeapon(player);

        UIManager.Instance.TargetDisableInteractUI(player.GetComponent<NetworkIdentity>().connectionToClient);
        // TODO: Switch this to an ammo shop here once ammo is implemented because you can only have one of each weapon.
    }

    private void OnTriggerExit(Collider other) {
        if (!other.gameObject.CompareTag("Player")) return;
        PlayerController player = other.GetComponent<PlayerController>();

        UIManager.Instance.TargetDisableInteractUI(player.GetComponent<NetworkIdentity>().connectionToClient);
    }

    // Adds the weapon given to the player's inventory. If inventory is full, replaces the weapon the player is currently holding.
    private void AddWeapon(PlayerController player) {
        WeaponController weaponControlller = player.GetComponent<WeaponController>();

        GameObject addedWeapon = null;
        foreach (Transform weaponTrans in weaponControlller.weaponHolder.transform) {
            if (weaponTrans.GetComponent<Weapon>().weaponName == weaponPrefab.GetComponent<Weapon>().weaponName) {
                addedWeapon = weaponTrans.gameObject;
            }
        }
        if (addedWeapon == null) Debug.LogError($"Failed to find the {weaponPrefab.GetComponent<Weapon>().weaponName} weapon in {player.playerName}'s weaponHolder while trying to add.", transform);

        // If there is an empty slot, auto switches hand slot to newly added weapon. Should it do this?
        for (int i = 0; i < weaponControlller.weapons.Length; i++) {
            if (weaponControlller.weapons[i] == null) {
                player.gold -= weapon.price;
                UIManager.Instance.TargetGoldUI(player.GetComponent<NetworkIdentity>().connectionToClient, player.gold);

                weaponControlller.weapons[i] = addedWeapon;
                weaponControlller.SwitchWeapon(i);
                UIManager.Instance.TargetChangeWeaponNameUI(player.GetComponent<NetworkIdentity>().connectionToClient, i, weapon.weaponName);
                return;
            }
        }

        // TODO: Clean this up to not repeat these lines or have to go through all the above process just to fail at this simple if statement.
        if (weaponPrefab.GetComponent<Weapon>().GetType() == weaponControlller.weapons[weaponControlller.holdingWeapon].GetComponent<Weapon>().GetType()) {
            player.gold -= weapon.price;
            UIManager.Instance.TargetGoldUI(player.GetComponent<NetworkIdentity>().connectionToClient, player.gold);

            weaponControlller.weapons[weaponControlller.holdingWeapon] = addedWeapon;
            weaponControlller.SwitchWeapon(weaponControlller.holdingWeapon);
            UIManager.Instance.TargetChangeWeaponNameUI(player.GetComponent<NetworkIdentity>().connectionToClient, weaponControlller.holdingWeapon, weapon.weaponName);
        }
    }
}
