using System;
using System.Collections;
using UnityEngine;

/*
 * The Game Level Manager exists in the Main Game scene and stores Scriptable Object Level Data.
 * The Current Level Data is then provided to the Section Manager to load the level content.
 */
public class GameLevelManager : MonoBehaviour
{
    private Game_Section_Manager _sectionManager;
    
    #region Initialisation
    private void Awake()
    {
        _sectionManager = FindFirstObjectByType<Game_Section_Manager>();
        if (_sectionManager == null) Debug.LogError("No Game Section Manager was found!");
        
        GameManager.GameLogic.OnGameStarted += InitialiseFirstLevel;
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

    [SerializeField] private LevelData currentLevelInstance;
    /// <summary> The count of the amount of levels that exist </summary>
    public int LevelCount { get; private set; }
    
    [SerializeField] private SO_LevelData[] levels;
    public  SO_LevelData[] Levels => levels;

    // Events
    public delegate void LevelLoaded();
    public static LevelLoaded OnLevelLoaded;

    private void InitialiseFirstLevel()
    {
        LoadLevel(0);
        Debug.Log("First Level Loaded");
    }

    /// <summary> Loads a specified level </summary>
    public void LoadLevel(int level)
    {
        if (level < 0 || level > LevelCount) 
        { Debug.LogWarning("Unable to load level less than 0 or greater than the current level count."); return; }
        
        // Assign the new current level ID
        CurrentLevel = level;
        
        // Assign the Section Manager it's new level data
        InstantiateLevel(levels[CurrentLevel], out var levelData);
        currentLevelInstance =  levelData;
        _sectionManager.AssignNewLevelData(currentLevelInstance);
        OnLevelLoaded?.Invoke();
        
        Debug.Log($"Loaded Level {currentLevelInstance.name}");
    }

    private void InstantiateLevel(SO_LevelData soLevelData,out LevelData levelData)
    {
        if (currentLevelInstance != null) UnloadCurrentLevel();

        var instance = Instantiate(soLevelData.sectionDataObject);
        levelData = instance.GetComponent<LevelData>();
    }

    public void UnloadCurrentLevel()
    {
        Debug.Log($"Unloading current level: {currentLevelInstance.name}");
       Destroy(currentLevelInstance.gameObject);
    }
    
    /// <summary> Loads the previous level based on the current level </summary>
    public void LoadPreviousLevel()
    {
        LoadLevel(CurrentLevel--);
    }

    /// <summary> Loads the next level based on the current level </summary>
    public void LoadNextLevel()
    {
        LoadLevel(CurrentLevel++);
    }
    #endregion
}