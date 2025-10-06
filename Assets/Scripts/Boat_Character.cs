using UnityEngine;
using EditorAttributes;

[RequireComponent(typeof(Rigidbody))]
public class Boat_Character : MonoBehaviour, IBoatSpaceMovement
// Boat Characters are any characters that are able to interact and move with the players boat
{
    [Header("Character Stats")]
    [Tooltip("How fast the character will step towards an open space.")]
    [SerializeField] protected float stepSpeed = 1f;
    [Space(5)]
    [Tooltip("How fast the character will vault over the wall to the opposite lane.")]
    [SerializeField] protected float vaultSpeed = 1f;
    [SerializeField] protected float vaultHeight = 1f;
    [SerializeField] protected AnimationCurve vaultHeightCurve;
    [Space(10)]
    [Tooltip("The current space on the boat the character is standing on")]
    [SerializeField] protected int _currentSpace;
    [Tooltip("The current lane of the boat the character is standing on")]
    [SerializeField] protected int _currentLane;
    [Tooltip("What space on the boat should the character start on (if applicable)")]
    public int startSpace = 1;
    [Space(10)]

    [SerializeField] protected bool _canMove = true;
    [SerializeField] protected bool _canVault = true;
    [SerializeField] protected bool _canJump = true;
    [SerializeField] protected bool _isVaulting;
    [SerializeField] protected bool _isMoving;
    [SerializeField] protected bool _isJumping;

    /// <summary>
    /// The Transform of the current targeted space on the boat.
    /// </summary> <summary>
    /// 
    /// </summary>
    protected Transform _currentMoveTarget;
    /// <summary>
    /// Current height of the character based off the distance to it and its current boat space
    /// </summary>
    [SerializeField] protected float currentHeight;

    [Header("Boat Interaction")]
    public bool canInteractWithBoat;
    [ShowField(nameof(canInteractWithBoat))] public Character_Boat_Interactor boatInteractor;
    [SerializeField] protected bool isOnBoat;

    [Header("Components")]
    [SerializeField] protected Rigidbody rb;
    protected Boat_Space_Manager boatSpaceManager;

    #region Space Movement Logic

    public void MoveToSpace(int direction, float speed)
    {
        if (!_isVaulting && _canMove)
        {
            Boat_Space_Manager.BoatSide.SpaceData sd = boatSpaceManager.GetSpaceFromDirection(_currentLane, _currentSpace, direction);
            _currentSpace = sd.ID;
            _currentMoveTarget = sd.t;
            _isMoving = true;
        }
    }

    public void VaultToSpace(int lane, int space, float speed)
    {
        if (!_isVaulting && !_isMoving && _canVault && IsGrounded())
        {
            Boat_Space_Manager.BoatSide.SpaceData sd = boatSpaceManager.GetSpace(lane, space);
            _currentSpace = sd.ID;
            _currentLane = boatSpaceManager.GetOppositeLaneID(_currentLane);
            _currentMoveTarget = sd.t;
            _isVaulting = true;
        }
    }

    public void GoToSpace(int lane, int space)
    {
        Boat_Space_Manager.BoatSide.SpaceData sd = boatSpaceManager.GetSpace(lane, space);
        _currentSpace = sd.ID;
        _currentMoveTarget = sd.t;
        if (isOnBoat) transform.localPosition = sd.t.localPosition;
        else transform.position = sd.t.position;
    }

    public int GetCurrentSpace()
    {
        return _currentSpace;
    }

    public int GetCurrentLane()
    {
        return _currentLane;
    }
    #endregion

    bool IsGrounded() => Mathf.Abs(currentHeight) < 0.05f;
    float GetCalculatedHeight() => Vector3.Distance(transform.localPosition, _currentMoveTarget != null ? _currentMoveTarget.localPosition : Vector3.zero);

    public void EnterBoat(bool goToCurrentSpace)
    {
        boatSpaceManager.AddPassenger(this);
        isOnBoat = true;
        if (goToCurrentSpace) MoveToSpace(_currentLane, _currentSpace);
    }

    public void ExitBoat(bool goToCurrentSpace)
    {
        boatSpaceManager.RemovePassenger(this);
        isOnBoat = false;
        if (goToCurrentSpace) MoveToSpace(_currentLane, _currentSpace);
    }

    #region Movement

    void FixedUpdate()
    {
        if (isOnBoat)
        {
            if (_isMoving && _canMove) transform.localPosition = StepMovement();
            else if (_isVaulting && _canVault) transform.localPosition = VaultMovement();
        }
        else if (_isJumping)
        {
            if (_isMoving && _canMove) transform.localPosition = StepMovement();
            else
            {
                Vector3 currentPos = _currentMoveTarget.position;
                transform.position = new(currentPos.x, currentHeight, currentPos.z);
                // print("Jumping");
            }
        }

    }

    Vector3 StepMovement()
    {
        if (_currentMoveTarget == null) return transform.localPosition;

        Vector3 targetPosition;
        Vector3 movePosition;

        if (!IsGrounded())
        {
            targetPosition = _currentMoveTarget.position;
            targetPosition.y = currentHeight;

            movePosition = Vector3.MoveTowards(transform.localPosition, targetPosition, stepSpeed * Time.fixedDeltaTime);
            movePosition.y = currentHeight;
        }
        else
        {
            targetPosition = _currentMoveTarget.localPosition;
            movePosition = Vector3.MoveTowards(transform.localPosition, targetPosition, stepSpeed * Time.fixedDeltaTime);
        }

        float xDistance = Mathf.Abs(movePosition.x - targetPosition.x);
        if (xDistance < 0.01f)
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
        if (_currentMoveTarget == null)
        {
            _isVaulting = false;
            _vaultElapsedTime = 0f;
            _vaultTotalDistance = 0f;
            return boatSpaceManager.GetSpace(_currentLane, _currentSpace).t.localPosition;
        }

        Vector3 targetPosition = _currentMoveTarget.localPosition; // Stores the space on the opposite lane

        // Track vault progress using a timer
        if (_vaultStartPosition == Vector3.zero)
        {
            _vaultStartPosition = transform.localPosition;
            _vaultElapsedTime = 0f;
        }
        _vaultTotalDistance = Vector3.Distance(_vaultStartPosition, targetPosition);

        _vaultElapsedTime += Time.fixedDeltaTime;
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
            if (canInteractWithBoat)
            {
                boatInteractor.ImpactBoat(_currentSpace);
            }
        }

        // Interpolate position
        Vector3 nextPosition = Vector3.Lerp(_vaultStartPosition, targetPosition, progress);
        nextPosition.y += currentHeight;

        return nextPosition;
    }
    #endregion

    #region Injection
    public void InjectBoatSpaceManager(Boat_Space_Manager manager)
    {
        boatSpaceManager = manager;
    }
    #endregion
}
