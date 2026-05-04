using UnityEngine;
using UnityEngine.InputSystem;

public class Pause_Input : MonoBehaviour
{
    private InputAction _pauseAction;

    private void Awake()
    {
        var actionMap = InputSystem.actions.actionMaps[0];

        // The name of the pause input action
        _pauseAction = actionMap.FindAction("Pause");
        _pauseAction.performed += OnPause;
    }

    private void OnEnable()
    {
        _pauseAction.performed += OnPause;
    }

    private void OnDisable()
    {
        _pauseAction.performed -= OnPause;
    }

    private void OnPause(InputAction.CallbackContext context)
    {
        GameManager.GameLogic.TogglePauseState();
    }
}
