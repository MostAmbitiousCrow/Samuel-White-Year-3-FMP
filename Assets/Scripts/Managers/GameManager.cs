using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public SceneLoading SceneLoader { get; private set; }
    public MainGameLogic GameLogic { get; private set; }

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        
    }

    #region Scene Loading
    public class SceneLoading
    {
        public bool IsLoadingScene;

        public enum GameScenes
        {
            MainMenu, MainGame
        }
        public GameScenes CurrentScene;

        public void LoadScene(GameScenes scene)
        {
            // Load game scene logic here
        }

        public void ReloadScene(GameScenes scene)
        {
            // Reload selected scene
        }

    }
    #endregion

    #region Main Game Logic
    public class MainGameLogic
    {
        public bool GamePauseState { get; private set; }

        public delegate void OnGamePause();
        public OnGamePause onGamePause;

        public delegate void OnGameResume();
        public OnGameResume onGameResume;

        public void SetPauseState(bool state)
        {
            // Pause game logic here
            print($"Game Pause State = {GamePauseState = state}");

            if (state) onGamePause?.Invoke();
            else onGameResume?.Invoke();
        }

        public void InitialiseGame()
        {
            // Logic to initalise the main game scene before starting the game
        }

        public void StartGame()
        {
            // Logic to start the main game after it has been initialised
        }
    }
    #endregion

    #region Level Loading
    public class LevelLoading : MonoBehaviour // TODO: Convert to its own script
    {
        // Level Information
        public int CurrentLevel { get; private set; }
        public int LevelCount;

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

        IEnumerator LevelLoadRoutine()
        {
            levelAsync = SceneManager.LoadSceneAsync(0);

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
    #endregion
}
