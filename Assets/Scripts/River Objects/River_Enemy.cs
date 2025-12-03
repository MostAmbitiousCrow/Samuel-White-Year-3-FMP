using System;
using EditorAttributes;
using UnityEngine;

public class River_Enemy : River_Object, ITargetsBoat
{
    [Line(GUIColor.Red, 1, 3)]
    [Header("Enemy Stats")]
    public EnemyData enemyData;

    public Boat_Space_Manager SpaceManager { get; private set; }
    private Transform _boatTransform;

    public void OverrideStats(EnemyData overrideStats)
    {
        enemyData = overrideStats;
        // TODO Override Health!
        print($"{name} stats were overrided");
    }

    public void InjectBoatSpaceManager(Boat_Space_Manager bsm)
    {
        if (!bsm) Debug.LogError($"Missing {bsm}");
        SpaceManager = bsm;
        _boatTransform = SpaceManager.transform;
    }

    protected override void OnFixedUpdate()
    {
        base.OnFixedUpdate();
        
        // TODO: Detect when close to the players boat

        if(_boatTransform && GetDistanceToCurrentLane() < 3f) //TODO: Something to consider here
        {
            _isMoving = false;
            _enemyStateMachine.EmergeFromRiver();
        }
        if (!_isMoving)
        {
            if (enemyData.FollowsBoat)
            {
                transform.position = new(transform.position.x, transform.position.y, _boatTransform.position.z);
            }
            else
            {
                transform.position = new(transform.position.x, transform.position.y, _boatTransform.position.z);

            }
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

    #region State Influence

    [SerializeField] EnemyStateController _enemyStateMachine;

    #endregion
}

[Serializable]
public class EnemyData
{
    public float EmergeTime = 3f;
    public int health = 1;
    public bool FollowsBoat = false;
}
