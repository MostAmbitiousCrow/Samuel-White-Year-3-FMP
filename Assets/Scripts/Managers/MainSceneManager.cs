using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainSceneManager : MonoBehaviour
{
    public bool IsLoadingScene { get; private set; }

    [Header("Loading Screen Components")]
    [SerializeField] GameObject _loadingScreen;

    public enum GameScenes
    {
        MainMenu, MainGame
    }
    public GameScenes CurrentScene;

    public void LoadScene(GameScenes scene, LoadSceneMode mode = LoadSceneMode.Additive)
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
    public float Progress { get; private set; }

    IEnumerator LoadScene(int scene, LoadSceneMode sceneMode)
    {
        async = SceneManager.LoadSceneAsync(scene, sceneMode);
        IsLoadingScene = true;

        //while (levelAsync.progress < .9f)
        //{
        //    progress = levelAsync.progress;

        //    yield return null;
        //}

        //if (levelAsync.isDone)
        //    onLevelLoaded?.Invoke(CurrentLevel);

        while (!async.isDone)
        {
            yield return null;
        }

        IsLoadingScene = false;

        yield break;
    }

}