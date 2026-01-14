using EditorAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Boat_Space_Manager.BoatSide;

public abstract class CharacterStateController : MonoTimeBehaviour
{
    #region Variables

    [Header("Character Stats")]
    [Tooltip("How fast the character will step towards an open space.")]
    [SerializeField] protected float stepSpeed = 1f;
    [SerializeField] protected AnimationCurve _stepCurve;
    public AnimationCurve StepCurve { get { return _stepCurve; } }
    [Space(5)]
    [Tooltip("How fast the character will vault over the wall to the opposite side.")]
    [SerializeField] protected float vaultSpeed = 1f;
    [SerializeField] protected float vaultHeight = 1f;
    [SerializeField] protected AnimationCurve vaultHeightCurve;

    [Space(5)]

    [Tooltip("The rate of which the character falls")]
    [SerializeField] protected float fallSpeed = 1f;

    [Space(10)]

    [Tooltip("What space on the boat should the character start on (if applicable)")]
    [SerializeField] protected int startSpace = 1;
    [SerializeField] protected int startSide = 0;


    /// <summary> The data of the current space the player is on </summary>
    [SerializeField, ReadOnly] protected SpaceData _currentSpace;
    public SpaceData CurrentSpace { get { return _currentSpace; } }

    [Space(10)]

    [Tooltip("Determines if the character can move across boat spaces")]
    [SerializeField] protected bool _canMove = true;
    public bool CanMove { get { return _canMove; } set { _canMove = value; } }

        [Tooltip("Determines if the character can vault over the boat wall")]
        [SerializeField] protected bool _canVault = true;
        public bool CanVault { get { return _canVault; } set { _canVault = value; } }

            [Tooltip("Determines if the character can jump off the boat")]
            [SerializeField] protected bool _canJump = true;
            public bool CanJump { get { return _canJump; } set { _canJump = value; } }

    [Tooltip("Determines if the character is currently vaulting")]
    [SerializeField, ReadOnly] protected bool _isVaulting;
    public bool IsVaulting { get { return _isVaulting; } }

        [Tooltip("Determines if the character is currently vaulting heavily")]
        [SerializeField, ReadOnly] protected bool _isVaultingHeavily;
        public bool IsVaultingHeavily { get { return _isVaultingHeavily; } }

            [Tooltip("Determines if the characters vault is an attack")]
            [SerializeField, ReadOnly] protected bool _isVaultAttacking;
            public bool IsVaultAttacking { get { return _isVaultAttacking; } }

    [Tooltip("Determines if the character is currently moving")]
    [SerializeField, ReadOnly] protected bool _isMoving;
    public bool IsMoving { get { return _isMoving; } set { _isMoving = value; } }

        [Tooltip("Determines if the character is currently jumping")]
        [SerializeField, ReadOnly] protected bool _isJumping;
        public bool IsJumping { get { return _isJumping; } set { _isJumping = value; } }

    /// <summary> The Transform of the current targeted space on the boat. </summary>
    protected Transform _currentMoveTarget;

    [Tooltip("Current height of the character based off the distance to it and its current boat space")]
    [SerializeField] protected float currentHeight;
    [Tooltip("The value of the offset for the character stomping this character to be positioned at")]
    [SerializeField] float _headStompOffset;
    public float HeadStompOffset { get { return _headStompOffset; } }

    [Space]

    [SerializeField] protected LayerMask targetableMasks;

    [Header("Boat Interaction")]
    public bool CanInteractWithBoat;
    [ShowField(nameof(CanInteractWithBoat))] public Character_Boat_Interactor boatInteractor;
    [SerializeField] protected bool isOnBoat;

    [Space]

    [Tooltip("Determines whether the character can move to and stand on the outer spaces of the boat")]
    //[SerializeField] protected bool CanAccessOuterSides = false;
    public bool CanAccessOuterSides = false;
    [Tooltip("Determines whether the character can move to and stand on the spaces of the boat")]
    //[SerializeField] protected bool CanAccessBoatSides = true;
    public bool CanAccessBoatSides = true;

