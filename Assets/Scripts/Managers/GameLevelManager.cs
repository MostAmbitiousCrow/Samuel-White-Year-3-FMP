using System.Collections;
using UnityEngine;

/*
 * The Game Level Manager exists in the Main Game scene and stores Scriptable Object Level Data.
 * The Current Level Data is then provided to the Section Manager to load the level content.
 */
public class GameLevelManager : MonoBehaviour
{
    // Previously the GameSectionManager
    private LevelSectionManager _sectionManager;
    
    #region Initialisation
    private void Awake()
    {
        _sectionManager = FindFirstObjectByType<LevelSectionManager>();
        if (!_sectionManager) Debug.LogError("No Game Section Manager was found!");
        
        // GameManager.GameLogic.OnGameStarted += InitialiseFirstLevel;
        LevelCount = levels.Length;

        _loadingScreenController = GameManager.SceneManager.LoadingScreenController;
    }

    private void OnEnable()
    {
        GameManager.GameLogic.OnGameStarted += InitialiseFirstLevel;
    }

    private void OnDisable()
    {
        GameManager.GameLogic.OnGameStarted -= InitialiseFirstLevel;
    }

    #endregion
    
    #region Level Loading
    // Level Information
    /// <summary> The current active level </summary>
    public int CurrentLevel => currentLevel;
    [SerializeField] private int currentLevel;

    /// <summary> The count of the amount of levels that exist </summary>
    public int LevelCount { get; private set; }
    
    [SerializeField] private SO_LevelData[] levels;
    public SO_LevelData[] Levels => levels;

    // Events
    public delegate void LevelLoaded();
    public static LevelLoaded OnLevelLoaded;

    public bool GameCompleted { get; private set; }

    // TODO: Temporary until I find out what's triggering the InitialiseFirstLevel method on Game Started
    private bool _levelInitialised;
    private void InitialiseFirstLevel()
    {
        if (_levelInitialised) return;
        LoadLevel(0);
        _levelInitialised = true;
        Debug.Log("First Level Loaded");
    }

    /// <summary> Loads a specified level </summary>
    public void LoadLevel(int level)
    {
        Debug.Log("Attempting to Load Level: " + level);
        if (GameCompleted)
        {
            Debug.Log("Game Already Completed. Rejected Level Load"); //TODO: Temp
            return;
        }
        if (level < 0) { Debug.LogWarning("Unable to load level less than 0."); return; }

        // Complete the game once all levels have been completed
        Debug.Log($"Current Level Condition: {currentLevel} ({level}) => {LevelCount}");
        if (level >= LevelCount)
        {
            GameManager.GameLogic.CompleteGame();
            GameCompleted = true;
            Debug.Log("All Levels Completed. Triggered Game Completion");
        }
        else if (!GameCompleted)
        {
            // Assign the new current level ID
            currentLevel = level;
            StartCoroutine(LevelTransitionRoutine());
        }
    }
    
    /// <summary> Loads the previous level based on the current level </summary>
    public void LoadPreviousLevel()
    {
        if (GameCompleted) return;
        var cl = currentLevel - 1;
        if (cl < 0) {currentLevel = 0; return;}
        LoadLevel(cl);
    }

    /// <summary> Loads the next level based on the current level </summary>
    public void LoadNextLevel()
    {
        if (GameCompleted) return;
        var cl = currentLevel + 1;
        LoadLevel(cl);
    }
    
    private Loading_Screen_Controller _loadingScreenController;

    private IEnumerator LevelTransitionRoutine()
    {
        _loadingScreenController.StartLoadingScreen();
        yield return new WaitUntil(() => !_loadingScreenController.IsTransitioning);
            
        // Assign the new level data to the section manager
        _sectionManager.AssignNewLevelData(levels[currentLevel]);
            
        // Update the world Spline 
        if (!levels[currentLevel]) Debug.LogError("No level assigned!");
        
        var spline = levels[currentLevel].levelSpline;
        if (spline.Count > 0) River_Manager.Instance.UpdateWorldSpline(spline);
        OnLevelLoaded?.Invoke();
        Debug.Log($"Loaded Level '{levels[currentLevel].levelName}'");

        _loadingScreenController.EndLoadingScreen();
    }
    
    #endregion
}

public enum Environments
{
    Sewer, Pyramid, Cave, Dungeon
}