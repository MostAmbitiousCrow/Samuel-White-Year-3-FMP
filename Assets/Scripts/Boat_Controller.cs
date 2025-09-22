using UnityEngine;

public class Boat_Controller : MonoBehaviour, IRiverLaneMovement, IDamageable, IAffectedByRiver
{
    [Header("Boat Settings")]
    public float steerSpeed = 1;
    public AnimationCurve SteerInterpolationCurve;
    [Space(10)]
    [Tooltip("The current lane of the boat the character is standing on")]
    [SerializeField] int _currentLane;
    [Tooltip("What lane should this object start on? (if applicable)")]
    public int startLane = 1;
    [Space(10)]
    [Tooltip("The duration of the stun after hitting an obstacle")]
    [SerializeField] float stunDuration = 1.5f;
    [Tooltip("How much of the current boats speed is decreased when an obstacle is hit")]
    [SerializeField] float stunSlowMultiplier = .5f;
    [Space(10)]
    [SerializeField] bool _isMoving;
    private Vector3 _currentMoveTarget;

    [Header("Components")]
    [SerializeField] Rigidbody rb;

    private River_Manager riverManager;
    [SerializeField] private Boat_Space_Manager boatSpaceManager;

    private void Start()
    {
        GoToLane(startLane);
    }

    void Awake()
    {
        if (boatSpaceManager == null)
            Debug.LogError("Missing Boat_Space_Manager component");

        RestoreHealth();
    }

    /// <summary>
    /// The main method to steer the players boat in a given direction and animates the force.
    /// </summary>
    public void SteerBoat(int direction, float force)
    {
        print($"Steered Board in the {direction} direction");
        MoveToLane(direction);
    }

    public void MoveToLane(int direction)
    {
        River_Manager.RiverLane rl = riverManager.GetLaneFromDirection(_currentLane, direction);

        _currentLane = rl.ID;
        _currentMoveTarget = new Vector3(rl.axis.x, rl.axis.y, transform.position.z); //TODO: Add movement interpolation
        _isMoving = true;
    }

    public void GoToLane(int lane)
    {
        River_Manager.RiverLane rl = riverManager.GetLane(lane);

        _currentLane = rl.ID;
        transform.position = new(rl.axis.x, rl.axis.y, transform.position.z);
    }

    public int GetCurrentLane()
    {
        return _currentLane;
    }

    #region Movement
    void FixedUpdate()
    {
        if (_isMoving) SteerMovement();
    }

    /// <summary>
    /// The interpolation of the steering movement
    /// </summary>
    void SteerMovement()
    {
        if (_currentMoveTarget != null)
        {
            Vector3 newPosition = Vector3.MoveTowards(rb.position, _currentMoveTarget, steerSpeed * Time.fixedDeltaTime);
            rb.MovePosition(newPosition);
            if (rb.position.sqrMagnitude == newPosition.sqrMagnitude)
            {
                _isMoving = false;
            }
        }
    }
    #endregion

    #region Injection
    public void InjectRiverManager(River_Manager manager)
    {
        riverManager = manager;
        print($"Injected {manager} into {name}");
    }
    #endregion

    #region Damage Events

    [Header("Health Stats")]
    [SerializeField] int _currentHealth;
    [SerializeField] int _maxHealth;
    public int CurrentHealth
    {
        get { return _currentHealth; }
        set { _currentHealth = value; }
    }
    public int MaxHealth
    {
        get { return _maxHealth; }
        set { _maxHealth = value; }
    }

    public void TakeDamage(float amount)
    {
        riverManager.SlowDownRiver();
    }

    public void Die()
    {
        throw new System.NotImplementedException();
    }

    public void RestoreHealth()
    {
        CurrentHealth = MaxHealth;
    }


    #endregion
}
