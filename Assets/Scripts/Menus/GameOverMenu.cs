using System;
using System.Collections;
using System.Linq;
using CameraShake;
using CarterGames.Assets.AudioManager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverMenu : MonoBehaviour
{
    [SerializeField] private PerlinShake.Params shakeParams;
    
    private Animation _animation;
    private Canvas _canvas;
    private Button _button;
    private void Start()
    {
        _canvas = GetComponent<Canvas>();
        _canvas.worldCamera = Camera.main;
        _animation = GetComponent<Animation>();
        _button = GetComponentInChildren(typeof(Button), true) as Button;

        GameManager.MainGameLogic.OnGameOver += OnGameOver;
        
        // Disable Menu
        transform.GetChild(0).gameObject.SetActive(false);
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
        // Hide Content
        foreach (var content in contents) content.canvasGroup.alpha = 0f;
        contentBackground.color = Color.clear;
        buttonsGroup.alpha = 0f;
        buttonsGroup.interactable = false;
        GameManager.GameLogic.CanPauseGame = false;
        StartCoroutine(RevealResultsRoutine());
    }

    [Header("Results")]
    [SerializeField] private Image contentBackground;
    [SerializeField] private CanvasGroup buttonsGroup;
    [SerializeField] private ContentData[] contents = new ContentData[7];
    [Serializable]
    private struct ContentData { public TextMeshProUGUI context; public CanvasGroup canvasGroup; }

    [SerializeField] private float timer = 1.25f;
    [Space]
    [SerializeField] private AudioSource soundScoreLoop;
    [SerializeField] private AudioSource soundScoreCompleted;
    
    private IEnumerator RevealScore(int index, int target, Func<int, string> textFormatter = null)
    {
        contents[index].canvasGroup.alpha = 1f;

        soundScoreLoop.Play();

        if (target > 0)
        {
            float elapsed = 0f;

            while (elapsed < timer)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / timer);
                int current = Mathf.RoundToInt(Mathf.Lerp(0, target, t));
                contents[index].context.SetText(textFormatter?.Invoke(current) ?? current.ToString());

                yield return null;
            }
        }
        else contents[index].context.SetText(textFormatter?.Invoke(0) ?? "0");

        soundScoreLoop.Stop();
        soundScoreCompleted.Play();

        yield return new WaitForSeconds(.5f);
    }
    
    private IEnumerator RevealResultsRoutine()
    {
        _animation.Play();

        yield return new WaitUntil(() => !_animation.isPlaying);
        yield return new WaitForSeconds(2.5f);
        
        contentBackground.color = new Color(1f, 1f, 1f, .8f);

        // Environment name
        contents[0].canvasGroup.alpha = 1f;
        contents[0].context.SetText(GameLevelManager.CurrentEnvironment.ToString());

        // Distance travelled
        yield return RevealScore(1, Mathf.RoundToInt(
                River_Manager.Instance.BoatController.RiverSplineObject.GlobalDistanceTravelled),
            current => $"{current}m");

        // Gems
        yield return RevealScore(2, GameManager.GameLogic.playerData.CurrentGemstones);

        // Enemies defeated
        yield return RevealScore(3, GameManager.GameLogic.playerData.EnemiesDefeated);

        // Levels beaten
        yield return RevealScore(4, GameLevelManager.LevelsCompleted);

        // Environments completed
        int completed = GameLevelManager.CountEnvironmentsCompleted();

        yield return RevealScore(5, completed,
            current => $"{current}/{GameLevelManager.EnvironmentCompletions.Count}");

        // Final score
        yield return RevealScore(6, GameManager.GameLogic.CalculateFinalScore());

        soundScoreLoop.Stop();

        yield return new WaitForSeconds(1.75f);
        
        buttonsGroup.alpha = 1f;
        buttonsGroup.interactable = true;
    }
}
