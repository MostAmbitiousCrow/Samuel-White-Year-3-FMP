using EditorAttributes;
using UnityEngine;
using UnityEngine.Serialization;
using static Boat_Space_Manager.BoatSide;

public class Boat_Controller : MonoTimeBehaviour, IRiverLaneMovement //, IDamageable
{
    [Line(GUIColor.Green)]
    [Header("Boat Settings")]
    public float steerSpeed = 1;
    public AnimationCurve SteerInterpolationCurve;
    [FormerlySerializedAs("_currentLane")]
    [Space(10)]
    [Tooltip("The current lane of the boat the character is standing on")]
    [SerializeField, ReadOnly] private int currentLane;
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
    private Vector3 _startMovePosition;
    private float _moveElapsed;
    [SerializeField] private float steerDuration = 0.35f;

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

        if (Mathf.Abs(steerDirection) > 0.01f)
        {
            MoveToLane((int)steerDirection);
        }

        Debug.Log("Boat was steered!");
    }

    public void MoveToLane(int direction)
    {
        River_Manager.RiverLane rl = River_Manager.Instance.GetLaneFromDirection(currentLane, direction);
        if (rl == null) return;

        currentLane = rl.ID;

        Vector3 lanePos = rl.transform.localPosition;

        // IMPORTANT:
        // Use CURRENT position as new start (allows mid-steer blending)
        _startMovePosition = transform.localPosition;
        _currentMoveTarget = new Vector3(lanePos.x, lanePos.y, transform.localPosition.z);

        _moveElapsed = 0f;
        _isMoving = true;
    }



    public void GoToLane(int lane)
    {
        River_Manager.RiverLane rl = River_Manager.Instance.GetLane(lane);

        var pos = rl.transform.localPosition;
        currentLane = rl.ID;
        transform.localPosition = new(pos.x, pos.y, transform.localPosition.z);
    }

    public int GetCurrentLane()
    {
        return currentLane;
    }

    #region Movement
    protected override void TimeUpdate()
    {
        if (_isMoving) SteerMovement();
    }
    
    private void SteerMovement()
    {
        _moveElapsed += Time.deltaTime / Mathf.Max(steerDuration, 0.0001f);

        float t = Mathf.Clamp01(_moveElapsed);
        float curvedT = SteerInterpolationCurve?.Evaluate(t) ?? t;

        Vector3 newPosition = Vector3.Lerp(_startMovePosition, _currentMoveTarget, curvedT);
        transform.localPosition = newPosition;

        if (!(t >= 1f)) return;
        transform.localPosition = _currentMoveTarget;
        _isMoving = false;
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
