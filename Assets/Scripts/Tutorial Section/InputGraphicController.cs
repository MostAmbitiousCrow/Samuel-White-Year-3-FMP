using UnityEngine;

public class InputGraphicController : MonoBehaviour
{
    [SerializeField] private float indicateTimer = 1f;
    private GraphicUpdater[] _graphics;
    private void Awake()
    {
        _graphics = GetComponentsInChildren<GraphicUpdater>(true);
        _time = Time.time + indicateTimer;
    }

    private float _time;
    private void Update()
    {
        _time += Time.deltaTime;
        
        if (!(_time > 2f)) return;
        _time = Mathf.Repeat(Time.timeScale, 2f);
        foreach (GraphicUpdater graphic in _graphics) graphic.UpdateGraphic();

    }

}
