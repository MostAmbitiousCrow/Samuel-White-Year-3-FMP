using System;
using System.Collections.Generic;
using Environment_Select;
using GameCharacters;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class Game_UI : MonoBehaviour
{
    public static Game_UI Instance;
    private static readonly int Alpha = Shader.PropertyToID("_Alpha");

    #region Subscriptions
    private void Awake()
    {
        Instance = this;
        _storedTime = Time.time;
        
        environmentSelectFolder.SetActive(false);
    }
    private void Start()
    {
        GameManager.GameLogic.OnGemstoneCollected += UpdateGemstoneCounter;
        PlayerCharacter.OnPlayerDamaged += CheckPlayerHealth;
        PlayerCharacter.OnPlayerDied += _ => ResetHealthBorder();
        GameLevelManager.OnLevelLoaded += ResetHealthBorder;
    }
    private void OnEnable()
    {
        GameManager.GameLogic.OnGemstoneCollected += UpdateGemstoneCounter;
        PlayerCharacter.OnPlayerDamaged += CheckPlayerHealth;
        PlayerCharacter.OnPlayerDied += _ => ResetHealthBorder();
        GameLevelManager.OnLevelLoaded += ResetHealthBorder;
    }

    private void OnDisable()
    {
        GameManager.GameLogic.OnGemstoneCollected -= UpdateGemstoneCounter;
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

    private void Update()
    {
        var progress = _storedTime - Time.time;

        if (progress < 0f) return;
        
        var value = Mathf.InverseLerp(0f, fadeDuration, progress);

        var target = _isHealthInDanger ? .15f : 0f;
        var start = _isHealthInDanger ? 0f : .15f;
        var lerp = Mathf.Lerp(target, start, value);
        visualHealthBorder.SetFloat(Alpha, lerp);
        
        // Debug.Log($"Health in Danger = {_isHealthInDanger}. Updating Health Border: Progress = {progress} Lerp = {lerp}");
    }

    #endregion

    #region Gemstone Counter
    [SerializeField] private TextMeshProUGUI gemstoneCounterText;

    /// <summary> Function to update the UI for the Gemstone Counter. Parameter must be the current Gemstone count. </summary>
    private void UpdateGemstoneCounter(int gemstones)
    {
        // if (!gemstoneCounterText.gameObject)
        // {
        //     Debug.LogError("Gemstone Counter Text is Missing!");
        //     return;
        // }
        if(gemstones <= 0) gemstoneCounterText.SetText("Gemstones: 0");

        gemstoneCounterText.SetText($"Gemstones: {gemstones}");
        //print($"Updated Gemstone Text: {gemstones}");
    }

    #endregion
    
    #region Environment Select
    
    [Header("Environment Select")]
    [SerializeField] private SO_EnvironmentPaths environmentPaths;
    [SerializeField] private EnvironmentSelectUI[] environmentSelectUI;
    [Space]
    [SerializeField] private GameObject environmentSelectFolder;

    /// <summary>
    /// <para> Shows the Environment Select Options when deciding to move to a new environment</para>
    /// <para> This stops the player from moving until the select screen is closed </para>
    /// </summary>
    public void OpenEnvironmentSelect(Environments environment)
    {
        var selectedRoot = environmentPaths.paths[(int)environment];
        Debug.Log($"Selected Environment: {selectedRoot.root}");
        
        environmentSelectFolder.SetActive(true);

        foreach (var element in environmentSelectUI)
        {
            element.gameObject.SetActive(false);
        }

        // Update selections based on the current environment
        for (int i = 0; i < selectedRoot.branches.Length; i++)
        {
            var element = environmentSelectUI[i];
            
            element.gameObject.SetActive(true);
            element.UpdateSelectDetails(environmentPaths.paths[(int)selectedRoot.branches[i]]);
        }
        
        // Select the first UI Select
        EventSystem.current.SetSelectedGameObject(environmentSelectUI[0].Button.gameObject);
        
        //TODO: Prevent Player Character Input whilst this menu is open!
        GameManager.GameLogic.SetPauseState(true, true);
        GameManager.GameLogic.CanPauseGame = false;
    }

    /// <summary>
    /// <para> Hides the Environment Select Options </para>
    /// <para> Additionally restores player input </para>
    /// </summary>
    public void CloseEnvironmentSelect()
    {
        // Disable content folder //TODO: Temp - Leave room for an animation?
        environmentSelectFolder.SetActive(false);
        
        //TODO: Restore Player Character Input!
        GameManager.GameLogic.CanPauseGame = true;
        GameManager.GameLogic.SetPauseState(false, true);
    }
    
    #endregion
}
