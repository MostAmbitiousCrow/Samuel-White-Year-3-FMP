using EditorAttributes;
using UnityEngine;

public class River_Object : MonoBehaviour, IRiverLaneMovement
{
    [Tooltip("Should this object be affected by the speed of the river?")]
    protected bool affectedByRiverSpeed;
    [Tooltip("The speed of which this object travels without the influence of the river.")]
    [ShowField(nameof(affectedByRiverSpeed))] protected float travelSpeed = 1f;

    [Tooltip("The current lane of the boat the character is standing on")]
    [SerializeField] int _currentLane;
    [Tooltip("What lane should this object start on? (if applicable)")]
    public int startLane = 1;

    [Header("Components")]
    [SerializeField] protected bool isAnimated;
    [SerializeField, ShowField(nameof(isAnimated))] River_Object_Animator riverObjectAnimator;

    #region Space Movement Logic

    public void MoveTowardsLane(int direction)
    {
        River_Manager.RiverLane rl = River_Manager.GetLaneFromDirection(_currentLane, direction);

        _currentLane = rl.ID;
        transform.position = new Vector3(rl.axis.x, rl.axis.y, transform.position.z); //TODO: Add movement interpolation
        print($"Moved {direction} to Space Position: {rl.axis}, ID {rl.ID}");
    }

    public void GoToLane(int lane)
    {
        River_Manager.RiverLane rl = River_Manager.GetLane(lane);

        _currentLane = rl.ID;
        transform.position = new(rl.axis.x, rl.axis.y, transform.position.z);
    }

    public int GetCurrentLane()
    {
        return _currentLane;
    }
    #endregion

    private void Start()
    {
        GoToLane(startLane);
    }
}
