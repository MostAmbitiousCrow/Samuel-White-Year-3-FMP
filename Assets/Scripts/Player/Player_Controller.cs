using UnityEngine;
using UnityEngine.InputSystem;

class Player_Controller : Boat_Character, IDamageable, IBoatSpaceMovement
{
    [Header("Player Stats")]

    #region Input Actions
    [Header("Input Actions")]
    public PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction vaultAction;
    private InputAction pauseAction;

    private void Awake()
    {
        var actionMap = playerInput.currentActionMap;
        moveAction = actionMap.FindAction("Move");
        vaultAction = actionMap.FindAction("Vault");
        pauseAction = actionMap.FindAction("Pause");

        moveAction.performed += OnMove;
        moveAction.canceled += OnMove;
        vaultAction.performed += OnVault;
        pauseAction.performed += OnPause;

        
        GoToSpace(0, startSpace);
    }

    private void OnEnable()
    {
        moveAction?.Enable();
        vaultAction?.Enable();
        pauseAction?.Enable();
    }

    private void OnDisable()
    {
        moveAction?.Disable();
        vaultAction?.Disable();
        pauseAction?.Disable();
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        Vector2 direction = context.ReadValue<Vector2>();
        // Handle movement logic here
        MoveToSpace(Mathf.RoundToInt(direction.x), stepSpeed);
    }

    private void OnVault(InputAction.CallbackContext context)
    {
        // Vault logic
        if (_isVaulting)
        {
            // Trigger Jump Upon Landing Logic Here
            return;
        }
        if (context.performed)
        {
            _isVaulting = true;
            VaultToSpace(Boat_Space_Manager.GetOppositeLaneID(GetCurrentLane()), GetCurrentSpace(), vaultSpeed);
            _isVaulting = false; //TODO: Implement vaulting animation here
        }
    }

    private void OnPause(InputAction.CallbackContext context)
    {
        // Handle pause logic here
    }
    #endregion

    #region Damage Logic
    public void TakeDamage(int amount)
    {
        print($"{name} Took Damage");
        // Damage Logic Here
    }
    #endregion
}
