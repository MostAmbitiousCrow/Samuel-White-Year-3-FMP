using EditorAttributes;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainSceneManager : MonoBehaviour
{
    public bool IsLoadingScene { get; private set; }

    [Header("Loading Screen Components")]
    [SerializeField] private Loading_Screen_Controller loadingScreenController;

    /// <summary>
    /// Delegate event for whenever the Main Scene Manager has successfully loaded a scene
    /// </summary>
    public delegate void OnLevelLoaded();
    public OnLevelLoaded onLevelLoaded;

    public enum GameScenes
    {
        MainMenu = 0, MainGame = 1, DemoCompleteScreen = 2
    }

    [SerializeField] GameScenes currentScene = GameScenes.MainMenu;
    public GameScenes CurrentScene => currentScene;
    
    /// <summary>Provide an ID corresponding to the GameScenes enum values</summary>
    /// <param name="sceneID"> The ID corresponding to the GameScene enum values</param>
    public void LoadScene(int sceneID)
    {
        LoadScene((GameScenes)sceneID, LoadSceneMode.Single);
    }

    public void LoadScene(GameScenes scene, LoadSceneMode mode = LoadSceneMode.Single)
    {
        // Load game scene logic here
        Menu_Transition_Controller.ResetTransitionEvents(); // Reset events to prevent stacking
        StartCoroutine(LoadSceneRoutine((int)scene, mode));
    }

    public void ReloadScene(GameScenes scene, LoadSceneMode mode = LoadSceneMode.Single)
    {
        // Reload selected scene and unpause the game
        StartCoroutine(LoadSceneRoutine((int)scene, mode));
        GameManager.GameLogic.SetPauseState(false);
    }

    // Loading Information
    private AsyncOperation _async;
    /// <summary> Progress of the current scene load </summary>
    public float Progress { get; private set; }

    // ReSharper disable Unity.PerformanceAnalysis
    private IEnumerator LoadSceneRoutine(int scene, LoadSceneMode sceneMode)
    {
        _async = SceneManager.LoadSceneAsync(scene, sceneMode);
        if (_async != null)
        {
            _async.allowSceneActivation = false;
            IsLoadingScene = true;

            // Begin the opening Loading Transition
            loadingScreenController.StartLoadingScreen();

            Debug.Log($"Loading Screen Controller is Transitioning: {loadingScreenController.IsTransitioning}");
            // Wait until the opening loading screen is finished
            yield return new WaitUntil(() => !loadingScreenController.IsTransitioning);
            Debug.Log($"Loading Screen Controller is Transitioning: {loadingScreenController.IsTransitioning}");

            // Update Loading Screen Progress and wait until async loading is completed
            while (_async.progress < .9f)
            {
                Progress = _async.progress;
                loadingScreenController.UpdateLoadingMeter(Progress);
                yield return null;
            }

            // Wait until the level is completed
            var prog = new WaitUntil(() => _async.progress >= .9f);
            yield return prog;

            loadingScreenController.UpdateLoadingMeter(1f);
            _async.allowSceneActivation = true;

            yield return new WaitUntil(() => _async.isDone);
        }
        else Debug.LogWarning($"Failed to load Scene: {scene}");

        // End the opening Loading transition
        loadingScreenController.EndLoadingScreen();
        IsLoadingScene = false;
        currentScene = (GameScenes)scene;

        onLevelLoaded?.Invoke();
        Debug.Log("Level Loaded");

        if (currentScene == GameScenes.MainGame)
        {
            // Unpause Game
            GameManager.GameLogic.SetPauseState(false);
            yield return new WaitForSecondsRealtime(1f); // TODO: Polish to have the game properly initialised
            GameManager.GameLogic.InitialiseGame();
        }

        yield break;
    }
}
