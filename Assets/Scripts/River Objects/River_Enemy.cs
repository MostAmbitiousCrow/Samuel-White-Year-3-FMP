using EditorAttributes;
using UnityEngine;

public class River_Enemy : River_Object
{
    [Line(GUIColor.Red, 1, 3)]
    [Header("Enemy Stats")]
    [SerializeField] float _emergeTriggerDetectRadius = 3f;
    public BoatEnemy_Data EnemyData { get; private set; }

    //public Boat_Space_Manager SpaceManager { get; private set; }
    private Transform BoatTransform { get { return Boat_Space_Manager.Instance.transform; } }
    [SerializeField] EnemyStateController _enemyController;

    public void OverrideStats(BoatEnemy_Data overrideStats)
    {
        EnemyData = overrideStats;

        _enemyController.InitialiseEnemy(overrideStats);

        // TODO Override Health!
        print($"{name} stats were overrided");
    }

    //public void InjectBoatSpaceManager(Boat_Space_Manager bsm)
    //{
    //    if (!bsm) Debug.LogError($"Missing Boat Space Manager");
    //    SpaceManager = bsm;
    //    _boatTransform = SpaceManager.transform;
    //}

    protected override void OnFixedUpdate()
    {
        base.OnFixedUpdate();
        
        // TODO: Detect when close to the players boat

        if(_isMoving) //TODO: Something to consider here
        {
            if (GetDistanceToCurrentLane() < _emergeTriggerDetectRadius)
            {
                _isMoving = false;
                _enemyController.EmergeFromRiver();
            }
        }
        else
        {
            transform.position = new(transform.position.x, transform.position.y, BoatTransform.position.z);
        }

        return;
    }

    #region Pooling Methods

    protected override void OnSpawn()
    {
        base.OnSpawn();
        return;
    }

    #endregion


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _emergeTriggerDetectRadius);
    }
}

///// <summary>
///// Override data for the enemy from creating levels
///// </summary>
//[Serializable]
//public class EnemyData
//{
//    //public float EmergeTime = 3f;
//    public int health = 1;
//    //public bool FollowsBoat = false;

//    public int TargetSpace;
//    public int TargetBoatSide;
//    [Space]
//    [Tooltip("")]
//    public int TargetSideSpace;
//    [Tooltip("Should the enemy target the left side space of the boat")]
//    public bool TargetLeftSide;
//}
