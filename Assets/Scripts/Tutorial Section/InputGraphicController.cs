using UnityEngine;

public class InputGraphicController : MonoBehaviour
{
    [SerializeField] private float indicateTimer = 1f;
    private GraphicUpdater[] _graphics;
    private void Awake()
    {
        _graphics = GetComponentsInChildren<GraphicUpdater>();
        _time = Time.time + indicateTimer;
    }

    private float _time;
    private void Update()
    {
        if (!(Time.time >= _time)) return;
        
        _time = Time.time + indicateTimer;
        foreach (GraphicUpdater graphic in _graphics) graphic.UpdateGraphic();
    }
}
