using UnityEngine;

public class RepairBarrier : MonoBehaviour {
    private BarrierController barrierController;

    private void Start() {
        barrierController = transform.parent.GetComponent<BarrierController>();
    }

    // Enable the interact UI when player walks into the barrier trigger.
    private void OnTriggerEnter(Collider other) {
        if (!other.gameObject.CompareTag("Player")) return;
        if (barrierController.activeBarriers == barrierController.barriers.Count) return;

        barrierController.ToggleBarrierUI(other.GetComponent<PlayerController>(), true);
    }

    // Check if player is in proximity to repair and if there are any barriers to be repaired. If so, start repairing.
    private void OnTriggerStay(Collider other) {
        if (!other.gameObject.CompareTag("Player")) return;
        if (barrierController.activeBarriers == barrierController.barriers.Count) return;

        barrierController.RepairBarrier(other.GetComponent<PlayerController>());
    }

    // Disable the interact UI when player walks out of the barrier trigger.
    private void OnTriggerExit(Collider other) {
        if (!other.gameObject.CompareTag("Player")) return;
        if (barrierController.activeBarriers == barrierController.barriers.Count) return;

        barrierController.ToggleBarrierUI(other.GetComponent<PlayerController>(), false);
    }
}
