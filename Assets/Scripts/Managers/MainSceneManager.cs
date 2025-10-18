using EditorAttributes;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainSceneManager : MonoBehaviour
{
    public bool IsLoadingScene { get; private set; }

    [Header("Loading Screen Components")]
    [SerializeField] GameObject _loadingScreen;
    [SerializeField] Loading_Screen_Controller _loadingScreenController;

    //[Header("Stats")]
    /// <summary>
    /// Delegate event for whenever the Main Scene Manager has successfully loaded a scene
    /// </summary>
    public delegate void OnLevelLoaded();
    public OnLevelLoaded onLevelLoaded;

    public enum GameScenes
    {
        MainMenu, MainGame
    }
    public GameScenes CurrentScene { get; private set; }

    [Button]
    public void LoadScene(GameScenes scene, LoadSceneMode mode = LoadSceneMode.Single)
    {
        // Load game scene logic here
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

    IEnumerator LoadScene(int scene, LoadSceneMode sceneMode)
    {
        async = SceneManager.LoadSceneAsync(scene, sceneMode);
        async.allowSceneActivation = false;
        IsLoadingScene = true;

        // Begin the opening Loading Transition
        _loadingScreenController.StartLoadingScreen();

        Debug.Log($"Loading Screen Controller is Transitioning: {_loadingScreenController.IsTransitioning}");
        // Wait until the opening loading screen is finished
        yield return new WaitUntil(() => !_loadingScreenController.IsTransitioning);
        Debug.Log($"Loading Screen Controller is Transitioning: {_loadingScreenController.IsTransitioning}");

        // Update Loading Screen Progress and wait until async loading is completed
        while (async.progress < .9f)
        {
            Progress = async.progress;
            _loadingScreenController.UpdateLoadingMeter(Progress);
            yield return null;
        }

        // Wait until the level is completed
        yield return new WaitUntil(() => async.progress >= .9f);

        _loadingScreenController.UpdateLoadingMeter(1f);
        async.allowSceneActivation = true;

        // End the opening Loading transition
        _loadingScreenController.EndLoadingScreen();

        onLevelLoaded?.Invoke();

        IsLoadingScene = false;

        yield break;
    }
}