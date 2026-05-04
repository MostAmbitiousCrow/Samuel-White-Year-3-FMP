using EditorAttributes;
using System;
using GameCharacters;
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
    // public static GameUserSettings UserSettings { get; private set; } = new();

    public static MainGameLogic GameLogic { get; private set; } = new();

    public EventSystem CurrentEventSystem => currentEventSystem;
    [SerializeField] private EventSystem currentEventSystem;
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
        // UserSettings = new GameUserSettings();

        // print($"Game Logic = {GameLogic} | Scene Manager = {SceneManager} | Level Manager = {LevelManager} | User Settings = {UserSettings}");
    }

    private void Start()
    {
        SceneManager.onLevelLoaded += SetEventSystem;
        SetEventSystem();
        // StartCoroutine(Uhh());
        GameLogic.InitialiseGame(); //TODO: Temp
    }

    [Button]
    public void DEVInitialiseGame()
    {
        if(GameLogic.GameStarted) GameLogic.InitialiseGame();
    }
    
    [Button]
    public void DEVStartGame()
    {
        GameLogic.StartGame();
    }

    #region Main Game Logic
    public class MainGameLogic
    {
        /// <summary> Checker if the game is paused </summary>
        public bool GamePaused => _gamePaused;
        [SerializeField] bool _gamePaused;
        public int GamePauseInt => GamePaused ? 0 : 1;

        /// <summary> Delegate for whenever the game is paused </summary>
        public delegate void GamePause();
        public GamePause OnGamePaused;
        public delegate void GameResume();
        /// <summary> Delegate for whenever the game is resumed </summary>
        public GameResume OnGameResume;

        public void SetPauseState(bool state)
        {
            // Pause game logic here
            print($"Game Pause State = {_gamePaused = state}");

            if (state)
            {
                Time.timeScale = 0f;
                OnGamePaused?.Invoke();
            }
            else
            {
                Time.timeScale = 1f;
                OnGameResume?.Invoke();
            }
        }

        public void TogglePauseState()
        {
            // Pause game logic here
            _gamePaused = !_gamePaused;
            print($"Game Pause State = {_gamePaused}");

            if (_gamePaused)
            {
                Time.timeScale = 0f;
                OnGamePaused?.Invoke();
            }
            else
            {
                Time.timeScale = 1f;
                OnGameResume?.Invoke();
            }
        }

        #region Game Initialisation
        public bool GameStarted => _gameStarted;
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

            LevelManager = FindFirstObjectByType<GameLevelManager>();

            // Logic to initalise the main game scene before starting the game
            playerData = new()
            {
                PlayerTransform = FindFirstObjectByType<PlayerCharacter>().transform // TODO: Maker Cleaner
            };
            
            onGameInitialised?.Invoke();

            // If the tutorial object exists in the scene, start the tutorial!
            if (TutorialSectionReader.TutorialObject)
            {
                TutorialSectionReader.StartTutorial.Invoke();
            }
            // Otherwise, start the game
            else
            {
                print("Game Initialised, started Game");
                StartGame();
            }

        }

        public Action OnGameStarted;
        public void StartGame()
        {
            // Logic to start the main game after it has been initialised
            _gameStarted = true;

            // Invoke subscribed methods
            OnGameStarted?.Invoke();

            Debug.Log("Game Started");
        }

        public static event Action<GameOverType> OnGameOver;
        
        /// <summary>
        /// An enum to help determine what type of game over was triggered
        /// </summary>
        public enum GameOverType
        {
            Default, Tsunami
        }
        /// <summary> Method that ends the current game session. </summary>
        public void EndGame(GameOverType gameOverType = GameOverType.Default)
        {
            // Logic to end the main game after it has started
            _gameStarted = false;
            // Invoke subscribed methods
            OnGameOver?.Invoke(gameOverType);

            Debug.Log("Game Ended");
        }

        /// <summary> Method that resets the current game session. </summary>
        public void ResetGame()
        {
            // TODO: Reset game content here
        }

        public delegate void GameCompleted();
        public GameCompleted OnGameCompleted;
        /// <summary> Method to call when the game has been completed. Typically if the player has beaten all levels. </summary>
        public void CompleteGame()
        {
            OnGameCompleted?.Invoke();
            Debug.Log("Game Completed");
            
            // TODO: Decide on a proper game complete thing... This is just temporary for the demo
            SceneManager.LoadScene(MainSceneManager.GameScenes.DemoCompleteScreen);
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

        public void AddGemstones(int amount = 1)
        {
            playerData.CurrentGemstones += amount;
            OnGemstoneCollected?.Invoke(playerData.CurrentGemstones); // Invoke all scripts that react to the collection of a gemstone
            // print($"Player Collected a Gemstone. Current Gemstones: {playerData.CurrentGemstones}");
        }

        /// <summary> Method to trigger whenever the player dies during the game </summary>
        public void PlayerDied()
        {

            EndGame();
        }

        #endregion
    }
    #endregion

    // #region Game Settings
    // public class GameUserSettings
    // {
    //     public delegate void SettingsUpdated(GameSettings gameSettings);
    //     public SettingsUpdated onSettingsUpdated;
    //
    //     public GameSettings gameSettings;
    //     public class GameSettings
    //     {
    //         public AspectResolution TargetAspectResolution;
    //
    //
    //     }
    // }
    // #endregion

    public void SetEventSystem()
    {
        currentEventSystem = FindFirstObjectByType<EventSystem>();

        if (!currentEventSystem)
        {
            Debug.LogWarning("Failed to set new event system");
        }
        else
        {
            Debug.Log($"New Event System {currentEventSystem}");
            EventSystem.current = currentEventSystem;
        }
    }
    // void Wait()
    // {
    //     StartCoroutine(Uhh());
    // }
    // IEnumerator Uhh()
    // {
    //     yield return new WaitForSeconds(1f);
    //     SetEventSystem();
    // }
}