using EditorAttributes;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainSceneManager : MonoBehaviour
{
    public bool IsLoadingScene { get; private set; }

    [Header("Loading Screen Components")]
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private Loading_Screen_Controller loadingScreenController;

    /// <summary>
    /// Delegate event for whenever the Main Scene Manager has successfully loaded a scene
    /// </summary>
    public delegate void OnLevelLoaded();
    public OnLevelLoaded onLevelLoaded;

    public enum GameScenes
    {
        MainMenu = 0, MainGame = 1
    }

    [SerializeField] GameScenes currentScene = GameScenes.MainMenu;
    public GameScenes CurrentScene { get { return currentScene; } }

    [Button]
    public void LoadScene(GameScenes scene, LoadSceneMode mode = LoadSceneMode.Single)
    {
        // Load game scene logic here
        Menu_Transition_Controller.ResetEvents(); // Reset events to prevent stacking
        StartCoroutine(LoadScene((int)scene, mode));
    }

    public void ReloadScene(GameScenes scene, LoadSceneMode mode = LoadSceneMode.Additive)
    {
        // Reload selected scene
        StartCoroutine(LoadScene((int)scene, mode));
    }

    // Loading Information
    private AsyncOperation async;
    /// <summary>
    /// Progress of the current scene load
    /// </summary>
    public float Progress { get; private set; }

    private IEnumerator LoadScene(int scene, LoadSceneMode sceneMode)
    {
        async = SceneManager.LoadSceneAsync(scene, sceneMode);
        async.allowSceneActivation = false;
        IsLoadingScene = true;

        // Begin the opening Loading Transition
        loadingScreenController.StartLoadingScreen();

        Debug.Log($"Loading Screen Controller is Transitioning: {loadingScreenController.IsTransitioning}");
        // Wait until the opening loading screen is finished
        yield return new WaitUntil(() => !loadingScreenController.IsTransitioning);
        Debug.Log($"Loading Screen Controller is Transitioning: {loadingScreenController.IsTransitioning}");

        // Update Loading Screen Progress and wait until async loading is completed
        while (async.progress < .9f)
        {
            Progress = async.progress;
            loadingScreenController.UpdateLoadingMeter(Progress);
            yield return null;
        }

        // Wait until the level is completed
        var prog = new WaitUntil(() => async.progress >= .9f);
        yield return prog;

        loadingScreenController.UpdateLoadingMeter(1f);
        async.allowSceneActivation = true;

        // End the opening Loading transition
        loadingScreenController.EndLoadingScreen();
        IsLoadingScene = false;
        currentScene = (GameScenes)scene;

        onLevelLoaded?.Invoke();
        Debug.Log("Level Loaded");

        yield break;
    }
}
