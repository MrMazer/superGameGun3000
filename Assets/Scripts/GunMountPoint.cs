using UnityEngine;


public class GunMountPoint : MonoBehaviour
{
    [Tooltip("Объект GravityGun который появится здесь после установки")]
    public GameObject gravityGunObject;

    private HighlightPulse _highlight;
    private bool _occupied;

    public bool IsOccupied => _occupied;

    void Start()
    {
        _highlight = GetComponent<HighlightPulse>();
        _highlight?.SetHighlight(false);
    }

    public void EnableHighlight()
    {
        _highlight?.SetHighlight(true);
    }

    public void InstallGun()
    {
        _occupied = true;
        _highlight?.SetHighlight(false);

        if (gravityGunObject != null)
        {
            gravityGunObject.SetActive(true);
            var gun = gravityGunObject.GetComponent<GravityGun>();
            if (gun != null) gun.isActivated = true;
        }
    }
}
