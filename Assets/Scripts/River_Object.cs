using EditorAttributes;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
/// <summary>
/// Base class of all Objects that will interact with the Sewer River
/// </summary>
public class River_Object : MonoBehaviour, IRiverLaneMovement, IAffectedByRiver
{
    [Header("River Object Options")]
    [Tooltip("Should this object be affected by the speed of the river?")]
    [SerializeField] protected bool isAffectedByRiverSpeed;
    [Tooltip("The speed of which this object travels without the influence of the river.")]
    [SerializeField, ShowField(nameof(isAffectedByRiverSpeed))] protected float travelSpeed = 1f;

    [Space(10)]

    [Tooltip("The current lane of the boat the character is standing on")]
    [SerializeField] protected int _currentLane;
    [Tooltip("What lane should this object start on? (if applicable)")]
    public int startLane = 1;
    [Space(10)]
    [SerializeField] protected bool _isMoving;

    protected Vector3 _currentMoveTarget;

    [Header("Components")]
    [SerializeField] protected bool isAnimated;
    [SerializeField, ShowField(nameof(isAnimated))] River_Object_Animator riverObjectAnimator;
    [Space(10)]
    public Rigidbody rb;

    protected River_Manager riverManager;

    #region Space Movement Logic

    public void MoveToLane(int direction)
    {
        River_Manager.RiverLane rl = riverManager.GetLaneFromDirection(_currentLane, direction);

        _currentLane = rl.ID;
        _currentMoveTarget = new Vector3(rl.axis.x, rl.axis.y, transform.position.z); //TODO: Add movement interpolation
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
    #endregion

    #region Update Events

    void FixedUpdate()
    {
        RiverFlowMovement();
    }

    void RiverFlowMovement()
    {
        float speed = isAffectedByRiverSpeed ? riverManager.RiverSpeed : travelSpeed;

        // Move the object forwards
        Vector3 forwardMovement = Time.fixedDeltaTime * speed * transform.forward;
        rb.MovePosition(rb.position + forwardMovement);
    }
    #endregion

    #region Injection
    public void InjectRiverManager(River_Manager manager)
    {
        riverManager = manager;
        print($"Injected {manager} into {name}");
    }
    #endregion
}
