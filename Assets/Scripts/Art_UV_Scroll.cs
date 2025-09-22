using UnityEngine;

public class Art_UV_Scroll : MonoBehaviour, IAffectedByRiver
{
    [SerializeField] Material scrollingMaterial;
    [SerializeField] Vector2 scrollDirection = Vector2.right;
    [SerializeField] bool _paused = true;

    #region FrameRateManager subscription
    void OnEnable()
    {
        scrollingMaterial.mainTextureOffset = new();
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
        if (_paused) return;
        
        float X = scrollDirection.x * riverManager.RiverSpeed * Time.time;
        float Y = scrollDirection.y * riverManager.RiverSpeed * Time.time;

        scrollingMaterial.mainTextureOffset = new(X, Y); // Note: if the UV is moving too quickly, it's because the art has been scaled
        //TODO: prevent the offset from getting too high
    }

    #region Injection
    private River_Manager riverManager;
    public void InjectRiverManager(River_Manager manager)
    {
        riverManager = manager;
    }
    #endregion
}
