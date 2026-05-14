using UnityEngine;
public class GateController : MonoBehaviour
{
    [Header("Open Settings")]
    public float openAngle = 90f;    
    public float openSpeed = 40f;       

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip openClip;

    private bool _opening;
    private float _currentAngle;
    private Quaternion _startRotation;

    void Start()
    {
        _startRotation = transform.localRotation;
    }

    public void Open()
    {
        if (_opening) return;
        _opening = true;
        audioSource?.PlayOneShot(openClip);
    }

    void Update()
    {
        if (!_opening) return;
        if (_currentAngle >= openAngle) return;

        float step = openSpeed * Time.deltaTime;
        _currentAngle += step;
        _currentAngle = Mathf.Min(_currentAngle, openAngle);

        transform.localRotation = _startRotation * Quaternion.Euler(0f, 0f, _currentAngle);
    }
}
