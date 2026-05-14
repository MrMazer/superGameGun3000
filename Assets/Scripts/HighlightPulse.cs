using UnityEngine;

public class HighlightPulse : MonoBehaviour
{
    public Color highlightColor = new Color(0.2f, 0.6f, 1f);
    public float intensity = 3f;
    public float pulseSpeed = 2f;

    private Renderer[] _renderers;
    private MaterialPropertyBlock _mpb;
    private bool _active;

    void Awake()
    {
        _mpb = new MaterialPropertyBlock();
        _renderers = GetComponentsInChildren<Renderer>();

        foreach (var r in _renderers)
            foreach (var mat in r.materials)
                mat.EnableKeyword("_EMISSION");
    }

    public void SetHighlight(bool on)
    {
        _active = on;
        if (!on) ApplyEmission(Color.black);
    }

    void Update()
    {
        if (!_active) return;

        float t = (Mathf.Sin(Time.time * pulseSpeed * Mathf.PI) + 1f) * 0.5f;
        Color emission = highlightColor * (t * intensity);
        ApplyEmission(emission);
    }

    void ApplyEmission(Color color)
    {
        foreach (var r in _renderers)
        {
            r.GetPropertyBlock(_mpb);
            _mpb.SetColor("_EmissionColor", color);
            r.SetPropertyBlock(_mpb);
        }
    }
}
