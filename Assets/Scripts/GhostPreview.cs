using UnityEngine;

public class GhostPreview : MonoBehaviour
{
    public Material ghostMaterial;
    public float castDistance = 25f;

    private GameObject _ghost;
    private LayerMask _envMask;

    void Awake()
    {
        _envMask = ~LayerMask.GetMask("Interactable", "Player");
    }

    public void StartPreview(GameObject source)
    {
        if (ghostMaterial == null) return;
        MeshFilter mf = source.GetComponentInChildren<MeshFilter>();
        if (mf == null) return;

        _ghost = new GameObject("_GhostPreview");
        _ghost.AddComponent<MeshFilter>().sharedMesh = mf.sharedMesh;
        _ghost.AddComponent<MeshRenderer>().sharedMaterial = ghostMaterial;
        _ghost.transform.localScale = source.transform.lossyScale;
    }

    public void UpdatePreview(GameObject source)
    {
        if (_ghost == null) return;

        if (Physics.Raycast(source.transform.position, Vector3.down,
            out RaycastHit hit, castDistance, _envMask))
        {
            _ghost.SetActive(true);
            _ghost.transform.position = hit.point + hit.normal * 0.01f;
            _ghost.transform.rotation = source.transform.rotation;
        }
        else
        {
            _ghost.SetActive(false);
        }
    }

    public void StopPreview()
    {
        if (_ghost != null) Destroy(_ghost);
        _ghost = null;
    }
}