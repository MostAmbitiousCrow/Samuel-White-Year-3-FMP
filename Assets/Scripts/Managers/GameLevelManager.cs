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
    public int CurrentLevel { get; private set; }

    /// <summary> The count of the amount of levels that exist </summary>
    public int LevelCount { get; private set; }
    
    [SerializeField] private SO_LevelData[] levels;
    public SO_LevelData[] Levels => levels;

    // Events
    public delegate void LevelLoaded();
    public static LevelLoaded OnLevelLoaded;

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
        if (level < 0) { Debug.LogWarning("Unable to load level less than 0."); return; }

        // Complete the game once all levels have been completed
        if (level > LevelCount-1) { GameManager.GameLogic.CompleteGame(); return; }

        // Assign the new current level ID
        CurrentLevel = level;

        // Assign the new level data to the section manager
        _sectionManager.AssignNewLevelData(levels[level]);
        
        // Update the world Spline 
        var spline = levels[level].levelSpline;
        if (spline.Count > 0) River_Manager.Instance.UpdateWorldSpline(spline);
        
        OnLevelLoaded?.Invoke();
        
        Debug.Log($"Loaded Level '{levels[level].levelName}'");
    }
    
    /// <summary> Loads the previous level based on the current level </summary>
    public void LoadPreviousLevel()
    {
        CurrentLevel--;
        if (CurrentLevel < 1) {CurrentLevel = 0; return;}
        LoadLevel(CurrentLevel);
    }

    /// <summary> Loads the next level based on the current level </summary>
    public void LoadNextLevel()
    {
        CurrentLevel++;
        LoadLevel(CurrentLevel);
    }
    #endregion
}