using UnityEngine;

public class MeleeWeapon : Weapon {
    public override void Attack() {
        // Checks if player is looking at an object within maxDistance and set its endPos at the hit object. If there is no object in sight, end bullet at maxDistance.
        Vector3 endPos;
        LayerMask mask = ~LayerMask.GetMask("Ignore Raycast");
        mask &= ~(1 << 3);
        if (Physics.Raycast(playerController.playerCamera.gameObject.transform.position, playerController.playerCamera.gameObject.transform.TransformDirection(Vector3.forward), out RaycastHit hit, maxDistance, mask)) {
            endPos = hit.point;
            // If the hit object has health, be prepared to deal damage to it.
            if (hit.transform.GetComponent<Health>()) {
                hit.transform.GetComponent<Health>().TakeDamage(baseDamage, goldPerHit, playerController);
            }
        } else {
            playerController.playerCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, maxDistance));
        }
    }
}
