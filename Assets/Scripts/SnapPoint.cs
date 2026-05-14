using UnityEngine;

public class SnapPoint : MonoBehaviour
{
    [Header("Settings")]
    public float snapDistance = 2f;
    public Transform targetObject;

    [Header("Visual")]
    public GameObject ghostPreview;

    private bool _snapped = false;
    private bool _active = false;
    private HighlightPulse _highlight;

    public bool IsSnapped => _snapped;

    void Start()
    {
        _highlight = GetComponent<HighlightPulse>();
        _highlight?.SetHighlight(false);

        if (ghostPreview != null)
            ghostPreview.SetActive(false);
    }

    public void Activate()
    {
        _active = true;
        _highlight?.SetHighlight(true);

        if (ghostPreview != null)
            ghostPreview.SetActive(true);
    }

    void Update()
    {
        if (_snapped || !_active || targetObject == null) return;

        float dist = Vector3.Distance(targetObject.position, transform.position);

        if (dist < snapDistance)
        {
            Snap();
        }
    }

    void Snap()
    {
        _snapped = true;
        _active = false;

        GravityGun.Instance?.ForceRelease();

        targetObject.position = transform.position;
        targetObject.rotation = transform.rotation;

        var rb = targetObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        var boxCol = targetObject.GetComponent<BoxCollider>();
        if (boxCol != null) boxCol.enabled = false;

        _highlight?.SetHighlight(false);
        if (ghostPreview != null)
            ghostPreview.SetActive(false);

        LevelManager.Instance?.OnLadderPlaced();
    }
}