using System;
using System.Collections;
using EditorAttributes;
using GameCharacters;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class TutorialSectionReader : MonoBehaviour
{
    [SerializeField] private float revealSpeed = 1f;
    [SerializeField, ReadOnly] private TutorialContent[] tutorialContents =  new TutorialContent[4];
    [SerializeField] private CanvasGroup skipTutorialGroup;
    private bool[] _conditions;

    /// <summary> Check if the tutorial has been completed. Resets when the tutorial object is created. </summary>
    public static bool TutorialComplete;
    /// <summary> Event to start the tutorial sequence </summary>
    public static UnityEvent StartTutorial = new();
    /// <summary> GameObject reference to detect whether the tutorial object exists </summary>
    public static GameObject TutorialObject;

    // Input Action Listeners
    private Action _movedAction;
    private Action _vaultedAction;
    private Action _groundPoundAction;
    private Action _jumpedAction;
    private Action _steeredLeftAction;
    private Action _steeredRightAction;
    private Action _targetKilledAction;
    
    private Action _skipTutorialAction;
    
    // Skip Tutorial Input
    private InputAction _tutorialSkipInput;

    private void Awake()
    {
        // Setup conditions
        _conditions =  new bool[tutorialContents.Length];
        
        // Assign event checks from player and boat controllers
        // Move Input Section
        _movedAction += () => OnAction(0);
        
        // Vault Input Section
        _vaultedAction += () => OnAction(1);
        
        // Jump Input Section
        _jumpedAction += () => OnAction(2);
        
        // Ground Pound Section
        _groundPoundAction +=  () => OnAction(3);
        
        // Boat Steering Section
        // _steeredLeftAction += () => OnAction(5);
        // _steeredRightAction += () => OnAction(6);
        
        // Enemy Killed Section
        _targetKilledAction += () => OnAction(4); // TODO
        
        var actionMap = InputSystem.actions.actionMaps[0];

        // The name of the pause input action
        _tutorialSkipInput = actionMap.FindAction("Pause Hold");
        
        // Obtain all the tutorial content classes
        tutorialContents = GetComponentsInChildren<TutorialContent>(false);
    }

    #region Enable/Disable
    private void OnEnable()
    {
        // Reset checks

        for (int i = 0; i < _conditions.Length; i++) _conditions[i] = false;
        
        // Assign event checks from player and boat controllers
        PlayerCharacter.OnPlayerMoved += _movedAction;
        PlayerCharacter.OnPlayerVaulted += _vaultedAction;
        PlayerCharacter.OnPlayerGroundPounded += _groundPoundAction;
        PlayerCharacter.OnPlayerJumped += _jumpedAction;
        Boat_Controller.OnSteeredLeftAction += _steeredLeftAction;
        Boat_Controller.OnSteeredRightAction += _steeredRightAction;
        PlayerCharacter.OnPlayerKilledEnemy += _targetKilledAction;
        
        TutorialComplete = false;
        
        // Add the input reader routine to the Start Tutorial Unity Event for simple calling
        StartTutorial.AddListener(StartTutorialSequence);
        TutorialObject = gameObject;
    }

    private void OnDisable()
    {
        // Unassign  event checks from player and boat controllers
        PlayerCharacter.OnPlayerMoved -= _movedAction;
        PlayerCharacter.OnPlayerVaulted -= _vaultedAction;
        PlayerCharacter.OnPlayerGroundPounded -= _groundPoundAction;
        PlayerCharacter.OnPlayerJumped -= _jumpedAction;
        Boat_Controller.OnSteeredLeftAction -= _steeredLeftAction;
        Boat_Controller.OnSteeredRightAction -= _steeredRightAction;
        PlayerCharacter.OnPlayerKilledEnemy -= _targetKilledAction;
        
        // Cancel input graphic repeating updates
        foreach (var content in tutorialContents) content.CancelInvoke(nameof(content.UpdateInputGraphics));
        
        // Remove the input reader routine from the Start Tutorial
        StartTutorial.RemoveListener(StartTutorialSequence);
        TutorialObject = null;
    }
    #endregion

    private void Update()
    {
        // Tutorial Skip Input
        if (_tutorialSkipInput.WasPerformedThisFrame() &&
            !GameManager.GameLogic.IsGamePaused && !Loading_Screen_Controller.IsTransitioning) StopTutorialSequence();
    }

    #region Action Event
    private void OnAction(int id)
    {
        if (TutorialComplete) return;
        
        // Check if the previous condition has been met. Skip if the first action is selected.
        var prevCondition = _conditions[Mathf.Clamp(id-1, 0, _conditions.Length-1)];
        // ReSharper disable once InvertIf
        if (prevCondition || id == 0)
        {
            if (tutorialContents[id].UpdateContentCheckState()) _conditions[id] = true;
            // print ($"Action {id}: {_conditions[id]}");
        }
    }
    #endregion

    // private void Start()
    // {
    //     StartTutorialSequence();
    // }

    private void OnValidate()
    {
        tutorialContents = GetComponentsInChildren<TutorialContent>();
    }

    public void StartTutorialSequence()
    {
        if (_inputReaderRoutine != null) return;
        
        // Initially set all canvas groups to be invisible
        foreach (var content in tutorialContents) content.CanvasGroup.alpha = 0f;

        // TODO: Match to the Game Manager to trigger on game initialised
        _inputReaderRoutine = StartCoroutine(InputReaderRoutine());
    }

    public void StopTutorialSequence()
    {
        if (TutorialComplete) return;
        if (_inputReaderRoutine != null) StopCoroutine(_inputReaderRoutine);
        
        OnTutorialCompleted();
    }

    private Coroutine _inputReaderRoutine;
    private IEnumerator InputReaderRoutine()
    {
        Debug.Log("Starting Input Reader Routine");
        
        for (int i = 0; i < tutorialContents.Length; i++)
        {
            // Reveal the tutorial content
            while (tutorialContents[i].RevealGroupContent()) yield return null;
            
            // If it's the enemy tutorial, spawn an enemy (TODO: FIX HARD CODED THINGAMAJIG)
            if (i == 4) SpawnEnemy();
            
            yield return new WaitUntil(() => _conditions[i]);
            
            // Establish the previous content id
            int fadeID = i - 1;
            // Continue if ID is out of the tutorial content range
            if (fadeID < 0) continue;
            
            // Fade the previous tutorial content
            while (tutorialContents[fadeID].FadeGroupContent()) yield return null;
        }

        // Trigger Tutorial Complete
        OnTutorialCompleted();

        // Cycle through the canvas groups and reduce their transparency
        var amount = tutorialContents.Length - 1;
        while (tutorialContents[amount].FadeGroupContent()) // Wait until the last group has finished fading
        {
            foreach (var content in tutorialContents) content.FadeGroupContent();
            yield return null;
        }
        
        // Destroy the tutorial section
        Destroy(gameObject);
    }
    
    private void OnTutorialCompleted()
    {
        TutorialComplete = true;
        // Debug.Log("Tutorial Completed");
        
        // Initially set all canvas groups to be invisible
        foreach (var content in tutorialContents) content.CanvasGroup.alpha = 0f;

        skipTutorialGroup.alpha = 0f;
        
        // Start the game once the tutorial has been completed
        GameManager.GameLogic.StartGame();
    }

    private void SpawnEnemy()
    {
        var enm = ObjectPoolManager.Instance.Spawn<River_Enemy>(0); // Spawn Crocodile
        // Override Data
        var data = new BoatEnemy_Data
        {
            targetSpace = 1
        };
        enm.OverrideData(data);
        // PlaceSectionObject(enm, item.lane, item.distance, item.height);
        
        // Debug.Log($"Placing Object: {ro.name}. Lane = {lane}, Distance = {distance}, Height = {height}");
        enm.InjectRiverManager(River_Manager.Instance);
        enm.canMove = true;
    
        enm.StartOnLane(1, River_Manager.Instance.BoatController.RiverSplineObject.GlobalDistanceTravelled, 0);
        Debug.Log("TUTORIAL SPAWNED ENEMY");
    }
    
    
}