    [Header("Components")]
    [SerializeField] protected Rigidbody rb;

    [Tooltip("The Characters Health Component")]
    [SerializeField] CharacterHealth _characterHealth;
    public CharacterHealth HealthComponent { get { return _characterHealth; } }
    //protected Boat_Space_Manager boatSpaceManager;

    #endregion

    public IState CurrentState { get; private set; }
    public IState StoredState { get; private set; }

    public void ChangeState(IState newState)
    {
        CurrentState?.OnExit();
        CurrentState = newState;
        print($"New State: {CurrentState}");
        CurrentState.OnEnter();
    }

    public void StoreState(IState newState)
    {
        StoredState = newState;
    }

    #region Health
    public abstract void TookDamage();

    public abstract void Died();

    public abstract void HealthRestored();
    #endregion

    #region MonoTimeBehaviour Events
    // TODO: Transfer movement to state machine

    protected override void TimeUpdate()
    {
        CurrentState?.UpdateState();
    }
    protected override void FixedTimeUpdate() // TODO: Rework player Step Movement controls to utilise animation curves. Use lerp.
    {
        if (isOnBoat)
        {
            if (_isMoving && _canMove) transform.localPosition = StepMovement();
            else if (_isVaulting && _canVault) transform.localPosition = VaultMovement();
        }
        else if (_isJumping)
        {
            if (_isMoving && _canMove) transform.localPosition = StepMovement(); // If moving and is jumping, step towards the targetted space
            else // Else, just maintain current position and height
            {
                Vector3 currentPos = CurrentSpace.t.position;
                transform.position = new(currentPos.x, currentHeight, currentPos.z);
            }
        }
        else if (!IsGrounded())
        {
            // Target any targetted characters Y space position, else return this characters Y position
            float y = GetTargetCharacterYPos() + currentHeight;
            Vector3 pos = StepMovement();

            transform.localPosition = new Vector3(pos.x, y, pos.z);
            currentHeight = GetHeightFromSpace();
        }

        CurrentState?.FixedUpdateState();
    }
    #endregion

    #region Space Movement Logic

    void TransferToSpace(SpaceData newSpace)
    {
        if (_currentSpace != null) _currentSpace.isOccupied = false;

        _currentSpace = newSpace;
        _currentSpace.isOccupied = true;

        //_currentSpaceID = _currentSpace.spaceID;
        //_currentSideID = _currentSpace.sideID;
        _currentMoveTarget = _currentSpace.t;

        //Debug.Log("Got New Space Data!");
    }

    /// <summary> Moves the character to a space on the boat via a given side and space </summary>
    public void MoveToSpace(int side, int space)
    {
        SpaceData sd = Boat_Space_Manager.Instance.GetSpace(side, space);

        if (!Boat_Space_Manager.Instance.CheckSpaceAccess(CanAccessOuterSides, CanAccessBoatSides, sd))
        {
            return;
        }
        if (!_isVaulting && _canMove)
        {
            TransferToSpace(sd);
            _isMoving = true;
        }
    }

    /// <summary>
    /// Moves the character to a space on the boat via a given direction
    /// </summary>
    public void MoveToSpaceInDirection(int direction)
    {
        SpaceData sd = Boat_Space_Manager.Instance.GetSpaceFromDirection(_currentSpace.sideID, _currentSpace.spaceID, direction);

        if (Boat_Space_Manager.Instance.CheckSpaceAccess(CanAccessOuterSides, CanAccessBoatSides, sd) && !_isVaulting && _canMove)
        {
            //print($"Moving to Space: {sd.spaceID}");
            TransferToSpace(sd);
            _isMoving = true;
        }
        //else print($"Couldn't access space: {sd.spaceID}");
    }

