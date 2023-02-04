using TMPro;
using UnityEngine;

public class DoorTriggerHandler : MonoBehaviour {
    [SerializeField] private DoorController doorController;
    [SerializeField] private RoomScriptableObject roomData;
    [SerializeField] private TextMeshProUGUI infoText;

    private void Awake() {
        infoText.text = $"{roomData.roomName}\n{roomData.openDoorPrice} Gold";
    }

    private void OnTriggerEnter(Collider other) {
        if (!other.gameObject.CompareTag("Player")) return;
        PlayerController player = other.GetComponent<PlayerController>();
        doorController.EnableDoorUI(player, roomData);
    }

    private void OnTriggerStay(Collider other) {
        if (!other.gameObject.CompareTag("Player")) return;
        PlayerController player = other.GetComponent<PlayerController>();
        
        doorController.EnableDoorUI(player, roomData);  // TODO: Make it so it's not enabling the UI every frame for better performance. We only want to change the UI once the player has sufficient gold.
        doorController.BuyDoor(player, roomData);
    }

    private void OnTriggerExit(Collider other) {
        if (!other.gameObject.CompareTag("Player")) return;
        PlayerController player = other.GetComponent<PlayerController>();

        doorController.DisableDoorUI(player, roomData);
    }
}
