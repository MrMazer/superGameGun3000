using UnityEngine;
using Unity.Cinemachine;

[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(LineRenderer))]
public class GravityGun : MonoBehaviour
{
    [Header("Grab")]
    public float grabRange = 20f;
    public float holdDistance = 4f;
    public float moveSpeed = 15f;
    public float throwForce = 22f;

    [Header("Aim")]
    public float aimSensitivity = 2f;
    public float maxPitch = 55f;
    public float maxYaw = 120f;

    [Header("Pivots")]
    public Transform yawPivot;
    public Transform pitchPivot;

    [Header("References")]
    public Transform gunBarrel;
    public Transform operatorPosition;
    public CinemachineCamera gunVCam;

    [Header("Barrel Direction")]
    public BarrelAxis barrelAxis = BarrelAxis.Forward_Z;

    [Header("Beam Visual")]
    public Material beamMaterial;
    public Color beamColor = new Color(0.3f, 0.7f, 1f, 1f);
    public float beamWidth = 0.08f;

    [Header("Audio (опционально)")]
    public AudioSource audioSource;
    public AudioClip grabClip;
    public AudioClip throwClip;
    public AudioClip humClip;

    [HideInInspector] public bool isActivated = false;

    public static GravityGun Instance { get; private set; }

    private PlayerController _player;
    private bool _inRange;
    private bool _mounted;
    private Transform _heldObj;
    private Rigidbody _heldRb;
    private InteractableObject _heldInteractable;
    private LineRenderer _beam;
    private float _pitch, _yaw;

    public enum BarrelAxis
    {
        Forward_Z,
        Up_Y,
        Right_X,
        Back_NegZ,
        Down_NegY,
        Left_NegX
    }

    Vector3 GetBarrelDirection()
    {
        switch (barrelAxis)
        {
            case BarrelAxis.Forward_Z: return gunBarrel.forward;
            case BarrelAxis.Up_Y: return gunBarrel.up;
            case BarrelAxis.Right_X: return gunBarrel.right;
            case BarrelAxis.Back_NegZ: return -gunBarrel.forward;
            case BarrelAxis.Down_NegY: return -gunBarrel.up;
            case BarrelAxis.Left_NegX: return -gunBarrel.right;
            default: return gunBarrel.forward;
        }
    }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        var sc = GetComponent<SphereCollider>();
        sc.isTrigger = true;
        sc.radius = 3.5f;

        _player = FindObjectOfType<PlayerController>();

        _beam = GetComponent<LineRenderer>();
        _beam.positionCount = 2;
        _beam.startWidth = beamWidth;
        _beam.endWidth = beamWidth * 0.3f;
        _beam.useWorldSpace = true;
        _beam.enabled = false;

        if (beamMaterial != null)
            _beam.material = beamMaterial;
        else
            _beam.material = new Material(Shader.Find("Sprites/Default"));

        _beam.startColor = beamColor;
        _beam.endColor = new Color(beamColor.r, beamColor.g, beamColor.b, 0.3f);

