using UnityEngine;

public class RepairBarrier : MonoBehaviour {
    private BarrierController barrierController;

    private void Start() {
        barrierController = transform.parent.GetComponent<BarrierController>();
    }

    // Check if player is in proximity to repair and if there are any barriers to be repaired. If so, start repairing.
    private void OnTriggerStay(Collider collision) {
        if (!collision.gameObject.CompareTag("Player")) return;
        if (barrierController.activeBarriers == barrierController.barriers.Count) return;

        barrierController.RepairBarrier(collision.GetComponent<PlayerController>());
    }
}
