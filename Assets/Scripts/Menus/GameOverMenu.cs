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
    
    private GameManager.MainGameLogic.GameEnded _playAnimationEvent;
    
    private void Start()
    {
        _canvas = GetComponent<Canvas>();
        _canvas.worldCamera = Camera.main;
        _animation = GetComponent<Animation>();
        _canvasGroup = GetComponentInChildren(typeof(CanvasGroup), true) as CanvasGroup;
        _button = GetComponentInChildren(typeof(Button), true) as Button;

        _playAnimationEvent += () => _animation.Play();
        GameManager.GameLogic.OnGameEnded += _playAnimationEvent;
    }

    // private void OnEnable()
    // {
    //     GameManager.GameLogic.OnGameEnded += _playAnimationEvent;
    // }

    private void OnDisable()
    {
        GameManager.GameLogic.OnGameEnded -= _playAnimationEvent;
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

        GameManager.Instance.CurrentEventSystem.
            SetSelectedGameObject(_button.gameObject);
    }
}
