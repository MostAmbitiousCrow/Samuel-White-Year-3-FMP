using EditorAttributes;
using UnityEngine;

/// <summary>
/// Base class of all Objects that will interact with the Sewer River
/// </summary>
public abstract class River_Object : MonoBehaviour, IRiverLaneMovement, IAffectedByRiver
{
    [Line(GUIColor.White, 1, 3)]
    [Header("River Object Options")]
    [Tooltip("Should this object be affected by the speed of the river?")]
    [SerializeField] protected bool isAffectedByRiverSpeed = true;
    [Tooltip("The speed of which this object travels without the influence of the river.")]
    [SerializeField, HideField(nameof(isAffectedByRiverSpeed))] protected float travelSpeed = 1f;

    [Space(10)]

    /// <summary>
    /// The current lane of the river this object is on
    /// </summary>
    [Tooltip("The current lane of the river this object is on")]
    [SerializeField] protected int _currentLane;
    /// <summary>
    /// The lane this object starts on
    /// </summary>
    [Tooltip("What lane should this object start on? (if applicable)")]
    [SerializeField] protected int startLane = 1;
    /// <summary>
    /// The current height of this object
    /// </summary>
    [Tooltip("The current height of this object")]
    [SerializeField] protected float _height = 0f;
    /// <summary>
    /// The distance of this object to the destination of its lane
    /// </summary>
    [Tooltip("The distance of this object to the destination of its lane")]
    [SerializeField] protected float _distance = 0f;
    [Space(10)]
    public bool CanMove = false;
    [SerializeField] protected bool _isMoving;

    protected Vector3 _currentMoveTarget;

    [Header("Components")]
    [SerializeField] protected bool isAnimated;
    [SerializeField, ShowField(nameof(isAnimated))] protected River_Object_Animator riverObjectAnimator;

    protected River_Manager riverManager;

    #region Space Movement Logic

    public void StartOnLane(int lane, float distance, float height)
    {
        startLane = lane;
        GoToLane(startLane);
        SetDistanceAndHeight(distance, height);
    }

    public void MoveToLane(int direction)
    {
        River_Manager.RiverLane rl = riverManager.GetLaneFromDirection(_currentLane, direction);

        _currentLane = rl.ID;
        _currentMoveTarget = new Vector3(rl.axis.x, rl.axis.y, transform.position.z); //TODO: Add optional movement interpolation
        _isMoving = true;
        // print($"Moved {direction} to Space Position: {rl.axis}, ID {rl.ID}");
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

    public void SetDistanceAndHeight(float distance, float height)
    {
        _distance = distance; _height = height;

        Vector3 t = transform.position;
        transform.position = new(t.x, height, distance);
    }
    #endregion

    #region Update Events
    private void Update()
    {
        OnUpdate();
    }
    void FixedUpdate()
    {
        OnFixedUpdate();
    }

    protected virtual void OnFixedUpdate()
    {
        if (_isMoving && CanMove)
        {
            RiverFlowMovement();
            _distance = GetDistanceToCurrentLane();
            
            if (_distance < .1f) // TODO Temporary until object pooling is implemented
                Destroy(gameObject);
        }
    }

    protected virtual void OnUpdate()
    {

    }

    void RiverFlowMovement()
    {
        float speed = isAffectedByRiverSpeed ? riverManager.CurrentRiverSpeed : travelSpeed;

        // Move the object forwards
        Vector3 travelDirection = Time.fixedDeltaTime * speed * Vector3.back;
        transform.Translate(travelDirection, Space.World); // Move along the river in world space
    }
    #endregion

    #region Pooling Methods

    public void Spawned()
    {
        OnSpawn();
    }

    protected virtual void OnSpawn() { }

    #endregion

    #region Injection
    public void InjectRiverManager(River_Manager manager)
    {
        riverManager = manager;
        print($"Injected {manager} into {name}");
    }
    #endregion

    #region Math
    protected float GetDistanceToCurrentLane()
    {
        // return Vector3.Distance(transform.position, riverManager.GetLane(_currentLane).axis);
        return transform.position.z - riverManager.GetLane(_currentLane).axis.z;
    }
    #endregion
}
