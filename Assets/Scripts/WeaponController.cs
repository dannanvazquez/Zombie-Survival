using System.Collections;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(PlayerController))]
public class WeaponController : NetworkBehaviour {
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform bulletSpawnTrans;
    [SerializeField] private float shootCooldown = 1f;
    [SerializeField] private float bulletSpeed = 30f;
    [SerializeField] private float maxDistance = 25f;
    [SerializeField] private int bulletDamage = 10;

    private bool canShoot = true;
    private PlayerController playerController;

    private void Start() {
        playerController = GetComponent<PlayerController>();
    }

    private void Update()
    {
        if (!isLocalPlayer) return;

        // Shoot when the player presses the Fire1 button.
        if (Input.GetButton("Fire1") && canShoot && playerController.canMove) {
            canShoot = false;
            CmdShoot();
            StartCoroutine(StartCooldown());
        }
    }

    // Player has commanded to shoot.
    // TODO: Check with the server if the player is allowed to shoot.
    [Command]
    private void CmdShoot() {
        // Checks if player is looking at an object within maxDistance and set its endPos at the hit object. If there is no object in sight, end bullet at maxDistance.
        Vector3 endPos;
        LayerMask mask = ~LayerMask.GetMask("Ignore Raycast");
        if (Physics.Raycast(playerController.playerCamera.gameObject.transform.position, playerController.playerCamera.gameObject.transform.TransformDirection(Vector3.forward), out RaycastHit hit, maxDistance, mask)) {
            endPos = hit.point;
            // If the hit object has health, be prepared to deal damage to it.
            if (hit.transform.GetComponent<Health>()) {
                StartCoroutine(ShootHitEnemy(hit));
            }
        } else {
            endPos = gameObject.GetComponent<PlayerController>().playerCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, maxDistance));
        }

        // Instantiate the bullet across the server. Starting at bulletSpawnTrans's position and move towards endPos.
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnTrans.position, bulletSpawnTrans.rotation);
        bullet.transform.LookAt(endPos);
        bullet.GetComponent<Rigidbody>().velocity = bullet.transform.forward * bulletSpeed;
        Destroy(bullet, Vector3.Distance(transform.position, endPos) / bulletSpeed);
        NetworkServer.Spawn(bullet);
    }

    // Cooldown before shooting again
    IEnumerator StartCooldown() {
        yield return new WaitForSeconds(shootCooldown);
        canShoot = true;
    }

    // Once the bullet has arrived at the enemy, deal damage to it.
    IEnumerator ShootHitEnemy(RaycastHit _hit) {
        float travelTime = Vector3.Distance(transform.position, _hit.point) / bulletSpeed;
        yield return new WaitForSeconds(travelTime);
        _hit.transform.GetComponent<Health>().TakeDamage(bulletDamage);
    }
}
