using CameraShake;
using UnityEngine;
using UnityEngine.UI;

public class GameOverMenu : MonoBehaviour
{
    [SerializeField] private PerlinShake.Params shakeParams;
    
    private CanvasGroup _canvasGroup;
    private Animation _animation;
    private Canvas _canvas;
    private Button _button;
    
    private void Start()
    {
        _canvas = GetComponent<Canvas>();
        _canvas.worldCamera = Camera.main;
        _animation = GetComponent<Animation>();
        _canvasGroup = GetComponentInChildren(typeof(CanvasGroup), true) as CanvasGroup;
        _button = GetComponentInChildren(typeof(Button), true) as Button;

        GameManager.MainGameLogic.OnGameOver += OnGameOver;
    }

    private void OnDisable()
    {
        GameManager.MainGameLogic.OnGameOver -= OnGameOver;
    }

    public void ReturnToMenu()
    {
        // Load to Main Menu Scene
        GameManager.SceneManager.LoadScene(MainSceneManager.GameScenes.MainMenu);
    }

    public void RetryGame()
    {
        GameManager.SceneManager.LoadScene(MainSceneManager.GameScenes.MainGame);
    }

    public void DoEffects()
    {
        CameraShaker.Presets.Explosion3D(); // TODO: Add a preset for art explosions
        CameraShaker.Shake(new PerlinShake(shakeParams));

        GameManager.Instance.CurrentEventSystem.
            SetSelectedGameObject(_button.gameObject);
    }

    private void OnGameOver(GameManager.MainGameLogic.GameOverType gameOverType)
    {
        _animation.Play();
    }
}
