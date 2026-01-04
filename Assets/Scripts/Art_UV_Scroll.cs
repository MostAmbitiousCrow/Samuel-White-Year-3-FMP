using UnityEngine;

public class Art_UV_Scroll : MonoBehaviour
{
    [SerializeField] Material _scrollingMaterial;
    [SerializeField] Vector2 _scrollDirection = Vector2.right;
    [SerializeField] bool _paused = true;
    [SerializeField] MeshRenderer[] _meshes;

    private float X, Y;

    #region FrameRateManager subscription
    private void Awake()
    {
        if (!_scrollingMaterial) _scrollingMaterial = _meshes[0].material;

        _scrollingMaterial = new(_scrollingMaterial)
        { mainTextureOffset = new() };

        foreach (var item in _meshes)
            item.material = _scrollingMaterial;
    }
    void OnEnable()
    {
        Animation_Frame_Rate_Manager.OnTick += HandleOnTick;
    }
    void OnDisable()
    {
        Animation_Frame_Rate_Manager.OnTick -= HandleOnTick;
    }
    private void HandleOnTick(object sender, Animation_Frame_Rate_Manager.OnTickEvent tickEvent)
    {
        ScrollUV();
    }
    #endregion

    void ScrollUV()
    {
        if (_paused || !River_Manager.Instance) return;

        X = Mathf.Repeat(_scrollDirection.x * River_Manager.Instance.RiverFlowSpeed * Time.time, 1f);
        Y = Mathf.Repeat(_scrollDirection.y * River_Manager.Instance.RiverFlowSpeed * Time.time, 1f);

        _scrollingMaterial.mainTextureOffset = new(X, Y); // Note: if the UV is moving too quickly, it's because the art has been scaled
    }

    #region Injection
    //private River_Manager riverManager;
    //public void InjectRiverManager(River_Manager manager)
    //{
    //    riverManager = manager;
    //}
    #endregion
}
