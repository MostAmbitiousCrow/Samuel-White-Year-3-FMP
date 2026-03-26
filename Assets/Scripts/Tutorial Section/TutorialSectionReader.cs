using System;
using System.Collections;
using EditorAttributes;
using GameCharacters;
using UnityEngine;
using UnityEngine.Events;

public class TutorialSectionReader : MonoBehaviour
{
    [SerializeField] private float revealSpeed = 1f;
    [SerializeField, ReadOnly] private TutorialContent[] tutorialContents =  new TutorialContent[4];
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
    private Action _heavyVaultedAction;
    private Action _jumpedAction;
    private Action _jumpedHeavyAction;
    private Action _steeredLeftAction;
    private Action _steeredRightAction;
    private Action _targetKilledAction;
    
    // Skip Tutorial Input
    private Action _tutorialSkipAction;

    private void Awake()
    {
        // Setup conditions
        _conditions =  new bool[tutorialContents.Length];
        
        // Assign event checks from player and boat controllers
        // Move Input Section
        _movedAction += () => OnAction(0);
        
        // Vault Input Section
        _vaultedAction += () => OnAction(1);
        _heavyVaultedAction +=  () => OnAction(2);
        
        // Jump Input Section
        _jumpedAction += () => OnAction(3);
        _jumpedHeavyAction += () => OnAction(4);
        
        // Boat Steering Section
        _steeredLeftAction += () => OnAction(5);
        _steeredRightAction += () => OnAction(6);
        
        // Enemy Killed Section
        // _targetKilledAction += () => OnAction(7); // TODO
        
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
        PlayerCharacter.OnPlayerHeavyVaulted += _heavyVaultedAction;
        PlayerCharacter.OnPlayerJumped += _jumpedAction;
        PlayerCharacter.OnPlayerHeavyJumped += _jumpedHeavyAction;
        Boat_Controller.OnSteeredLeftAction += _steeredLeftAction;
        Boat_Controller.OnSteeredRightAction += _steeredRightAction;
        PlayerCharacter.OnPlayerKilledEnemy += _targetKilledAction;
        
        TutorialComplete = false;

        // Start input graphic repeating updates
        foreach (var content in tutorialContents)
            content.InvokeRepeating(nameof(content.UpdateInputGraphics), 0f, 1f);
        
        // Add the input reader routine to the Start Tutorial Unity Event for simple calling
        StartTutorial.AddListener(() => StartCoroutine(InputReaderRoutine()));
        TutorialObject = gameObject;
    }

    private void OnDisable()
    {
        // Unassign  event checks from player and boat controllers
        PlayerCharacter.OnPlayerMoved -= _movedAction;
        PlayerCharacter.OnPlayerVaulted -= _vaultedAction;
        PlayerCharacter.OnPlayerHeavyVaulted -= _heavyVaultedAction;
        PlayerCharacter.OnPlayerJumped -= _jumpedAction;
        PlayerCharacter.OnPlayerHeavyJumped -= _jumpedHeavyAction;
        Boat_Controller.OnSteeredLeftAction -= _steeredLeftAction;
        Boat_Controller.OnSteeredRightAction -= _steeredRightAction;
        PlayerCharacter.OnPlayerKilledEnemy -= _targetKilledAction;
        
        // Cancel input graphic repeating updates
        foreach (var content in tutorialContents) content.CancelInvoke(nameof(content.UpdateInputGraphics));
        
        // Remove the input reader routine from the Start Tutorial
        StartTutorial.RemoveListener(() => StartCoroutine(InputReaderRoutine()));
        TutorialObject = null;
    }
    #endregion

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

    private void Start()
    {
        StartTutorialSequence();
    }

    private void OnValidate()
    {
        tutorialContents = GetComponentsInChildren<TutorialContent>(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Backspace)) StopTutorialSequence();
    }

    public void StartTutorialSequence()
    {
        if (_inputReaderRoutine != null) StopCoroutine(_inputReaderRoutine);
        
        // Initially set all canvas groups to be invisible
        foreach (var content in tutorialContents) content.CanvasGroup.alpha = 0f;

        // TODO: Match to the Game Manager to trigger on game initialised
        _inputReaderRoutine = StartCoroutine(InputReaderRoutine());
    }

    public void StopTutorialSequence()
    {
        if (_inputReaderRoutine != null) StopCoroutine(_inputReaderRoutine);
        
        // Initially set all canvas groups to be invisible
        foreach (var content in tutorialContents) content.CanvasGroup.alpha = 0f;
        
        OnTutorialCompleted();
    }

    private Coroutine _inputReaderRoutine;
    private IEnumerator InputReaderRoutine()
    {
        for (int i = 0; i < tutorialContents.Length; i++)
        {
            // Reveal the tutorial content
            while (tutorialContents[i].RevealGroupContent()) yield return null;
            
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
        Debug.Log("Tutorial Completed");
        
        // Start the game once the tutorial has been completed
        GameManager.GameLogic.StartGame();
    }
}
