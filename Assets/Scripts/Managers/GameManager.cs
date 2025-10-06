using UnityEngine;

[RequireComponent(typeof(MainSceneManager))]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public MainSceneManager SceneManager { get; private set; }
    public GameLevelManager LevelManager { get; private set; }
    
    public MainGameLogic GameLogic { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(this);

        GameLogic = new MainGameLogic();
        SceneManager = GetComponent<MainSceneManager>();

        // if (SceneManager.CurrentScene == MainSceneManager.GameScenes.MainGame)
        // {
        //     GameLogic.InitialiseGame();
        // }

        GameLogic.InitialiseGame(); //TODO: Temp
    }

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
            playerData = new()
            {
                PlayerTransform = FindObjectOfType<Player_Controller>().transform // TODO: Maker Cleaner
            };

            print("Game Initialised");
        }

        public void StartGame()
        {
            // Logic to start the main game after it has been initialised
        }

        public void EndGame()
        {
            // Logic to end the main game after it has started
        }

        public PlayerData playerData = new();

        public class PlayerData
        {
            public int CurrentGemstones { get; set; } = 0;
            public Transform PlayerTransform { get; set; }
            // public Player_Controller controller; // TODO: Create a script that controls certain player events (dying, resetting etc)
        }

        public delegate void OnGemstoneCollected(int gemstones);
        public OnGemstoneCollected onGemstoneCollected;

        public void AddGemstones(int amount = 1)
        {
            playerData.CurrentGemstones += amount;
            onGemstoneCollected?.Invoke(playerData.CurrentGemstones); // Invoke all scripts that react to the collection of a gemstone
            print($"Player Collected a Gemstone. Current Gemstones: {playerData.CurrentGemstones}");
        }
    }
    #endregion
}