    /// <summary> Vaults the character to a given side and space </summary>
    public void VaultToSpace(SpaceData spaceData, bool isHeavy = false)
    {
        if (!Boat_Space_Manager.Instance.CheckSpaceAccess(CanAccessOuterSides, CanAccessBoatSides, spaceData))
        {
            return;
        }
        else if (!_isVaulting && !_isMoving && _canVault && !_isJumping)
        {
            TransferToSpace(spaceData);
            _isVaulting = true;
            _isVaultingHeavily = isHeavy;
        }
    }

    protected CharacterStateController targetedCharacter;

    /// <summary> Vaults the character over the boat wall and onto a targetted character </summary>
    public void VaultOnCharacter(SpaceData spaceData, CharacterStateController target)
    {
        if (!Boat_Space_Manager.Instance.CheckSpaceAccess(CanAccessOuterSides, CanAccessBoatSides, spaceData))
        {
            return;
        }
        else if (!_isVaulting && !_isMoving && _canVault && !_isJumping)
        {
            TransferToSpace(spaceData);

            // Set move target to the Character target and disable its movement
            targetedCharacter = target;
            _currentMoveTarget = target.transform;
            target._canMove = false;

            _isVaulting = true;
            _isVaultAttacking = true;
        }
    }

    /// <summary> Sends the character directly to the position of the specified space on a given side </summary>
    public void GoToSpace(int side, int space)
    {
        SpaceData sd = Boat_Space_Manager.Instance.GetSpace(side, space);
        if (!Boat_Space_Manager.Instance.CheckSpaceAccess(CanAccessOuterSides, CanAccessBoatSides, sd))
        {
            return;
        }
        TransferToSpace(sd);

        if (isOnBoat) transform.localPosition = sd.t.localPosition;
        else transform.position = sd.t.position;
    }

    /// <summary> Sends the character directly to the position of the specified Side Space on a given space </summary>
    public void GoToSideSpace(int side, bool goLeftSide = true)
    {
        SpaceData sd = Boat_Space_Manager.Instance.GetSideSpace(side, goLeftSide);
        if (!Boat_Space_Manager.Instance.CheckSpaceAccess(CanAccessOuterSides, CanAccessBoatSides, sd))
        {
            return;
        }

        TransferToSpace(sd);


        // TODO: Consider this. Character might be off the boat if they're going to a side space
        if (isOnBoat) transform.localPosition = sd.t.localPosition;
        else transform.position = sd.t.position;
    }

    /// <summary> Sends the character directly to the position of the specified space on the Boat </summary>
    public void GoToBoatSpace(int side, int space)
    {
        SpaceData sd = Boat_Space_Manager.Instance.GetBoatSpace(side, space);
        if (!Boat_Space_Manager.Instance.CheckSpaceAccess(CanAccessOuterSides, CanAccessBoatSides, sd))
        {
            return;
        }

        TransferToSpace(sd);


        // TODO: Consider this. Character might be off the boat if they're going to a side space
        if (isOnBoat) transform.localPosition = sd.t.localPosition;
        else transform.position = sd.t.position;
    }

    /// <summary> Get this characters current space in the boat </summary>
    public SpaceData GetCurrentSpaceData()
    {
        return _currentSpace;
    }

    /// <summary> Returns whether or not the next space is available to go to </summary>
    public bool CheckAvailableSpaceFromDirection(int direction)
    {
        SpaceData sd = Boat_Space_Manager.Instance.GetSpaceFromDirection(_currentSpace.sideID, _currentSpace.spaceID, direction);
        print($"Checked space: {sd.spaceID}");
        return Boat_Space_Manager.Instance.CheckSpaceAccess(CanAccessOuterSides, CanAccessBoatSides, sd);
    }
    #endregion

    // TODO: Ground check is messing with the players position and is restricting vault movement.
    // Additionally, doesn't compare height to the heads of targeted characters
    bool IsGrounded() => Mathf.Abs(GetHeightFromSpace()) < 0.05f; // Mathf.Abs(GetCalculatedHeight()) < 0.05f;

