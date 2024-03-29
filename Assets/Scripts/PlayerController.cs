using UnityEngine;
using Mirror;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : NetworkBehaviour {
    public string playerName = "Player";
    public int kills = 0;
    public int gold = 0;

    public float walkingSpeed = 7.5f;
    public float runningSpeed = 11.5f;
    public float jumpSpeed = 8.0f;

    public Camera playerCamera;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 90.0f;

    CharacterController characterController;
    Vector3 moveDirection = Vector3.zero;
    float rotationX = 0;
    [HideInInspector] public bool canMove = true;
    [HideInInspector] public bool isInteracting = false;

    private void Start() {
        characterController = GetComponent<CharacterController>();

        if (!isLocalPlayer) return;

        Camera.main.gameObject.SetActive(false);

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Disable meshes of self
        MeshRenderer[] meshes = GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer mesh in meshes) {
            // TODO: Make this look cleaner once the player model is a single mesh and more polished
            if (mesh.gameObject != this.gameObject && mesh.gameObject.transform.parent.gameObject != this.gameObject && mesh.gameObject.transform.parent.gameObject.transform.parent.gameObject == GetComponent<WeaponController>().weaponHolder) continue;  // Doesn't hide the gun model from local player
            mesh.enabled = false;
        }

        // Have only self camera enabled
        playerCamera.gameObject.SetActive(true);

        UIManager.Instance.goldText.text = $"Gold: {gold}";
    }

    private void Update() {
        if (!isLocalPlayer) return;

        // Press escape key to "pause" and "unpause" the game
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (canMove) {
                canMove = false;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            } else {
                canMove = true;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        // Check if player is trying to interact with E key
        if (Input.GetKeyDown(KeyCode.E)) CmdIsInteracting(true);
        if (Input.GetKeyUp(KeyCode.E)) CmdIsInteracting(false);

        // We are grounded, so recalculate move direction based on axes
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        // Press Left Shift to run
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float curSpeedX = canMove ? Input.GetAxis("Vertical") : 0;
        float curSpeedY = canMove ? Input.GetAxis("Horizontal") : 0;

        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);
        moveDirection.Normalize();
        moveDirection.x *= (isRunning ? runningSpeed : walkingSpeed);
        moveDirection.z *= (isRunning ? runningSpeed : walkingSpeed);

        if (Input.GetButton("Jump") && canMove && characterController.isGrounded) {
            moveDirection.y = jumpSpeed;
        } else {
            moveDirection.y = movementDirectionY;
        }

        // Apply gravity. Gravity is multiplied by deltaTime twice (once here, and once below
        // when the moveDirection is multiplied by deltaTime). This is because gravity should be applied
        // as an acceleration (ms^-2)
        if (!characterController.isGrounded) {
            moveDirection += Physics.gravity * Time.deltaTime;
        }

        // Move the controller
        if (characterController.enabled) characterController.Move(moveDirection * Time.deltaTime);

        // Player and Camera rotation
        if (canMove) {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            GetComponent<WeaponController>().weaponHolder.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }
    }

    // Tell the server when the player is trying to interact
    [Command]
    private void CmdIsInteracting(bool interacting) {
        if (interacting) isInteracting = true;
        else isInteracting = false;
    }

    // Let all clients know if the player can now move or no longer move.
    [ClientRpc]
    public void RpcCanMove(bool _canMove) {
        GetComponent<PlayerController>().canMove = _canMove;
        GetComponent<CharacterController>().enabled = _canMove;
    }
}