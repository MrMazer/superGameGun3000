using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    public Color grabColor = new Color(0.3f, 0.7f, 1f);
    public float grabIntensity = 2.5f;

    private Renderer[] _renderers;
    private MaterialPropertyBlock _mpb;
    private HighlightPulse _pulse;

    void Awake()
    {
        _mpb = new MaterialPropertyBlock();
        _renderers = GetComponentsInChildren<Renderer>();
        _pulse = GetComponent<HighlightPulse>();

        foreach (var r in _renderers)
            foreach (var mat in r.materials)
                mat.EnableKeyword("_EMISSION");
    }

    public void OnGrab()
    {
        _pulse?.SetHighlight(false);
        SetEmission(grabColor * grabIntensity);
    }

    public void OnRelease()
    {
        SetEmission(Color.black);
    }

    void SetEmission(Color color)
    {
        foreach (var r in _renderers)
        {
            r.GetPropertyBlock(_mpb);
            _mpb.SetColor("_EmissionColor", color);
            r.SetPropertyBlock(_mpb);
        }
    }
}