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
    [SerializeField] protected AnimationCurve vaultSpeedCurve;
    [SerializeField] protected float vaultHeight = 1f;
    [SerializeField] protected AnimationCurve vaultHeightCurve;
    [Space(10)]
    [Tooltip("The current space on the boat the character is standing on")]
    [SerializeField] protected int _currentSpace;
    [Tooltip("The current lane of the boat the character is standing on")]
    [SerializeField] protected int _currentLane;
    [Tooltip("What space on the boat should the character start on (if applicable)")]
    public int startSpace = 1;

    [SerializeField] protected bool _isVaulting;
    [SerializeField] protected bool _isMoving;

    protected Vector3 _currentMoveTarget;

    [Header("Boat Interaction")]
    public bool canInteractWithBoat;
    [ShowField(nameof(canInteractWithBoat))] public Character_Boat_Interactor boatInteractor;

    [Header("Components")]
    [SerializeField] protected Rigidbody rb;
    protected Boat_Space_Manager boatSpaceManager;

    #region Space Movement Logic

    public void MoveToSpace(int direction, float speed)
    {
        if (!_isVaulting)
        {
            Boat_Space_Manager.BoatSide.SpaceData sd = boatSpaceManager.GetSpaceFromDirection(_currentLane, _currentSpace, direction);
            _currentSpace = sd.ID;
            _currentMoveTarget = sd.position;
            _isMoving = true;
            // print($"Moved {direction} to Space Position: {sd.position}, ID {sd.ID}");
        }

    }

    public void VaultToSpace(int lane, int space, float speed)
    {
        if (!_isVaulting && !_isMoving)
        {
            Boat_Space_Manager.BoatSide.SpaceData sd = boatSpaceManager.GetSpace(lane, space);
            _currentSpace = sd.ID;
            _currentLane = boatSpaceManager.GetOppositeLaneID(_currentLane);
            _currentMoveTarget = sd.position; //TODO: Add vaulting movement interpolation
            _isVaulting = true;
        }
    }

    public void GoToSpace(int lane, int space)
    {
        Boat_Space_Manager.BoatSide.SpaceData sd = boatSpaceManager.GetSpace(lane, space);
        _currentSpace = sd.ID;
        _currentMoveTarget = sd.position;
        transform.localPosition = sd.position;
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

    #region Updates

    void FixedUpdate()
    {
        if (!_isVaulting && _isMoving) StepMovement();
        else if (!_isMoving && _isVaulting) VaultMovement();
    }
    private void Update()
    {
        if (_isMoving || _isVaulting) return;
        rb.position = _currentMoveTarget + boatSpaceManager.GetBoatCentre();
    }

    void StepMovement()
    {
        if (_currentMoveTarget != null)
        {
            Vector3 targetPosition = _currentMoveTarget + boatSpaceManager.GetBoatCentre();
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, stepSpeed * Time.fixedDeltaTime);
            if (transform.position.sqrMagnitude == targetPosition.sqrMagnitude)
            {
                _isMoving = false;
            }
        }
    }

    // Vault tracking variables
    private Vector3 _vaultStartPosition = Vector3.zero;
    private float _vaultElapsedTime = 0f;
    private float _vaultTotalDistance = 0f;
    
    void VaultMovement()
    {
        if (_currentMoveTarget != null)
        {
            Vector3 boatCentre = boatSpaceManager.GetBoatCentre();
            Vector3 targetPosition = _currentMoveTarget + boatCentre;

            // Track vault progress using a timer
            if (_vaultStartPosition == Vector3.zero)
            {
                _vaultStartPosition = transform.position;
                _vaultElapsedTime = 0f;
                _vaultTotalDistance = Vector3.Distance(_vaultStartPosition, targetPosition);
            }

            _vaultElapsedTime += Time.fixedDeltaTime;
            float progress = Mathf.Clamp01(_vaultElapsedTime * vaultSpeed / Mathf.Max(_vaultTotalDistance, 0.001f));

            // Evaluate height curve
            float heightOffset = vaultHeightCurve.Evaluate(progress) * vaultHeight;

            // Interpolate position
            Vector3 nextPosition = Vector3.Lerp(_vaultStartPosition, targetPosition, progress);
            nextPosition.y += heightOffset;

            transform.position = nextPosition;

            // Check if reached target (ignoring height offset)
            if (progress >= 1f)
            {
                transform.position = targetPosition; // Snap to final position
                _isVaulting = false;
                _vaultStartPosition = Vector3.zero;
                _vaultElapsedTime = 0f;
                _vaultTotalDistance = 0f;
                if (canInteractWithBoat)
                {
                    boatInteractor.ImpactBoat(_currentSpace);
                }
            }
        }
    }
    #endregion

    #region Injection
    public void InjectBoatSpaceManager(Boat_Space_Manager manager)
    {
        boatSpaceManager = manager;
    }
    #endregion
}
