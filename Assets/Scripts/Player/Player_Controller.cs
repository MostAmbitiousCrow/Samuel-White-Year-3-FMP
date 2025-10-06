using System.Collections;
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
    private InputAction vaultJumpAction;
    private InputAction pauseAction;

    private void Awake()
    {
        var actionMap = playerInput.currentActionMap;
        moveAction = actionMap.FindAction("Move");
        vaultAction = actionMap.FindAction("Vault");
        vaultJumpAction = actionMap.FindAction("VaultJump");
        pauseAction = actionMap.FindAction("Pause");

        moveAction.performed += OnMove;
        // moveAction.canceled += OnMove;
        vaultAction.performed += OnVault;
        vaultJumpAction.performed += OnVaultJump;
        pauseAction.performed += OnPause;

        _currentSpace = startSpace;
    }

    private void Start()
    {
        EnterBoat(true);
    }

    private void OnEnable()
    {
        moveAction?.Enable();
        vaultAction?.Enable();
        vaultJumpAction?.Enable();
        pauseAction?.Enable();

        if(GameManager.Instance != null) GameManager.Instance.GameLogic.onGemstoneCollected += GemstoneCollected;
    }

    private void OnDisable()
    {
        moveAction?.Disable();
        vaultAction?.Disable();
        vaultJumpAction?.Disable();
        pauseAction?.Disable();

        if(GameManager.Instance != null) GameManager.Instance.GameLogic.onGemstoneCollected -= GemstoneCollected;
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
        else
        {
            // print("Player Vaulted");
            VaultToSpace(boatSpaceManager.GetOppositeLaneID(GetCurrentLane()), GetCurrentSpace(), vaultSpeed);
            //TODO: Implement vaulting animation here
        }
    }

    private void OnVaultJump(InputAction.CallbackContext context)
    {
        if (!_isVaulting) return;

        print("Player Jumped");
        _jumpRoutine = StartCoroutine(VaultJump());
        // InvokeRepeating(nameof(VaultJump), 0f, Time.fixedDeltaTime);
    }

    private void OnPause(InputAction.CallbackContext context)
    {
        // Handle pause logic here
    }
    #endregion

    #region Vault Jump Action

    [Header("Jump Settings")]
    private Coroutine _jumpRoutine;

    [Tooltip("The delay before jumping")]
    [SerializeField] float _jumpDelay = .1f;
    [Space(5)]
    [Tooltip("The height of the jump")]
    [SerializeField] float _jumpHeight = 5f;
    [Tooltip("The time to reach the max height of the jump")]
    [SerializeField] float _jumpTime = 2f;
    [Tooltip("The curve representing the process of the jump")]
    [SerializeField] AnimationCurve _jumpCurve;
    [Space(5)]
    [Tooltip("Jump sustain height")]
    [SerializeField] float _jumpSustainHeight = 0f;
    [Tooltip("The time the player remains in the air after the jump")]
    [SerializeField] float _jumpSustainTime = .2f;
    [Tooltip("The curve representing the sustain process of the jump")]
    [SerializeField] AnimationCurve _jumpSustainCurve;
    [Space(5)]
    [Tooltip("The rate of which the player falls after the sustain process")]
    [SerializeField] float _fallRate = 1f;

    public IEnumerator VaultJump()
    {
        float elapsed = 0f;
        _canMove = false;

        //TODO: Jump Prepare Animation
        yield return new WaitForSeconds(_jumpDelay);
        ExitBoat(false);
        _isJumping = true;
        _canMove = true;

        // Jump Up
        while (elapsed < _jumpTime)
        {
            elapsed += Time.fixedDeltaTime;
            float t = Mathf.Clamp01(elapsed / _jumpTime);

            currentHeight = _jumpCurve.Evaluate(t) * _jumpHeight;
            yield return new WaitForFixedUpdate();
        }

        // Sustain
        elapsed = 0f;
        while (elapsed < _jumpSustainTime)
        {
            elapsed += Time.fixedDeltaTime;
            float t = Mathf.Clamp01(elapsed / _jumpSustainTime);

            currentHeight = Mathf.Lerp(_jumpHeight, _jumpSustainHeight, _jumpSustainCurve.Evaluate(t));
            yield return new WaitForFixedUpdate();
        }

        // Fall
        while (currentHeight > 0f)
        {
            currentHeight -= _fallRate * Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        currentHeight = 0f;
        _isJumping = false;
        EnterBoat(false);

        // Temporary fix for the player getting stuck at y pos 0 after the jump
        Vector3 localPos = transform.localPosition;
        transform.localPosition = new(localPos.x, _currentMoveTarget.localPosition.y, localPos.z);
    }

    public void CancelJump()
    {
        //TODO
    }

    #endregion

    #region Damage Events

    [Header("Health Stats")]
    [SerializeField] int _currentHealth;
    [SerializeField] int _maxHealth;
    public int CurrentHealth
    {
        get { return (_currentHealth); }
        set { _currentHealth = value; }
    }
    public int MaxHealth
    {
        get { return (_maxHealth); }
        set { _maxHealth = value; }
    }

    [Header("Events")]
    [SerializeField] IDamageable[] damageableEvents;

    public void TakeDamage(float amount)
    {
        int newamount = Mathf.RoundToInt(amount);
        CurrentHealth -= newamount;
        // TODO: Damage Effect and Animation

        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        // Death Event / Game Over, Effect and Animation

    }

    public void RestoreHealth()
    {
        CurrentHealth = MaxHealth;
    }

    #endregion

    #region Gemstone Events

    void GemstoneCollected(int amount)
    {
        // TODO: Gemstone Collected, Trigger some sort of effect
    }

    #endregion
}
