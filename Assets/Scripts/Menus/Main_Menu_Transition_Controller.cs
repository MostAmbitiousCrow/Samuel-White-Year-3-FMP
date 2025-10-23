using System;
using System.Collections;
using UnityEngine;
using EditorAttributes;

public class Main_Menu_Transition_Controller : MonoBehaviour
{
    // Delegates and Status variables
    public bool IsTransitioning { get { return _isTransitioning; } }

    /// <summary> Called whenever the transition has started </summary>
    public static event Action<MainMenuScreenContent> OnTransitionStarted;
    /// <summary> Called whenever the transition has begun waiting </summary>
    public static event Action<MainMenuScreenContent, MainMenuScreenContent> OnTransitionWaiting;
    /// <summary> Called whenever the transition has finished waiting </summary>
    public static event Action<MainMenuScreenContent> OnTransitionWaitCompleted;
    /// <summary> Called whenever the transition has been completed </summary>
    public static event Action OnTransitionCompleted;

    [Header("Transition Settings")]
    [SerializeField] bool _isTransitioning;
    [Space]
    [SerializeField] bool _doTransitionAnimation;
    [Space]
    [SerializeField, ShowField(nameof(_doTransitionAnimation))] float _transitionStartTime = .1f;
    [SerializeField, ShowField(nameof(_doTransitionAnimation))] float _transitionWaitTime = .3f;
    [SerializeField, ShowField(nameof(_doTransitionAnimation))] float _transitionEndTime = .1f;

    [Header("Dependency")]
    [SerializeField] Main_Menu_Manager _mainMenuManager;

    /// <summary>
    /// Method to trigger the screen transition
    /// </summary>
    /// <param name="screenToOpen"> The Screen contents to enable (open) </param>
    /// <param name="screenToClose"> The Screen contents to disable (close)</param>
    /// <param name="multiplier"> Optional. Multiply the speed of the transition</param>
    public void TriggerTransition(MainMenuScreenContent screenToOpen, MainMenuScreenContent screenToClose,
        bool doTransition = true, float multiplier = 1f)
    {
        if(doTransition && _doTransitionAnimation) StartCoroutine(TransitionRoutine(screenToOpen, screenToClose, multiplier));
        else
        {
            OnTransitionStarted.Invoke(screenToOpen);
            OnTransitionWaiting.Invoke(screenToOpen, screenToClose);
            OnTransitionWaitCompleted(screenToOpen);
            OnTransitionCompleted?.Invoke();
        }
    }

    IEnumerator TransitionRoutine(MainMenuScreenContent screenToOpen, MainMenuScreenContent screenToClose, float speed = 1f)
    {
        _isTransitioning = true;

        // Trigger Transition Started delegate
        OnTransitionStarted.Invoke(screenToOpen);

        float t = 0f;
        while (t * speed < _transitionStartTime)
        {
            // Insert your Start Transition Animation code here

            yield return t += Time.deltaTime;
        }

        t = 0f;
        OnTransitionWaiting.Invoke(screenToOpen, screenToClose);
        while (t * speed < _transitionWaitTime)
        {
            // Insert your Transition Animation code here

            yield return t += Time.deltaTime;
        }
        OnTransitionWaitCompleted.Invoke(screenToOpen);

        t = 0f;
        while (t * speed < _transitionEndTime)
        {
            // Insert your End Transition Animation code here

            yield return t += Time.deltaTime;
        }

        // Trigger Transition Completed delegate
        OnTransitionCompleted?.Invoke();

        _isTransitioning = false;

        yield break;
    }
}
