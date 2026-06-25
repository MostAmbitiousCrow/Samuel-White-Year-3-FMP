using Environment_Select;
using GameCharacters;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Game_UI : MonoBehaviour
{
    public static Game_UI Instance;
    private static readonly int Alpha = Shader.PropertyToID("_Alpha");
    

    private void Update()
    {
        AnimateHealthBar();
        if (_isEnvironmentSelectActive) EnvironmentSelectionProcess();
    }
    
    #region Subscriptions
    private void Awake()
    {
        Instance = this;
        _storedTime = Time.time;
        
        environmentSelectFolder.SetActive(false);
    }
    private void Start()
    {
        // GameManager.GameLogic.OnGemstoneCollected += UpdateGemstoneCounter;
        PlayerCharacter.OnPlayerDamaged += CheckPlayerHealth;
        PlayerCharacter.OnPlayerDied += _ => ResetHealthBorder();
        GameLevelManager.OnLevelLoaded += ResetHealthBorder;
    }
    private void OnEnable()
    {
        // GameManager.GameLogic.OnGemstoneCollected += UpdateGemstoneCounter;
        PlayerCharacter.OnPlayerDamaged += CheckPlayerHealth;
        PlayerCharacter.OnPlayerDied += _ => ResetHealthBorder();
        GameLevelManager.OnLevelLoaded += ResetHealthBorder;
    }

    private void OnDisable()
    {
        // GameManager.GameLogic.OnGemstoneCollected -= UpdateGemstoneCounter;
        PlayerCharacter.OnPlayerDamaged -= CheckPlayerHealth;
        PlayerCharacter.OnPlayerDied -= _ => ResetHealthBorder();
        GameLevelManager.OnLevelLoaded -= ResetHealthBorder;
    }

    private void OnValidate()
    {
        environmentSelectUI = GetComponentsInChildren<EnvironmentSelectUI>(true);

        // OpenEnvironmentSelect(yeah);
    }

    #endregion

    #region Player Health UI
    [Header("Player UI")]
    [SerializeField] private Material visualHealthBorder;
    [SerializeField] private float fadeDuration = 3.25f;
    private float _storedTime;
    private bool _isHealthInDanger;

    /// <summary>  </summary>
    private void CheckPlayerHealth(int health)
    {
        // If on their last hit point, set health in danger true
        _isHealthInDanger = health == 1;
        _storedTime = Time.time + fadeDuration; // Duration of the fade based on player invincibility duration
        // TODO: ^ Link this up to the player invincibility duration?
    }

    private void ResetHealthBorder()
    {
        visualHealthBorder.SetFloat(Alpha, 0f);
        _isHealthInDanger = false;
    }

    private void AnimateHealthBar()
    {
        var progress = _storedTime - Time.time;

        if (progress < 0f) return;
        
        var value = Mathf.InverseLerp(0f, fadeDuration, progress);

        var target = _isHealthInDanger ? .15f : 0f;
        var start = _isHealthInDanger ? 0f : .15f;
        var lerp = Mathf.Lerp(target, start, value);
        visualHealthBorder.SetFloat(Alpha, lerp);
    }



    #endregion

    #region Gemstone Counter
    [SerializeField] private TextMeshProUGUI gemstoneCounterText;

    /// <summary> Function to update the UI for the Gemstone Counter. Parameter must be the current Gemstone count. </summary>
    /*private void UpdateGemstoneCounter(int gemstones)
    {
        // if (!gemstoneCounterText.gameObject)
        // {
        //     Debug.LogError("Gemstone Counter Text is Missing!");
        //     return;
        // }
        if(gemstones <= 0) gemstoneCounterText.SetText("Gemstones: 0");

        gemstoneCounterText.SetText($"Gemstones: {gemstones}");
        //print($"Updated Gemstone Text: {gemstones}");
    }*/

    #endregion
    
    #region Environment Select
    
    [Header("Environment Select")]
    [SerializeField] private SO_EnvironmentPaths environmentPaths;
    [SerializeField] private EnvironmentSelectUI[] environmentSelectUI;
    [Space]
    [SerializeField] private GameObject environmentSelectFolder;
    [SerializeField] private Sprite[] directionSprites;
    [Space]
    [SerializeField] private TextMeshProUGUI countDownText;
    [SerializeField] private float selectionTime = 6.5f;
    private bool _isEnvironmentSelectActive;
    private int _branchCount;
    private SO_EnvironmentPaths.EnvironmentPath _environmentPath;

    /// <summary>
    /// <para> Shows the Environment Select Options when deciding to move to a new environment</para>
    /// </summary>
    public void OpenEnvironmentSelect(Environments environment)
    {
        _environmentPath = environmentPaths.paths[(int)environment];
        Debug.Log($"Selected Environment: {_environmentPath.root}");
        
        environmentSelectFolder.SetActive(true);

        foreach (var element in environmentSelectUI)
        {
            element.DeSelectEnvironment();
            element.gameObject.SetActive(false);
        }

        _branchCount = _environmentPath.branches.Length;
        // Update selections based on the current environment
        for (int i = 0; i < _branchCount; i++)
        {
            var element = environmentSelectUI[i];
            
            element.gameObject.SetActive(true);
            element.UpdateSelectDetails(environmentPaths.paths[(int)_environmentPath.branches[i]]);
        }

        // Set the arrow icons to each environment direction
        if (_branchCount < 3)
        {
            // Set icons to use the Left and Right arrows
            environmentSelectUI[0].DirectionalIcon.sprite = directionSprites[0];
            environmentSelectUI[1].DirectionalIcon.sprite = directionSprites[2];
        }
        else
        {
            // Set icons to use all arrows
            for (int i = 0; i < _branchCount; i++)
                environmentSelectUI[i].DirectionalIcon.sprite = directionSprites[i];
        }

        _selectTime = selectionTime;
        _isEnvironmentSelectActive = true;
        
        _lastSelectedBoatLane = River_Manager.Instance.BoatController.CurrentLane.id;
        
        var currentUI = GetEnvironmentUIIndex(_lastSelectedBoatLane);
        if (currentUI != -1) environmentSelectUI[currentUI].SelectEnvironment();
    }

    /// <summary>
    /// <para> Hides the Environment Select Options </para>
    /// </summary>
    public void ChooseEnvironment()
    {
        // Disable content folder //TODO: Temp - Animate process?
        environmentSelectFolder.SetActive(false);
        
        // Reset conditions
        _isEnvironmentSelectActive = false;
        _selectTime = 0f;
        
        int boatLane = River_Manager.Instance.BoatController.CurrentLane.id;
        
        // If only 2 branches, middle lane will default to zero.
        if (_branchCount == 2 && boatLane == 1) boatLane = 0;
        var environment = _environmentPath.branches[GetEnvironmentUIIndex(boatLane)];
        GameLevelManager.Instance.LoadEnvironmentAndLevel(environment);
    }

    private int _lastSelectedBoatLane;
    private float _selectTime;
    private void EnvironmentSelectionProcess()
    {
        int boatLane = River_Manager.Instance.BoatController.CurrentLane.id;

        if (_selectTime < 0f)
        {
            ChooseEnvironment();
            countDownText.SetText($"0s Remaining");
        }
        else
        {
            countDownText.SetText($"{Mathf.RoundToInt(_selectTime)}s Remaining");
            _selectTime -= Time.deltaTime;
        }
        
        if (boatLane == _lastSelectedBoatLane)
            return;

        int previousUI = GetEnvironmentUIIndex(_lastSelectedBoatLane);
        int currentUI = GetEnvironmentUIIndex(boatLane);

        // Deselect previous
        if (previousUI != -1)
            environmentSelectUI[previousUI].DeSelectEnvironment();

        // Store new lane
        _lastSelectedBoatLane = boatLane;

        // Select new (if valid)
        if (currentUI != -1)
            environmentSelectUI[currentUI].SelectEnvironment();
    }
    
    private int GetEnvironmentUIIndex(int boatLane)
    {
        if (_branchCount == 2)
        {
            return boatLane switch
            {
                0 => 0,
                2 => 1,
                _ => -1
            };
        }

        return boatLane;
    }
    
    #endregion
}
