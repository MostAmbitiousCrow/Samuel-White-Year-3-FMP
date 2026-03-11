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

    private Action _movedAction;
    private Action _vaultedAction;
    private Action _jumpedAction;
    private Action _steeredAction;

    private void Awake()
    {
        // Setup conditions
        _conditions =  new bool[tutorialContents.Length];
        
        // Assign event checks from player and boat controllers
        _movedAction += () => OnAction(0);
        _vaultedAction += () => OnAction(1);
        _jumpedAction += () => OnAction(2);
        _steeredAction += () => OnAction(3);
        
        // Obtain all the tutorial content classes
        tutorialContents = GetComponentsInChildren<TutorialContent>();
    }

    #region Enable/Disable
    private void OnEnable()
    {
        // Reset checks

        for (int i = 0; i < _conditions.Length; i++) _conditions[i] = false;
        
        // Assign event checks from player and boat controllers
        PlayerCharacter.OnPlayerMoved += _movedAction;
        PlayerCharacter.OnPlayerVaulted += _vaultedAction;
        PlayerCharacter.OnPlayerJumped += _jumpedAction;
        Boat_Controller.OnBoatMoved += _steeredAction;
        
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
        PlayerCharacter.OnPlayerJumped -= _jumpedAction;
        Boat_Controller.OnBoatMoved -= _steeredAction;
        
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
        // Initially set all canvas groups to be invisible
        foreach (var content in tutorialContents) content.CanvasGroup.alpha = 0f;

        // TODO: Match to the Game Manager to trigger on game start
        StartCoroutine(InputReaderRoutine());
    }

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
