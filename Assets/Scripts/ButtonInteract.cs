using UnityEngine;

public class ButtonInteract : MonoBehaviour
{
    public GateController gate;

    [Header("Visual")]
    public Color pressedColor = Color.green;

    private bool _pressed;
    private HighlightPulse _highlight;
    private Renderer _renderer;
    private MaterialPropertyBlock _mpb;

    public bool IsPressed => _pressed;

    void Start()
    {
        _highlight = GetComponent<HighlightPulse>();
        _renderer = GetComponentInChildren<Renderer>();
        _mpb = new MaterialPropertyBlock();

        if (_renderer != null)
            foreach (var mat in _renderer.materials)
                mat.EnableKeyword("_EMISSION");
    }

    public void Press()
    {
        if (_pressed) return;
        _pressed = true;

        _highlight?.SetHighlight(false);

        if (_renderer != null)
        {
            _renderer.GetPropertyBlock(_mpb);
            _mpb.SetColor("_EmissionColor", pressedColor * 2f);
            _renderer.SetPropertyBlock(_mpb);
        }

        gate?.Open();
        LevelManager.Instance?.OnGateOpened();
    }
}