        if (gunVCam != null) gunVCam.Priority = 0;
    }


    void OnTriggerEnter(Collider other)
    {
        if (!isActivated || !other.CompareTag("Player")) return;
        _inRange = true;
        UIManager.Instance?.ShowPrompt(true, "[E] — Использовать пушку");
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        _inRange = false;
        if (!_mounted) UIManager.Instance?.ShowPrompt(false, "");
    }


    void Update()
    {
        if (!isActivated) return;

        if (!_mounted)
        {
            if (_inRange && Input.GetKeyDown(KeyCode.E)) Mount();
            return;
        }

        HandleAim();

        if (Input.GetMouseButtonDown(0))
        {
            if (_heldObj == null) TryGrab();
            else Throw();
        }

        if (Input.GetMouseButtonDown(1) && _heldObj != null)
            Release();

        holdDistance += Input.GetAxis("Mouse ScrollWheel") * 2f;
        holdDistance = Mathf.Clamp(holdDistance, 1.5f, 12f);

        if (Input.GetKeyDown(KeyCode.E)) Dismount();

        MoveHeldObject();
        UpdateBeam();

        Debug.DrawRay(gunBarrel.position, GetBarrelDirection() * grabRange, Color.yellow);
    }


    void Mount()
    {
        _mounted = true;
        _player.SetMounted(true);
        _player.transform.position = operatorPosition.position;
        _player.transform.rotation = operatorPosition.rotation;

        gunVCam.Priority = 20;
        _pitch = 0f;
        _yaw = 0f;

        audioSource?.PlayOneShot(humClip);
        UIManager.Instance?.ShowPrompt(true, "[ЛКМ] Захват  [ПКМ] Отпустить  [Скролл] Дистанция  [E] Выйти");
        UIManager.Instance?.ShowCrosshair(true);
    }

    void Dismount()
    {
        if (_heldObj != null) Release();

        _mounted = false;
        _player.SetMounted(false);
        gunVCam.Priority = 0;
        _beam.enabled = false;
        UIManager.Instance?.ShowCrosshair(false);
        UIManager.Instance?.ShowPrompt(false, "");
    }


    void HandleAim()
    {
        _yaw += Input.GetAxis("Mouse X") * aimSensitivity;
        _pitch -= Input.GetAxis("Mouse Y") * aimSensitivity;

        _yaw = Mathf.Clamp(_yaw, -maxYaw, maxYaw);
        _pitch = Mathf.Clamp(_pitch, -maxPitch, maxPitch);

        yawPivot.localRotation = Quaternion.Euler(0f, _yaw, 0f);
        pitchPivot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
    }


    void UpdateBeam()
    {
        if (_heldObj != null)
        {
            _beam.enabled = true;
            _beam.SetPosition(0, gunBarrel.position);
            _beam.SetPosition(1, _heldObj.position);
        }
        else
        {
            _beam.enabled = false;
        }
    }


    void TryGrab()
    {
        Vector3 dir = GetBarrelDirection();
        Ray ray = new Ray(gunBarrel.position, dir);

        if (!Physics.Raycast(ray, out RaycastHit hit, grabRange))
        {
            Debug.Log("Луч никуда не попал");
            return;
        }
        InteractableObject io = hit.collider.GetComponentInParent<InteractableObject>();
        if (io == null)
        {

            bool isInteractable = hit.collider.gameObject.layer == LayerMask.NameToLayer("Interactable");
            if (!isInteractable)
            {
                Transform check = hit.collider.transform;
                while (check.parent != null)
                {
                    check = check.parent;
                    if (check.gameObject.layer == LayerMask.NameToLayer("Interactable"))
                    { isInteractable = true; break; }
                }
            }

            if (!isInteractable)
            {
                Debug.Log($"Попал в {hit.collider.name}, layer={LayerMask.LayerToName(hit.collider.gameObject.layer)} — не Interactable");
                return;
            }
        }


        Transform target = io != null ? io.transform : hit.collider.transform;

        _heldObj = target;
        _heldInteractable = io;
        _heldRb = target.GetComponent<Rigidbody>();

        if (_heldRb != null)
        {
            _heldRb.isKinematic = true;
            _heldRb.useGravity = false;
        }

        holdDistance = Mathf.Clamp(hit.distance, 1.5f, 10f);
        _heldInteractable?.OnGrab();
        audioSource?.PlayOneShot(grabClip);

        Debug.Log($"Захвачен: {target.name}");
    }


    void MoveHeldObject()
    {
        if (_heldObj == null) return;

        Vector3 dir = GetBarrelDirection();
        Vector3 targetPos = gunBarrel.position + dir * holdDistance;

        _heldObj.position = Vector3.Lerp(_heldObj.position, targetPos, moveSpeed * Time.deltaTime);
    }


    void Throw()
    {
        if (_heldObj == null) return;

        if (_heldRb != null)
        {
            _heldRb.isKinematic = false;
            _heldRb.useGravity = true;
            _heldRb.linearVelocity = GetBarrelDirection() * throwForce;
        }

        _heldInteractable?.OnRelease();
        _beam.enabled = false;
        audioSource?.PlayOneShot(throwClip);

        var impulse = GetComponent<CinemachineImpulseSource>();
        if (impulse != null) impulse.GenerateImpulse(GetBarrelDirection() * 0.15f);

        _heldObj = null;
        _heldRb = null;
        _heldInteractable = null;
    }


    void Release()
    {
        if (_heldObj == null) return;

        if (_heldRb != null)
        {
            _heldRb.isKinematic = false;
            _heldRb.useGravity = true;
            _heldRb.linearVelocity = Vector3.zero;
        }

        _heldInteractable?.OnRelease();
        _beam.enabled = false;

        _heldObj = null;
        _heldRb = null;
        _heldInteractable = null;
    }


    public void ForceRelease()
    {
        _heldInteractable?.OnRelease();
        _beam.enabled = false;
        _heldObj = null;
        _heldRb = null;
        _heldInteractable = null;
    }
}