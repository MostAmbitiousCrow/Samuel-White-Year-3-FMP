using CameraShake;
using UnityEngine;

public class GameOverMenu : MonoBehaviour
{
    [SerializeField] private PerlinShake.Params shakeParams;
    
    private CanvasGroup _canvasGroup;
    private Animation _animation;
    private Canvas _canvas;
    
    private void Start()
    {
        _canvas = GetComponent<Canvas>();
        _canvas.worldCamera = Camera.main;
        _canvasGroup = GetComponentInChildren(typeof(CanvasGroup)) as CanvasGroup;
        _animation = GetComponent<Animation>();
    }

    public void ReturnToMenu()
    {
        // Load to Main Menu Scene
        GameManager.SceneManager.LoadScene(MainSceneManager.GameScenes.MainMenu);
    }

    public void DoEffects()
    {
        CameraShaker.Presets.Explosion3D(); // TODO: Add a preset for art explosions
        CameraShaker.Shake(new PerlinShake(shakeParams));

    }
}
