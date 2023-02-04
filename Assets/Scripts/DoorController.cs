using Mirror;

public class DoorController : NetworkBehaviour {
    // Toggle the interact UI for the specified client.
    public void EnableDoorUI(PlayerController player, RoomScriptableObject roomData) {
        if (player.gold >= roomData.openDoorPrice) {
            UIManager.Instance.TargetInteractUI(player.GetComponent<NetworkIdentity>().connectionToClient, $"Press E to buy door to {roomData.roomName} for {roomData.openDoorPrice} gold");
        } else {
            UIManager.Instance.TargetInteractUI(player.GetComponent<NetworkIdentity>().connectionToClient, $"You need {roomData.openDoorPrice} to open {roomData.roomName}");
        }
    }

    public void DisableDoorUI(PlayerController player, RoomScriptableObject roomData) {
        UIManager.Instance.TargetDisableInteractUI(player.GetComponent<NetworkIdentity>().connectionToClient);
    }

    // Called when a player is near the door(using OnTriggerStay). Check if the player is interacting and do so.
    public void BuyDoor(PlayerController player, RoomScriptableObject roomData) {
        if (!player.isInteracting || player.gold < roomData.openDoorPrice) return;

        player.gold -= roomData.openDoorPrice;
        UIManager.Instance.TargetDisableInteractUI(player.GetComponent<NetworkIdentity>().connectionToClient);
        GameManager.Instance.OpenRoom(roomData);
        NetworkServer.Destroy(gameObject);
    }
}
