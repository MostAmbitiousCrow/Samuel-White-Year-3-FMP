using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

/*
 * The Game Level Manager exists in the Main Game scene and stores Scriptable Object Level Data.
 * The Current Level Data is then provided to the Section Manager to load the level content.
 */
/*public class GameLevelManager : MonoBehaviour
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
    
    #region Level Selection

    public static Environments[] GetAvailableEnvironments(Environments environment)
    {
        return environment switch
        {
            Environments.Sewer => new[] { Environments.Cave, Environments.Pyramid },
            Environments.Pyramid => new[] { Environments.Dungeon, Environments.Forest },
            Environments.Cave => new[] { Environments.Sewer, Environments.Forest },
            Environments.Forest => new[] { Environments.Cave, Environments.Dungeon },
            Environments.Dungeon => new[] { Environments.Sewer, Environments.Pyramid },
            _ => throw new ArgumentOutOfRangeException(nameof(environment), environment, null)
        };
    }
    
    #endregion

    #region Level Creation
    
    private Dictionary<int, SO_LevelData[]> _levels;

    public void CreateLevels()
    {
        
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
}*/

public class GameLevelManager : MonoBehaviour
{
    /// <summary>
    /// The current Difficulty value of the game.
    /// <para>Level Sections will be added into the level selection based on the current game difficulty. </para>
    /// </summary>
    public static float CurrentDifficulty { get; private set; }
    public static SO_GameDifficultyValues.DifficultyValues CurrentDifficultyValues { get; private set; }
    /// <summary> What Environment the player is currently on </summary>
    public static Environments CurrentEnvironment { get; private set; }
    
    /// <summary> What level is the player currently on in the current environment</summary>
    public static int CurrentLevel { get; private set; }
    public static int LevelsCompleted {get; private set;}
    public static SO_LevelData CurrentLevelData;
    /// <summary> Checklist of which environments the player has completed </summary>
    public static Dictionary<Environments, bool> EnvironmentCompletions { get; private set; } = new()
    {
        { Environments.Sewer, false },
        { Environments.Pyramid, false },
        { Environments.Cave, false },
        { Environments.Forest, false },
        { Environments.Dungeon, false },
    };

    public static GameLevelManager Instance { get; private set; }
    public static event Action OnLevelLoaded;
    public static event Action OnLevelCompleted;

    #region Instanced Variables
    [SerializeField] private float difficultyIncreasePerLevel = 5f;
    
    [Header("Dependencies")]
    [SerializeField] private SO_LevelsContainer levelsContainer;
    [SerializeField] private SO_GameDifficultyValues difficultyValues;
    [SerializeField] private SO_EnvironmentPaths environmentPaths;
    [SerializeField] private LevelSectionManager sectionManager;

    #endregion

    #region Initialisation

    private void OnEnable()
    {
        GameManager.GameLogic.OnGameStarted += InitialiseLevels;
    }
    
    private void OnDisable()
    {
        GameManager.GameLogic.OnGameStarted -= InitialiseLevels;
    }

    private void Awake()
    {
        if (!Instance) Instance = this;
        else { Destroy(gameObject); return; }
        
        CurrentLevel = 0;
        LevelsCompleted = 0;
        CurrentDifficulty = 0f;
            
        sectionManager = FindFirstObjectByType<LevelSectionManager>();
        if (!sectionManager) Debug.LogError("No Game Section Manager was found!");
    }

    private void Start()
    {
        _loadingScreenController = GameManager.SceneManager.LoadingScreenController;
    }
    #endregion

    #region Environment Checks
    
    /// <summary>
    /// Returns true if the player has completed every environment
    /// </summary>
    /// <returns>Boolean based on if all environments were completed</returns>
    public static bool CheckEnvironmentsCompleted()
    {
        var requirement = EnvironmentCompletions.Count;
        var progress = EnvironmentCompletions.Count(environment => environment.Value);

        return progress >= requirement;
    }

    /// <summary>
    /// Check if a specific environment has been completed
    /// </summary>
    /// <param name="environment">The environment type to check</param>
    /// <returns>Boolean based on if the given environment has been completed</returns>
    public static bool CheckEnvironmentCompleted(Environments environment)
    {
        return EnvironmentCompletions[environment];
    }

    public static int CountEnvironmentsCompleted()
    {
        return EnvironmentCompletions.Count(x => x.Value);
    }
    
    #endregion

    #region Environment and Level Loading

    private void InitialiseLevels()
    {
        LoadEnvironmentAndLevel(Environments.Sewer);
    }

    public void LoadNextLevel()
    {
        CurrentLevel++;
        LevelsCompleted++;
        // Check if all the required levels are completed
        if (CurrentLevel >= CurrentDifficultyValues.levels)
        {
            EnvironmentCompletions[CurrentEnvironment] = true;

            if (CheckEnvironmentsCompleted())
            {
                //TODO: Temp
                GameManager.GameLogic.CompleteGame();
            }
            else InitiateEnvironmentSelect();
            return;
        }

        OnLevelCompleted?.Invoke();

        UpdateDifficulty();
        
        StartCoroutine(LevelTransitionRoutine());
        Debug.Log($"Loaded Level '{CurrentLevel}' of {CurrentDifficultyValues.levels}. Difficulty = {CurrentDifficulty}.");
    }

