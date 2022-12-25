using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrierController : NetworkBehaviour
{
    public List<GameObject> barriers = new();
    [HideInInspector ] public int activeBarriers = 6;
    public float breakCooldown = 2f;
    private float currentCooldown = 2f;

    private void Start() {
        activeBarriers = barriers.Count;
        currentCooldown = breakCooldown;
    }

    public void DestroyBarrier() {
        currentCooldown -= Time.deltaTime;
        if (currentCooldown > 0) return;

        RpcDisableBarrier(barriers.Count - activeBarriers);
        activeBarriers--;
        currentCooldown = breakCooldown;
    }

    [ClientRpc]
    public void RpcDisableBarrier(int barrierIndex) {
        barriers[barrierIndex].SetActive(false);
    }
}
