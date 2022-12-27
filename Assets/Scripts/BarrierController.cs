using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class BarrierController : NetworkBehaviour {
    public List<GameObject> barriers = new();
    [HideInInspector ] public int activeBarriers = 6;
    public float breakCooldown = 2f;
    private float breakTimer = 2f;
    public float repairCooldown = 2f;
    private float repairTimer = 2f;

    private void Start() {
        activeBarriers = barriers.Count;
        breakTimer = breakCooldown;
        repairTimer = repairCooldown;
    }

    // Called when an enemy is near the barrier(using OnTriggerStay) and trying to destroy it.
    public void DestroyBarrier() {
        breakTimer -= Time.deltaTime;
        if (breakTimer > 0) return;

        RpcDisableBarrier(barriers.Count - activeBarriers);
        activeBarriers--;
        breakTimer = breakCooldown;
    }

    // Disable the barrier on all clients
    [ClientRpc]
    public void RpcDisableBarrier(int barrierIndex) {
        barriers[barrierIndex].SetActive(false);
    }

    // Called when a player is near the barrier(using OnTriggerStay). Check if the player is interacting and do so.
    public void RepairBarrier(PlayerController player) {
        if (!player.isInteracting) return;

        repairTimer -= Time.deltaTime;
        if (repairTimer > 0) return;

        RpcEnableBarrier(barriers.Count - activeBarriers - 1);
        activeBarriers++;
        repairTimer = repairCooldown;
    }

    // Enable the barrier on all clients
    [ClientRpc]
    public void RpcEnableBarrier(int barrierIndex) {
        barriers[barrierIndex].SetActive(true);
    }
}
