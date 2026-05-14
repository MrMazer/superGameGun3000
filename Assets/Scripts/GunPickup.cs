using UnityEngine;

public class GunPickup : MonoBehaviour
{
    private HighlightPulse _highlight;

    void Start()
    {
        _highlight = GetComponent<HighlightPulse>();
        _highlight?.SetHighlight(true); 
    }

    public void Pickup()
    {
        _highlight?.SetHighlight(false);
        gameObject.SetActive(false);
    }
}
