using EditorAttributes;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Base class of all Objects that will interact with the Sewer River
/// </summary>
public abstract class River_Object : MonoTimeBehaviour, IRiverLaneMovement, IPooledObject
{
    [Line(GUIColor.White, 1, 3)]
    [Header("River Object Options")]
    [Tooltip("Should this object be affected by the speed of the river?")]
    [SerializeField] protected bool isAffectedByRiverSpeed = true;
    [Tooltip("The speed of which this object travels without the influence of the river.")]
    [SerializeField, HideField(nameof(isAffectedByRiverSpeed))] protected float travelSpeed = 1f;
    
    [Space]
    
    [Tooltip("The current lane of the river this object is on")]
    [SerializeField] protected int currentLane;
    /// <summary>
    /// The lane this object starts on
    /// </summary>
    [Tooltip("What lane should this object start on? (if applicable)")]
    [SerializeField] protected int startLane = 1;
    /// <summary>
    /// The current height of this object
    /// </summary>
    [Tooltip("The current height of this object")]
    [SerializeField] protected float height = 0f;
    /// <summary>
    /// The distance of this object to the destination of its lane
    /// </summary>
    [Tooltip("The distance of this object to the destination of its lane")]
    [SerializeField] protected float distance = 0f;
    [Space]
    public bool canMove = false;
    public bool isMoving;

    protected Vector3 CurrentMoveTarget;
    
    [Header("Components")]
    [SerializeField] protected bool explodesOnHit;
    [SerializeField, ShowField(nameof(explodesOnHit))] protected ArtExplode artExploder;

    //protected River_Manager riverManager;

    #region Space Movement Logic

    public void StartOnLane(int lane, float startDistance, float startHeight)
    {
        startLane = lane;
        GoToLane(startLane);
        SetDistanceAndHeight(startDistance, startHeight);
    }

    public void MoveToLane(int direction)
    {
        var rl = River_Manager.Instance.GetLaneFromDirection(currentLane, direction);

        currentLane = rl.ID;
        CurrentMoveTarget = new Vector3(rl.axis.x, rl.axis.y, transform.position.z); //TODO: Add optional movement interpolation
        isMoving = true;
        // print($"Moved {direction} to Space Position: {rl.axis}, ID {rl.ID}");
    }

    public void GoToLane(int lane)
    {
        var rl = River_Manager.Instance.GetLane(lane);

        currentLane = rl.ID;
        transform.position = new(rl.axis.x, rl.axis.y, transform.position.z);
    }

    public int GetCurrentLane()
    {
        return currentLane;
    }

    public void SetDistanceAndHeight(float distance, float height)
    {
        this.distance = distance; this.height = height;

        Vector3 t = transform.position;
        transform.position = new(t.x, height, distance);
    }
    #endregion

    #region Update Events
    protected override void FixedTimeUpdate()
    {
        // Do Movement
        if (!isMoving || !canMove) return;
        RiverFlowMovement();
        distance = GetDistanceToCurrentLane();
            
        // Once out of sight, return to pool
        if (distance < -10f) ReturnToPool();
    }

    private void RiverFlowMovement()
    {
        float speed = isAffectedByRiverSpeed ? River_Manager.Instance.CurrentRiverSpeed : travelSpeed;

        // Move the object forwards
        Vector3 travelDirection = Time.fixedDeltaTime * speed * Vector3.back;
        transform.Translate(travelDirection, Space.World); // Move along the river in world space
    }
    #endregion

    #region Math
    protected float GetDistanceToCurrentLane()
    {
        return transform.position.z - River_Manager.Instance.GetLane(currentLane).axis.z;
    }
    #endregion

    #region Pooling
    
    public int PoolObjectID { get; set; }

    public void ReturnToPool()
    {
        ObjectPoolManager.Instance.ReturnToPool(PoolObjectID, gameObject);
    }

    public virtual void OnSpawn()
    {
        isMoving = true;
        return;
    }

    #endregion

}
