using EditorAttributes;
using System;
using UnityEngine;

/// <summary>
/// Main source of management for the game. Always exists.
/// </summary>
[RequireComponent(typeof(MainSceneManager))]
public class GameManager : MonoBehaviour
{
    //private static GameManager _instance;
    public static GameManager Instance { get; private set; }

    public static MainSceneManager SceneManager { get; private set; }
    public static GameLevelManager LevelManager { get; private set; }
    public static GameUserSettings UserSettings { get; private set; } = new();

    public static MainGameLogic GameLogic { get; private set; } = new();

    private void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(this);

        // Initialise Managers (Temporary)
        GameLogic = new MainGameLogic();
        SceneManager = GetComponent<MainSceneManager>();
        LevelManager = GetComponent<GameLevelManager>();
        UserSettings = new GameUserSettings();
    }

    private void Start()
    {
        GameLogic.InitialiseGame(); //TODO: Temp
    }

    [Button]
    public void DEVInitialiseGame()
    {
        GameLogic.InitialiseGame();
    }

    #region Main Game Logic
    public class MainGameLogic
    {
        /// <summary> Checker if the game is paused </summary>
        public bool GamePaused { get { return _gamePaused; } }
        [SerializeField] bool _gamePaused;
        public int GamePauseInt { get { return GamePaused ? 0 : 1; } }
        
        /// <summary> Delegate for whenever the game is paused </summary>
        public delegate void OnGamePause();
        public OnGamePause onGamePause;
        /// <summary> Delegate for whenever the game is resumed </summary>
        public delegate void OnGameResume();
        public OnGameResume onGameResume;

        public void SetPauseState(bool state)
        {
            // Pause game logic here
            print($"Game Pause State = {_gamePaused = state}");

            if (state) onGamePause?.Invoke();
            else onGameResume?.Invoke();
        }

        public void TogglePauseState()
        {
            // Pause game logic here
            print($"Game Pause State = {_gamePaused = !_gamePaused}");

            if (_gamePaused) onGamePause?.Invoke();
            else onGameResume?.Invoke();
        }

        #region Game Initialisation
        public delegate void OnGameInitialised();
        public OnGameInitialised onGameInitialised;
        public void InitialiseGame()
        {
            // Logic to initalise the main game scene before starting the game
            playerData = new()
            {
                PlayerTransform = FindObjectOfType<Player_Controller>().transform // TODO: Maker Cleaner
            };

            print("Game Initialised");
        }

        public delegate void OnGameStarted();
        public OnGameStarted onGameStarted;
        public void StartGame()
        {
            // Logic to start the main game after it has been initialised
            Debug.Log("Game Started");
        }

        public delegate void OnGameEnded();
        public OnGameEnded onGameEnded;
        public void EndGame()
        {
            // Logic to end the main game after it has started
            Debug.Log("Game Ended");
        }
        #endregion

        #region Player
        /// <summary>
        /// The class containing all data related to the current player
        /// </summary>
        public PlayerData playerData = new();

        public class PlayerData
        {
            public int CurrentGemstones { get; set; } = 0;
            public Transform PlayerTransform { get; set; }
            // public Player_Controller controller; // TODO: Create a script that controls certain player events (dying, resetting etc)

            public bool IsControlsPaused { get; private set; }
        }

        public event Action<int> OnGemstoneCollected;
        public event Action<int> OnPlayerDamaged;

        public void AddGemstones(int amount = 1)
        {
            playerData.CurrentGemstones += amount;
            OnGemstoneCollected?.Invoke(playerData.CurrentGemstones); // Invoke all scripts that react to the collection of a gemstone
            // print($"Player Collected a Gemstone. Current Gemstones: {playerData.CurrentGemstones}");
        }
        #endregion
    }
    #endregion

    #region Game Settings
    public class GameUserSettings
    {
        public delegate void SettingsUpdated(GameSettings gameSettings);
        public SettingsUpdated onSettingsUpdated;

        public GameSettings gameSettings;
        public class GameSettings
        {
            public AsepectResolution TargetAspectResolution;


        }
    }
    #endregion
}