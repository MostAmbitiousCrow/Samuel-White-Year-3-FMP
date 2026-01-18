using UnityEngine;
using static Boat_Space_Manager.BoatSide;

public class Boat_Controller : MonoTimeBehaviour, IRiverLaneMovement //, IDamageable
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
    public bool IsMoving { get { return _isMoving; } }
    private Vector3 _currentMoveTarget;

    [Header("Components")]
    [SerializeField] private Rigidbody rb;

    //private River_Manager riverManager;
    //[SerializeField] private Boat_Space_Manager boatSpaceManager;

    private void Start()
    {
        GoToLane(startLane);
    }

    /// <summary>
    /// The main method to steer the players boat in a given direction and animates the force.
    /// </summary>
    public void SteerBoat(SpaceData spaceData, float force)
    {
        Transform spaceTransform = spaceData.t;
        Vector3 localPos = transform.InverseTransformPoint(spaceTransform.position);

        float steerDirection = Mathf.Sign(localPos.x);
        
        MoveToLane(Mathf.RoundToInt(steerDirection));
    }

    public void MoveToLane(int direction)
    {
        River_Manager.RiverLane rl = River_Manager.Instance.GetLaneFromDirection(_currentLane, direction);

        _currentLane = rl.ID;
        _currentMoveTarget = new Vector3(rl.axis.x, rl.axis.y, transform.position.z); //TODO: Add movement interpolation
        _isMoving = true;
    }

    public void GoToLane(int lane)
    {
        River_Manager.RiverLane rl = River_Manager.Instance.GetLane(lane);

        _currentLane = rl.ID;
        transform.position = new(rl.axis.x, rl.axis.y, transform.position.z);
    }

    public int GetCurrentLane()
    {
        return _currentLane;
    }

    #region Movement
    protected override void FixedTimeUpdate()
    {
        //if (GameManager.GameLogic.GamePaused) return;

        if (_isMoving) SteerMovement();
    }

    /// <summary>
    /// The interpolation of the steering movement
    /// </summary>
    void SteerMovement()
    {
        if (_currentMoveTarget != null)
        {
            Vector3 newPosition = Vector3.MoveTowards(rb.position, _currentMoveTarget, steerSpeed * Time.fixedDeltaTime * GameManager.GameLogic.GamePauseInt);
            rb.MovePosition(newPosition);
            if (rb.position.sqrMagnitude == newPosition.sqrMagnitude)
            {
                _isMoving = false;
            }
        }
    }
    #endregion

    #region Injection
    //public void InjectRiverManager(River_Manager manager)
    //{
    //    riverManager = manager;
    //    print($"Injected {manager} into {name}");
    //}
    #endregion

    #region Damage Events

    public void TookDamage()
    {
        print("Boat hit, slowing river");
        River_Manager.Instance.SlowDownRiver();
    }

    public void Died()
    {
        return;
    }

    public void HealthRestored()
    {
        return;
    }


    #endregion
}
