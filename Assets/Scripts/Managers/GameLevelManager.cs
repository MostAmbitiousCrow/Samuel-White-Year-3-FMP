using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLevelManager : MonoBehaviour
{
    // Level Information
    public int CurrentLevel { get; private set; }
    public int LevelCount { get; private set; }

    [Serializable]
    public struct GameLevel
    {
        /// <summary>
        /// Array of levels to choose from
        /// </summary>
        public int[] Levels;
    }
    [SerializeField] GameLevel[] gameLevels;

    // Loading Information
    private AsyncOperation levelAsync;
    public float LevelAsyncProgress { get; private set; }

    // Events
    public delegate void OnLevelLoaded(int level);
    public OnLevelLoaded onLevelLoaded;

    /// <summary>
    /// Loads a specified level
    /// </summary>
    public void LoadLevel(int level)
    {
        if (level < 0 || level > LevelCount) { Debug.LogWarning("Unable to load level less than 0 or greater than the current level count."); return; }
        CurrentLevel = level;

        StartCoroutine(LevelLoadRoutine());
    }


    /// <summary>
    /// Loads the previous level based on the current level
    /// </summary>
    public void LoadPreviousLevel()
    {
        LoadLevel(CurrentLevel--);
    }

    /// <summary>
    /// Loads the next level based on the current level
    /// </summary>
    public void LoadNextLevel()
    {
        LoadLevel(CurrentLevel++);
    }

    /// <summary>
    /// Routine which loads the provided level
    /// </summary>
    IEnumerator LevelLoadRoutine()
    {
        levelAsync = SceneManager.LoadSceneAsync(CurrentLevel);

        // while (levelAsync.progress < .9f)
        // {
        //     LevelAsyncProgress = levelAsync.progress;

        //     yield return null;
        // }

        // if(levelAsync.isDone)
        //     onLevelLoaded?.Invoke(CurrentLevel);

        while (!levelAsync.isDone)
        {
            yield return null;
        }
        onLevelLoaded?.Invoke(CurrentLevel);

        yield break;
    }
}