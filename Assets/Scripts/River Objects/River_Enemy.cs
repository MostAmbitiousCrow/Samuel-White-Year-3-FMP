using EditorAttributes;
using UnityEngine;

public class River_Enemy : River_Object
{
    [Line(GUIColor.Red, 1, 3)]
    [Header("Enemy Stats")]
    [SerializeField] float emergeTriggerDetectRadius = 3f;
    public BoatEnemy_Data EnemyData { get; private set; }
    
    [SerializeField] BoatEnemyStateController enemyController;

    public void OverrideStats(BoatEnemy_Data overrideStats)
    {
        EnemyData = overrideStats;

        enemyController.InitialiseEnemy(overrideStats);

        // TODO Override Health!
        print($"{name} stats were overrided");
    }

    private void OnEnable()
    {
        if (!enemyController) return;
        enemyController.gameObject.SetActive(false); //TODO: Adjust this for bat enemies who will spawn with their enemy active
    }

    protected override void FixedTimeUpdate()
    {
        base.FixedTimeUpdate();
        
        // TODO: Detect when close to the players boat

        if(isMoving) //TODO: Something to consider here
        {
            if (!(GetDistanceToCurrentLane() < emergeTriggerDetectRadius)) return;
            isMoving = false;
            enemyController.gameObject.SetActive(true);
            enemyController.EmergeFromRiver();
        }
        else
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, Boat_Space_Manager.Instance.transform.position.z);
        }

        return;
    }

    #region Pooling Methods

    public override void OnSpawn()
    {
        base.OnSpawn();
        return;
    }

    #endregion


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, emergeTriggerDetectRadius);
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
