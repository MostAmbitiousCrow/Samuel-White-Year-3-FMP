using UnityEngine;
using UnityEngine.InputSystem;

class Player_Controller : Boat_Character, IDamageable
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
        // moveAction.canceled += OnMove;
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

    private void OnMove(InputAction.CallbackContext context) //TODO: Rework to allow the player to simply hold down the move button to continue moving in that direction or tap to move a single space
    // Additionally, fix the issue where the player is able to trigger the move event when pressing and releasing an additional key (or perhaps rework movement to use buttons instead?)
    {
        // Handle movement logic here
        int direction = Mathf.RoundToInt(context.ReadValue<Vector2>().x);
        MoveToSpace(Mathf.RoundToInt(direction), stepSpeed);
    }

    /// <summary>
    /// The Vault Player Input Action Function
    /// </summary>
    private void OnVault(InputAction.CallbackContext context)
    {
        // Vault logic
        if (_isVaulting)
        {
            //TODO: Trigger Jump Upon Landing Logic Here
            return;
        }
        if (context.performed)
        {
            _isVaulting = true;
            VaultToSpace(Boat_Space_Manager.GetOppositeLaneID(GetCurrentLane()), GetCurrentSpace(), vaultSpeed);
            _isVaulting = false; //TODO: Implement vaulting animation here
        }
    }

    private void OnVaultJump(InputAction.CallbackContext context)
    {
        print("Jumped");
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
