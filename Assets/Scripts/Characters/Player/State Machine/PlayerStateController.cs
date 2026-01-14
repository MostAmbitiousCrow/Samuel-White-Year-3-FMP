using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerStateController : CharacterStateController
{
    #region Variables
    [Header("Player Stats")]

    //[SerializeField] LayerMask _enemyMask;

    #endregion

    #region Input Actions
    [Header("Input Actions")]
    public PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction vaultAction;
    private InputAction vaultHeavyAction;
    //private InputAction vaultJumpAction;

    private void Awake()
    {
        var actionMap = playerInput.currentActionMap;
        moveAction = actionMap.FindAction("Move");
        moveAction.performed += OnMove;

        vaultAction = actionMap.FindAction("Vault");
        vaultHeavyAction = actionMap.FindAction("VaultHeavy");

        // On Press trigger Vault
        vaultHeavyAction.started += OnVaultHeavy;
        vaultAction.started += OnVault;

        // Jump Performed
        vaultAction.performed += OnVaultJump;
        vaultHeavyAction.performed += OnVaultJump;
    }

    private void Start()
    {
        EnterBoat(true);

        GoToSpace(startSide, startSpace);
    }

    private void OnEnable()
    {
        moveAction?.Enable();
        vaultAction?.Enable();
        vaultHeavyAction?.Enable();

        if (GameManager.Instance != null) GameManager.GameLogic.OnGemstoneCollected += GemstoneCollected;
    }

    private void OnDisable()
    {
        moveAction?.Disable();
        vaultAction?.Disable();
        vaultHeavyAction?.Disable();

        if (GameManager.Instance != null) GameManager.GameLogic.OnGemstoneCollected -= GemstoneCollected;
    }

    private void Update()
    {
        //TODO: Temporary way of controlling timescale for playtesting purposes
        if (Input.GetKeyDown(KeyCode.Alpha1))
            Time.timeScale = 0f;
        if (Input.GetKeyDown(KeyCode.Alpha2))
            Time.timeScale = .25f;
        if (Input.GetKeyDown(KeyCode.Alpha3))
            Time.timeScale = .5f;
        if (Input.GetKeyDown(KeyCode.Alpha4))
            Time.timeScale = 1f;
        if (Input.GetKeyDown(KeyCode.Alpha5))
            Time.timeScale = 2f;
    }

    private void OnMove(InputAction.CallbackContext context) //TODO: Rework to allow the player to simply hold down the move button to continue moving in that direction or tap to move a single space
    // Additionally, fix the issue where the player is able to trigger the move event when pressing and releasing an additional key (or perhaps rework movement to use buttons instead?)
    {
        if (GameManager.GameLogic.GamePaused) return;

        // Handle movement logic here
        int direction = Mathf.RoundToInt(context.ReadValue<Vector2>().x);

        // Note: Inverting the direction since the order of the boat spaces are flipped...
        //MoveToSpace(Mathf.RoundToInt(direction * -1), _currentSpace);
        MoveToSpaceInDirection(Mathf.RoundToInt(direction * -1));
    }

    /// <summary>
    /// The Vault Player Input Action Function
    /// </summary>
    private void OnVault(InputAction.CallbackContext context)
    {
        if (GameManager.GameLogic.GamePaused) return;

        // Vault logic
        if (_isVaultingHeavily || _isVaulting)
        {
            //TODO: Trigger Jump Upon Landing Logic Here

            return;
        }
        else
        {
            //print("Player Vaulted");
            Boat_Space_Manager.BoatSide.SpaceData newSpace = Boat_Space_Manager.Instance.GetSpaceFromOppositeLane(_currentSpace.sideID, _currentSpace.spaceID);

            // Vault to space. Additionally, if an enemy is on the opposite side of the space, do an attack vault
            CharacterStateController bc = CharacterSpaceChecks.ScanAreaForDamageableCharacter
                (newSpace.t.position, Vector3.one, Quaternion.identity, targetableMasks, true, false);
            if (bc != null)
            {
                VaultOnCharacter(newSpace, bc);
            }
            else
            {
                VaultToSpace(newSpace);
            }
            //TODO: Implement vaulting animation here?
        }
        print("Performed Light Vault");
    }

    private void OnVaultHeavy(InputAction.CallbackContext context)
    {
        if (GameManager.GameLogic.GamePaused) return;

        // Vault logic
        if (_isVaultingHeavily || _isVaulting)
        {
            //TODO: Trigger Jump Upon Landing Logic Here

            return;
        }
        else
        {
            //print("Player Vaulted");
            Boat_Space_Manager.BoatSide.SpaceData newSpace = Boat_Space_Manager.Instance.GetSpaceFromOppositeLane(_currentSpace.sideID, _currentSpace.spaceID);

            // Vault to space. Additionally, if an enemy is on the opposite side of the space, do an attack vault
            CharacterStateController bc = CharacterSpaceChecks.ScanAreaForDamageableCharacter
                (newSpace.t.position, Vector3.one, Quaternion.identity, targetableMasks, true, false);
            if (bc != null)
            {
                VaultOnCharacter(newSpace, bc);
            }
            else
            {
                VaultToSpace(newSpace, true);
            }
            //TODO: Implement vaulting animation here?
        }
        print("Performed Heavy Vault");
    }

    private void OnVaultJump(InputAction.CallbackContext context)
    {
        if (GameManager.GameLogic.GamePaused || !_isVaulting || _isJumping) return;

        // print("Player Jumped");
        _jumpRoutine = StartCoroutine(VaultJump());
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
    //[Space(5)]
    //[Tooltip("The rate of which the player falls after the sustain process")]
    //[SerializeField] float _fallRate = 1f;

    /// <summary> The routine for the jumping process of the player </summary>
    public IEnumerator VaultJump()
    {
        float elapsed = 0f;
        _canMove = false;

        WaitForFixedUpdate wait = new();
        yield return new WaitUntil(() => !_isVaulting); //  Wait until the vaulting process has stopped

        float offset = transform.localPosition.y;
        currentHeight = offset;


        //TODO: Jump Prepare Animation
        while (elapsed < _jumpDelay)
        {
            elapsed += Time.fixedDeltaTime * GameManager.GameLogic.GamePauseInt;
            yield return wait;
        }

        ExitBoat(false);
        _isJumping = true;
        _canMove = true;

        // Jump Up
        elapsed = 0f;
        while (elapsed < _jumpTime)
        {
            float t = Mathf.Clamp01(elapsed / _jumpTime);
            currentHeight = _jumpCurve.Evaluate(t) * _jumpHeight + offset;

            elapsed += Time.fixedDeltaTime * GameManager.GameLogic.GamePauseInt;

            yield return wait;
        }

        // Sustain
        elapsed = 0f;
        while (elapsed < _jumpSustainTime)
        {
            float t = Mathf.Clamp01(elapsed / _jumpSustainTime);
            currentHeight = Mathf.Lerp(_jumpHeight, _jumpSustainHeight, _jumpSustainCurve.Evaluate(t)) + offset;

            elapsed += Time.fixedDeltaTime * GameManager.GameLogic.GamePauseInt;

            yield return wait;
        }

        // Check for any enemies below the player
        CharacterStateController bc = CharacterSpaceChecks.ScanAreaForDamageableCharacter
            (_currentMoveTarget.position, new(1, 10, 1), Quaternion.identity, targetableMasks, true, false); // Scan in a tall area

        // Determine target height. If an enemy is below the player whilst falling, target their HeadStompOffset height
        float targetHeight = bc == null ? _currentMoveTarget.position.y : bc.HeadStompOffset + _currentMoveTarget.position.y;

        // Fall
        while (currentHeight > targetHeight)
        {
            if (_isMoving)
            {
                bc = CharacterSpaceChecks.ScanAreaForDamageableCharacter
                    (_currentMoveTarget.position, new(1, 10, 1), Quaternion.identity, targetableMasks, true, false); // Scan in a tall area

                if (bc == null) targetHeight = 0f;
                else targetHeight = bc.HeadStompOffset;
            }

            float f = fallSpeed * Time.fixedDeltaTime * GameManager.GameLogic.GamePauseInt;
            currentHeight -= f;

            if (currentHeight < targetHeight) // Prevents the player from sinking into the ground
                currentHeight = targetHeight;

            yield return wait;
        }

        currentHeight = targetHeight;

        //// Temporary fix for the player getting stuck at y pos 0 after the jump
        //Vector3 localPos = transform.localPosition;
        //transform.localPosition = new(localPos.x, bc == null ? _currentMoveTarget.localPosition.y : targetHeight, localPos.z);

        _isJumping = false;
        EnterBoat(false);
    }

    /// <summary> Cancels the jump (for whatever reason) </summary>
    public void CancelJump()
    {
        //TODO
    }

    #endregion

    #region Gemstone Events

    void GemstoneCollected(int amount)
    {
        // TODO: Gemstone Collected, Trigger some sort of effect
    }

    #endregion
    public override void Died()
    {
        Debug.Log("PLAYER DIED");
    }

    public override void HealthRestored()
    {
        //throw new System.NotImplementedException();
    }

    public override void TookDamage()
    {
        //throw new System.NotImplementedException();
    }
}
