using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class Pause_Input : MonoBehaviour
{
    private PlayerInput playerInput;
    private InputAction pauseAction;

    private void Awake()
    {
        if (!playerInput) playerInput = GetComponent<PlayerInput>();

        var actionMap = playerInput.currentActionMap;

        pauseAction = actionMap.FindAction("Pause");
        pauseAction.performed += OnPause;
    }

    private void OnEnable()
    {
        pauseAction.performed += OnPause;
    }

    private void OnDisable()
    {
        pauseAction.performed -= OnPause;
    }

    void OnPause(InputAction.CallbackContext context)
    {
        GameManager.GameLogic.TogglePauseState();
    }
}
