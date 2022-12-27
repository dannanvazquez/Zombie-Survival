using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour {
    public static UIManager Instance { get; private set; }

    public RectTransform healthBarUI;
    public TextMeshProUGUI healthText;

    private void Awake() {
        // If there is an instance, and it's not me, delete myself.
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
    }
}
