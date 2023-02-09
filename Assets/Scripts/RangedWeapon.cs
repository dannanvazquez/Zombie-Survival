using Mirror;
using System.Collections;
using UnityEngine;

public class RangedWeapon : Weapon {
    public GameObject bulletPrefab;
    public Transform bulletSpawnTrans;
    public float bulletSpeed = 30f;
    public int ammoCapacity = 300;
    [HideInInspector] public int currentAmmo = 290;
    public int clipCapacity = 10;
    [HideInInspector] public int currentClip = 10;
    public int ammoPrice = 400;
    public float reloadTime = 1.5f;
    public bool isReloading = false;

    private void Awake() {
        currentAmmo = ammoCapacity - clipCapacity;
        currentClip = clipCapacity;

    }

    public override void Attack() {
        // Checks if player is looking at an object within maxDistance and set its endPos at the hit object. If there is no object in sight, end bullet at maxDistance.
        Vector3 endPos;
        LayerMask mask = ~LayerMask.GetMask("Ignore Raycast");
        mask &= ~(1 << 3);
        if (Physics.Raycast(playerController.playerCamera.gameObject.transform.position, playerController.playerCamera.gameObject.transform.TransformDirection(Vector3.forward), out RaycastHit hit, maxDistance, mask)) {
            endPos = hit.point;
            // If the hit object has health, be prepared to deal damage to it.
            if (hit.transform.GetComponent<Health>()) {
                StartCoroutine(ShootHitEnemy(hit));
            }
        } else {
            endPos = playerController.playerCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, maxDistance));
        }

        // Instantiate the bullet across the server. Starting at bulletSpawnTrans's position and move towards endPos.
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnTrans.position, bulletSpawnTrans.rotation);
        bullet.transform.LookAt(endPos);
        bullet.GetComponent<Rigidbody>().velocity = bullet.transform.forward * bulletSpeed;
        Destroy(bullet, Vector3.Distance(transform.position, endPos) / bulletSpeed);
        NetworkServer.Spawn(bullet);
    }

    // Once the bullet has arrived at the enemy, deal damage to it.
    IEnumerator ShootHitEnemy(RaycastHit _hit) {
        float travelTime = Vector3.Distance(transform.position, _hit.point) / bulletSpeed;
        yield return new WaitForSeconds(travelTime);
        if (_hit.transform != null) _hit.transform.GetComponent<Health>().TakeDamage(baseDamage, goldPerHit, playerController);
    }

    public IEnumerator Reload() {
        if (currentAmmo == 0) yield break;

        isReloading = true;
        yield return new WaitForSeconds(reloadTime);

        int reloadAmount = clipCapacity - currentClip;
        if (currentAmmo < reloadAmount) reloadAmount = currentAmmo;

        currentAmmo -= reloadAmount;
        currentClip += reloadAmount;

        UIManager.Instance.TargetUpdateAmmoUI(playerController.GetComponent<NetworkIdentity>().connectionToClient, currentClip, currentAmmo);

        isReloading = false;
    }
}
