using UnityEngine;
using Mirror;
using TMPro;

public class UIManager : NetworkBehaviour {
    public static UIManager Instance { get; private set; }

    public RectTransform healthBarUI;
    public TextMeshProUGUI healthText;

    public TextMeshProUGUI goldText;

    public TextMeshProUGUI interactText;

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
}
