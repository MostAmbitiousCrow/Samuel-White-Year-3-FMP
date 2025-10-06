using System;
using EditorAttributes;
using UnityEngine;

public class River_Enemy : River_Object, ITargetsBoat
{
    [Line(GUIColor.Red, 1, 3)]
    [Header("Enemy Stats")]
    public EnemyData enemyData;

    public Boat_Space_Manager SpaceManager { get; set; }
    private Transform _boatTransform;

    public void OverrideStats(EnemyData overrideStats)
    {
        enemyData = overrideStats;
        // TODO Override Health!
        print($"{name} stats were overrided");
    }

    public void InjectBoatSpaceManager(Boat_Space_Manager bsm)
    {
        if (bsm == null) Debug.LogError($"Missing {bsm}");
        SpaceManager = bsm;
        _boatTransform = SpaceManager.transform;
    }

    protected override void VirtualUpdateMethod()
    {
        base.VirtualUpdateMethod();
        
        // TODO: Detect when close to the players boat

        return;
    }


    #region Pooling Methods

    protected override void OnSpawn()
    {
        base.OnSpawn();
        return;
    }

    #endregion
}

[Serializable]
public class EnemyData
{
    public float EmergeTime = 3f;
    public int health = 1;
}
