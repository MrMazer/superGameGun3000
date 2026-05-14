using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpHeight = 1.4f;
    public float gravity = -18f;

    [Header("Look")]
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 85f;

    [Header("Interaction")]
    public float interactRange = 4f;
    public Transform cameraRoot;

    private CharacterController _cc;
    private Vector3 _velocity;
    private float _xRot;
    private bool _isMounted;
    private bool _carryingGun;

    public bool CarryingGun => _carryingGun;

    void Start()
    {
        _cc = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (_isMounted) return;

        HandleMovement();
        HandleLook();
        HandleInteraction();
    }

    void HandleMovement()
    {
        bool grounded = _cc.isGrounded;
        if (grounded && _velocity.y < 0f) _velocity.y = -2f;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 move = transform.right * h + transform.forward * v;
        _cc.Move(move * moveSpeed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && grounded)
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        _velocity.y += gravity * Time.deltaTime;
        _cc.Move(_velocity * Time.deltaTime);
    }

    void HandleLook()
    {
        float mx = Input.GetAxis("Mouse X") * mouseSensitivity;
        float my = Input.GetAxis("Mouse Y") * mouseSensitivity;

        _xRot -= my;
        _xRot = Mathf.Clamp(_xRot, -maxLookAngle, maxLookAngle);

        cameraRoot.localRotation = Quaternion.Euler(_xRot, 0f, 0f);
        transform.Rotate(Vector3.up * mx);
    }

    void HandleInteraction()
    {
        if (!Input.GetKeyDown(KeyCode.E)) return;

        Ray ray = new Ray(cameraRoot.position, cameraRoot.forward);
        if (!Physics.Raycast(ray, out RaycastHit hit, interactRange)) return;

   
        var pickup = hit.collider.GetComponentInParent<GunPickup>();
        if (pickup != null)
        {
            pickup.Pickup();
            _carryingGun = true;
            LevelManager.Instance?.OnGunPickedUp();
            return;
        }

        var mount = hit.collider.GetComponentInParent<GunMountPoint>();
        if (mount != null && _carryingGun && !mount.IsOccupied)
        {
            mount.InstallGun();
            _carryingGun = false;
            LevelManager.Instance?.OnGunInstalled();
            return;
        }

        var button = hit.collider.GetComponentInParent<ButtonInteract>();
        if (button != null && !button.IsPressed)
        {
            button.Press();
            return;
        }
    }

    public void SetMounted(bool mounted) => _isMounted = mounted;
}