    float GetHeightFromSpace()
    {
        return transform.localPosition.y - CurrentSpace.t.localPosition.y;
        // Vector3.Distance(transform.localPosition, _currentMoveTarget.localPosition);
    }

    float GetTargetCharacterYPos()
    {
        if (_isVaultAttacking && targetedCharacter != null)
            return targetedCharacter.transform.localPosition.y + targetedCharacter.HeadStompOffset;
        else
            return CurrentSpace.t.localPosition.y;
    }

    /// <summary>
    /// Method to make the character enter the boats parent
    /// </summary>
    public void EnterBoat(bool goToCurrentSpace)
    {
        Boat_Space_Manager.Instance.AddPassenger(this);
        isOnBoat = true;
        if (goToCurrentSpace) MoveToSpace(_currentSpace.sideID, _currentSpace.spaceID);
    }

    /// <summary>
    /// Method to make the character exit the boats parent
    /// </summary>
    public void ExitBoat(bool goToCurrentSpace)
    {
        Boat_Space_Manager.Instance.RemovePassenger(this);
        isOnBoat = false;
        if (goToCurrentSpace) MoveToSpace(_currentSpace.sideID, _currentSpace.spaceID);
    }

    #region Movement

    Vector3 StepMovement()
    {
        if (_currentMoveTarget == null)
        {
            Debug.LogWarning("Player is missing the current move target");
            return transform.localPosition;
        }

        Vector3 targetPosition;
        Vector3 movePosition;

        if (_isJumping) // (!IsGrounded())
        {
            targetPosition = _currentMoveTarget.position;
            targetPosition.y = currentHeight;

            movePosition = Vector3.MoveTowards(transform.localPosition, targetPosition, 
                stepSpeed * Time.fixedDeltaTime * GameManager.GameLogic.GamePauseInt);
            movePosition.y = currentHeight;
        }
        else
        {
            targetPosition = _currentMoveTarget.localPosition;
            movePosition = Vector3.MoveTowards(transform.localPosition, targetPosition, 
                stepSpeed * Time.fixedDeltaTime * GameManager.GameLogic.GamePauseInt);

            currentHeight = 0f;
        }

        float xDistance = Mathf.Abs(movePosition.x - targetPosition.x);
        if (xDistance < 0.05f)
        {
            _isMoving = false;
            return targetPosition;
        }
        else
            return movePosition;
    }

    // Vault tracking variables
    private Vector3 _vaultStartPosition = Vector3.zero;
    private float _vaultElapsedTime = 0f;
    private float _vaultTotalDistance = 0f;

    Vector3 VaultMovement()
    {
        if (_currentMoveTarget == null || (_isVaultAttacking && targetedCharacter == null))
        {
            _isVaulting = false;
            _vaultElapsedTime = 0f;
            _vaultTotalDistance = 0f;
            return Boat_Space_Manager.Instance.GetSpace(_currentSpace.sideID, _currentSpace.spaceID).t.localPosition;
        }

        // Stores the space on the opposite side. Gets the targeted characters head offset if atacking
        Vector3 targetPosition = _isVaultAttacking ?
            targetedCharacter.transform.localPosition + (Vector3.up * targetedCharacter._headStompOffset)
            : _currentMoveTarget.localPosition;

        // Track vault progress using a timer
        if (_vaultStartPosition == Vector3.zero)
        {
            _vaultStartPosition = transform.localPosition;
            _vaultElapsedTime = 0f;
        }
        _vaultTotalDistance = Vector3.Distance(_vaultStartPosition, targetPosition);

        _vaultElapsedTime += Time.fixedDeltaTime * GameManager.GameLogic.GamePauseInt;
        float progress = Mathf.Clamp01(_vaultElapsedTime * vaultSpeed / Mathf.Max(_vaultTotalDistance, 0.001f));

        // Evaluate height curve
        currentHeight = vaultHeightCurve.Evaluate(progress) * vaultHeight;

        // Check if reached target (ignoring height offset)
        if (progress >= 1f)
        {
            transform.position = targetPosition; // Snap to final position
            _isVaulting = false;
            _vaultStartPosition = Vector3.zero;
            _vaultElapsedTime = 0f;
            _vaultTotalDistance = 0f;
            currentHeight = 0f;

            VaultPerformed();
        }

        // Interpolate position
        Vector3 nextPosition = Vector3.Lerp(_vaultStartPosition, targetPosition, progress);
        nextPosition.y += currentHeight;

        return nextPosition;
    }

