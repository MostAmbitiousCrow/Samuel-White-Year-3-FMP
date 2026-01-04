using EditorAttributes;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary> Main source of management for the game. Always exists. </summary>
[RequireComponent(typeof(MainSceneManager))]
public class GameManager : MonoBehaviour
{
    //private static GameManager _instance;
    public static GameManager Instance { get; private set; }

    public static MainSceneManager SceneManager { get; private set; }
    public static GameLevelManager LevelManager { get; private set; }
    public static GameUserSettings UserSettings { get; private set; } = new();

    public static MainGameLogic GameLogic { get; private set; } = new();

    public EventSystem CurrentEventSystem { get { return _currentEventSystem; } }
    [SerializeField] EventSystem _currentEventSystem;
    private void Awake()
    {
        if (Instance) 
        {
            Debug.Log("Game Manager already exists, deleting new Game Manager");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this);

        // Initialise Managers (Temporary)
        GameLogic = new MainGameLogic();
        SceneManager = GetComponent<MainSceneManager>();
        //LevelManager = GetComponent<GameLevelManager>();
        UserSettings = new GameUserSettings();

        print($"Game Logic = {GameLogic} | Scene Manager = {SceneManager} | Level Manager = {LevelManager} | User Settings = {UserSettings}");
    }

    private void Start()
    {
        SceneManager.onLevelLoaded += Wait;
        //SetEventSystem();
        StartCoroutine(Uhh());
        GameLogic.InitialiseGame(); //TODO: Temp
    }

    [Button]
    public void DEVInitialiseGame()
    {
        if(GameLogic.GameStarted)
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
        public bool GameStarted { get { return _gameStarted; } }
        bool _gameStarted;
        public delegate void OnGameInitialised();
        public OnGameInitialised onGameInitialised;
        public void InitialiseGame()
        {
            if (SceneManager.CurrentScene == MainSceneManager.GameScenes.MainMenu)
            {
                print("Scene is Main Menu, ignoring Game Initialisation");
                return;
            }

            LevelManager = FindObjectOfType<GameLevelManager>();

            // Logic to initalise the main game scene before starting the game
            playerData = new()
            {
                PlayerTransform = FindObjectOfType<PlayerStateController>().transform // TODO: Maker Cleaner
            };

            print("Game Initialised, started Game");
            StartGame(); // Start the game upon the game being initialised.// TODO: Move to a stage where the game is started via a countdown/trigger
        }

        public delegate void OnGameStarted();
        public OnGameStarted onGameStarted;
        public void StartGame()
        {
            // Logic to start the main game after it has been initialised
            _gameStarted = true;

            // Invoke subscribed methods
            onGameStarted?.Invoke();

            Debug.Log("Game Started");
        }

        public delegate void OnGameEnded();
        public OnGameEnded onGameEnded;
        /// <summary> Method that ends the current game session. </summary>
        public void EndGame()
        {
            // Logic to end the main game after it has started
            _gameStarted = false;

            // Invoke subscribed methods
            onGameEnded?.Invoke();

            Debug.Log("Game Ended");
        }

        /// <summary> Method that resets the current game session. </summary>
        public void ResetGame()
        {
            // TODO: Reset game content here
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

        public delegate void OnPlayerDeath();
        public OnPlayerDeath onPlayerDeath;

        /// <summary> Method to trigger whenever the player dies during the game </summary>
        public void PlayerDied()
        {
            onPlayerDeath?.Invoke();

            EndGame();
        }

        #endregion
    }
    #endregion

    #region Game Settings
    public partial class GameUserSettings
    {
        public delegate void SettingsUpdated(GameSettings gameSettings);
        public SettingsUpdated onSettingsUpdated;

        public GameSettings gameSettings;
        public class GameSettings
        {
            public AspectResolution TargetAspectResolution;


        }
    }
    #endregion

    public void SetEventSystem()
    {
        _currentEventSystem = FindObjectOfType<EventSystem>();

        if (!_currentEventSystem)
        {
            Debug.LogWarning($"Failed to set new event system");
        }
        else
        {
            Debug.Log($"New Event System {_currentEventSystem}");
            EventSystem.current = _currentEventSystem;
        }
    }
    void Wait()
    {
        StartCoroutine(Uhh());
    }
    IEnumerator Uhh()
    {
        yield return new WaitForSeconds(1f);
        SetEventSystem();
    }
}