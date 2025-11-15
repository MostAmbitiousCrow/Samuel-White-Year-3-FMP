using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Tsunami_Shadow_Controller : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] DecalProjector _decalProjector;

    [SerializeField] Material _material;
    [SerializeField] MeshRenderer _mesh;
    float _progress = 0f;

    private void Awake()
    {
        _progress = 0f;
        _mesh.material = new(_mesh.material);
    }

    public void UpdateShadow(bool reversed = false)
    {
        int d = reversed ? -1 : 1;
        _progress += Time.deltaTime * d * GameManager.GameLogic.GamePauseInt;

        _material.color = Color.Lerp(Color.black, Color.clear, _progress);
    }

    public void ResetShadow()
    {
        _material.color = Color.clear;
        _progress = 0f;
    }
}
