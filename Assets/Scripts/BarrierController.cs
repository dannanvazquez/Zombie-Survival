using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class BarrierController : NetworkBehaviour {
    public List<GameObject> barriers = new();
    [HideInInspector ] public int activeBarriers = 6;
    public float breakCooldown = 2f;
    private float currentCooldown = 2f;

    private void Start() {
        activeBarriers = barriers.Count;
        currentCooldown = breakCooldown;
    }

    // Called when an enemy is near the barrier(using OnTriggerStay) and trying to destroy it.
    public void DestroyBarrier() {
        currentCooldown -= Time.deltaTime;
        if (currentCooldown > 0) return;

        RpcDisableBarrier(barriers.Count - activeBarriers);
        activeBarriers--;
        currentCooldown = breakCooldown;
    }

    // Disable the object on all clients
    [ClientRpc]
    public void RpcDisableBarrier(int barrierIndex) {
        barriers[barrierIndex].SetActive(false);
    }
}