    protected virtual void VaultPerformed()
    {
        if (_isVaultAttacking) // If attacking, damage the character upon landing
        {
            _isVaultAttacking = false;
            targetedCharacter.HealthComponent.TakeDamage();

            BumpCharacter(); //TODO

            targetedCharacter = null;
        }
        if (_isVaultingHeavily && CanInteractWithBoat)
        {
            boatInteractor.ImpactBoat(_currentSpace.spaceID);
            _isVaultingHeavily = false;
        }
    }
    #endregion

    #region Bump Interaction

    [Header("Bump Stats")]
    [Tooltip("Multiplier for how high the character will be bumped upwards after stomping an enemy")]
    [SerializeField] float _bumpMultiplier = 1f;
    [Tooltip("The duration of the bump")]
    [SerializeField] float _bumpTime = 1.5f;
    [Tooltip("The curve animation of the bump")]
    [SerializeField] AnimationCurve _bumpCurve;

    /// <summary> Method to bump the character upwards </summary>
    public void BumpCharacter(float amount = 1f)
    {
        print($"{name} was bumped upwards!");

        // TODO: Play SFX Here
        if (isOnBoat) ExitBoat(false);

        StartCoroutine(BumpProcess(amount));
    }

    IEnumerator BumpProcess(float amount)
    {
        float elapsed = 0f;

        float offset = !isOnBoat ? transform.position.y : transform.localPosition.y;
        currentHeight = offset;

        WaitForFixedUpdate wait = new();

        // Bump upwards
        while (elapsed < _bumpTime)
        {
            elapsed += Time.fixedDeltaTime * GameManager.GameLogic.GamePauseInt;

            float t = Mathf.Clamp01(elapsed / _bumpTime);
            currentHeight = _bumpCurve.Evaluate(t) * (amount * _bumpMultiplier) + offset;

            yield return wait;
        }

        // Check for any enemies below the player
        //CurrentSpace.t.position
        CharacterStateController bc = CharacterSpaceChecks.ScanAreaForDamageableCharacter
            (transform.position, new(1, 10, 1), Quaternion.identity, targetableMasks, true, false); // Scan in a tall area

        // Determine target height. If an enemy is below the player whilst falling, target their HeadStompOffset height
        float targetHeight = bc == null ? CurrentSpace.t.position.y : bc.HeadStompOffset + CurrentSpace.t.position.y;

        // Fall
        while (currentHeight > targetHeight)
        {
            bc = CharacterSpaceChecks.ScanAreaForDamageableCharacter
                (CurrentSpace.t.position, new(1, 10, 1), Quaternion.identity, targetableMasks, true, false); // Scan in a tall area
            //if (_isMoving)
            //{
                targetHeight = bc == null? 
                    CurrentSpace.t.position.y : 
                    bc.HeadStompOffset + CurrentSpace.t.position.y;
            //}

            float f = fallSpeed * Time.fixedDeltaTime * GameManager.GameLogic.GamePauseInt;
            currentHeight -= f;

            if (currentHeight < targetHeight) // Prevents the player from sinking into the ground
                currentHeight = targetHeight;

            yield return wait;
        }

        currentHeight = targetHeight;

        EnterBoat(true);
    }
    #endregion
}
