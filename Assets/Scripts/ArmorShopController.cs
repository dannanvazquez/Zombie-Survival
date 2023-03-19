using Mirror;
using TMPro;
using UnityEngine;

public class ArmorShopController : NetworkBehaviour
{
    [SerializeField] private GameObject armorPrefab;
    private Armor armor;

    [SerializeField] private TextMeshProUGUI infoText;

    private void Awake() {
        armor = armorPrefab.GetComponent<Armor>();
        infoText.text = $"{armor.armorName}\n{armor.price}";
    }

    private void OnTriggerEnter(Collider other) {
        if (!other.gameObject.CompareTag("Player") || !isServer || !other.GetComponent<Health>().IsArmorUpgrade(armor)) return;
        PlayerController player = other.GetComponent<PlayerController>();

        if (player.gold >= armor.price) {
            UIManager.Instance.TargetInteractUI(player.GetComponent<NetworkIdentity>().connectionToClient, $"Press E to buy {armor.armorName} for {armor.price} gold");
        } else {
            UIManager.Instance.TargetInteractUI(player.GetComponent<NetworkIdentity>().connectionToClient, $"You need {armor.price} gold to buy {armor.armorName}");
        }
    }

    private void OnTriggerStay(Collider other) {
        if (!other.gameObject.CompareTag("Player") || !isServer || !other.GetComponent<Health>().IsArmorUpgrade(armor)) return;
        PlayerController player = other.GetComponent<PlayerController>();

        // TODO: Make it so it's not enabling the UI every frame for better performance. We only want to change the UI once the player has sufficient gold.
        if (player.gold >= armor.price) {
            UIManager.Instance.TargetInteractUI(player.GetComponent<NetworkIdentity>().connectionToClient, $"Press E to buy {armor.armorName} for {armor.price} gold");
        }

        if (!player.isInteracting || player.gold < armor.price) return;

        player.GetComponent<Health>().AddArmor(armor);

        UIManager.Instance.TargetDisableInteractUI(player.GetComponent<NetworkIdentity>().connectionToClient);
    }

    private void OnTriggerExit(Collider other) {
        if (!other.gameObject.CompareTag("Player") || !isServer) return;
        PlayerController player = other.GetComponent<PlayerController>();

        UIManager.Instance.TargetDisableInteractUI(player.GetComponent<NetworkIdentity>().connectionToClient);
    }
}
