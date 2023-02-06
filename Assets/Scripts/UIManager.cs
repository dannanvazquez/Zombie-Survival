using UnityEngine;
using Mirror;
using TMPro;
using System.Collections.Generic;

public class UIManager : NetworkBehaviour {
    public static UIManager Instance { get; private set; }

    public RectTransform healthBarUI;
    public TextMeshProUGUI healthText;

    public TextMeshProUGUI goldText;

    public TextMeshProUGUI interactText;

    public RectTransform[] weaponsUI;
    public TextMeshProUGUI[] weaponNames;
    public TextMeshProUGUI ammoText;

    private void Awake() {
        // If there is an instance, and it's not me, delete myself.
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
    }

    // Change the client's gold amount in the UI.
    [TargetRpc]
    public void TargetGoldUI(NetworkConnection conn, int _gold) {
        goldText.text = $"Gold: {_gold}";
    }

    // Set the client's interact UI text and enable/show it.
    [TargetRpc]
    public void TargetInteractUI(NetworkConnection conn, string _text) {
        interactText.text = _text;
        interactText.gameObject.SetActive(true);
    }

    // Disable/hide the interact UI from the client.
    [TargetRpc]
    public void TargetDisableInteractUI(NetworkConnection conn) {
        interactText.gameObject.SetActive(false);
    }

    [TargetRpc]
    public void TargetChangeWeaponNameUI(NetworkConnection conn, int order, string name) {
        if (order >= weaponNames.Length || weaponNames[order] == null) Debug.LogError($"Changing UI order {order} to {name} has failed. Possibly out of bounds.", transform);

        weaponNames[order].text = name;
    }

    [TargetRpc]
    public void TargetUpdateAmmoUI(NetworkConnection conn, int ammo, int maxAmmo) {
        ammoText.text = $"{ammo}/{maxAmmo} Ammo";
        ammoText.enabled = true;
    }

    [TargetRpc]
    public void TargetDisableAmmoUI(NetworkConnection conn) {
        ammoText.enabled = false;
    }
}