    public void LoadEnvironmentAndLevel(Environments environment)
    {
        CurrentEnvironment = environment;
        CurrentLevel = 0;
        
        UpdateDifficulty();
        StartCoroutine(LevelTransitionRoutine());
        
        Debug.Log($"Loaded Environment '{CurrentEnvironment}'. Difficulty = {CurrentDifficulty}. Current Level = {CurrentLevel}. Max Levels = {CurrentDifficultyValues.levels}");
    }

    private void UpdateDifficulty()
    {
        CurrentDifficulty += difficultyIncreasePerLevel;
        
        if (CurrentDifficulty >= CurrentDifficultyValues.threshold.y)
            CurrentDifficultyValues = CalculateDifficulty();
    }

    private void InitiateEnvironmentSelect()
    {
        Game_UI.Instance.OpenEnvironmentSelect(CurrentEnvironment);
    }

    /// <summary> Creates a level under the current environment </summary>
    private SO_LevelData CreateLevel()
    {
        List<SO_SectionData> selectedSections = new();
        int sectionCount = Mathf.RoundToInt(Mathf.Lerp(CurrentDifficultyValues.sectionsRange.x,
            CurrentDifficultyValues.sectionsRange.y, CurrentDifficulty / difficultyValues.maxDifficulty));
        
        Debug.Log($"Creating {sectionCount} sections for {CurrentEnvironment}");

        for (int i = 0; i < sectionCount; i++)
        {
            SO_SectionData section = levelsContainer.GetRandomSection
                (CurrentEnvironment, CurrentDifficultyValues.difficulty);
            if (!section) continue;
            
            selectedSections.Add(section);
        }

        // Get a level template from the current environment
        SO_LevelData template = levelsContainer.GetRandomLevel(CurrentEnvironment);

        if (!template) { Debug.LogError($"No level template found for {CurrentEnvironment}"); return null; }

        // Clone it so we don't modify the original asset
        SO_LevelData level = Instantiate(template);

        // Replace its sections with our generated ones
        level.sectionData = selectedSections.ToArray();
        
        return level;
    }
    
    private Loading_Screen_Controller _loadingScreenController;

    private IEnumerator LevelTransitionRoutine()
    {
        _loadingScreenController.StartLoadingScreen();
        var waitForFrame = new WaitForEndOfFrame();

        yield return new WaitUntil(() => !Loading_Screen_Controller.IsOpening);
        
        CurrentLevelData = CreateLevel();

        sectionManager.AssignNewLevelData(CurrentLevelData);
        sectionManager.StartSpawning();
        
        var spline = CurrentLevelData.levelSpline;
        if (spline.Count > 0) River_Manager.Instance.UpdateWorldSpline(spline);

        // yield return waitForFrame;
        
        OnLevelLoaded?.Invoke();
        Debug.Log($"Loaded Level '{CurrentLevelData.levelName}'");

        for (int i = 0; i < 8; i++) yield return waitForFrame;

        _loadingScreenController.EndLoadingScreen();
    }
    
    /// <summary>
    /// Get a random difficulty value based on a difficulty value
    /// </summary>
    /// <returns></returns>
    public SO_GameDifficultyValues.DifficultyValues CalculateDifficulty()
    {
        var easy = difficultyValues.GameDifficultyValues[0];
        var medium = difficultyValues.GameDifficultyValues[1];
        var hard = difficultyValues.GameDifficultyValues[2];

        float t = Mathf.Clamp01(CurrentDifficulty / difficultyValues.maxDifficulty);

        float easyWeight =
            Mathf.Lerp(easy.threshold.x, easy.threshold.y, t);

        float mediumWeight =
            Mathf.Lerp(medium.threshold.x, medium.threshold.y, t);

        float hardWeight =
            Mathf.Lerp(hard.threshold.x, hard.threshold.y, t);

        float total = easyWeight + mediumWeight + hardWeight;

        float roll = Random.Range(0f, total);

        if (roll < easyWeight)
            return easy;

        roll -= easyWeight;

        if (roll < mediumWeight)
            return medium;

        return hard;
    }
    #endregion
}

/// <summary>
/// What difficulty a section qualifies as.
/// <para>
/// Setting difficulty as none should cause a section to be ignored.
/// </para>
/// </summary>
[Flags]
public enum DifficultyQualification { None = 0, Easy = 1, Medium = 2, Hard = 4 }

/// <summary> The difficulty of game as a whole </summary>
public enum GameDifficulty { Easy, Medium, Hard }

// NOTE: You might need to modify the Sewer Environment Manager to load based on current Environment, not level
public enum Environments { Sewer = 0, Pyramid = 1, Cave = 2, Forest = 3, Dungeon = 4 }
