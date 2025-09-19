using EditorAttributes;
using UnityEngine;

public class River_Enemy : River_Object, ITargetsBoat
{
    [Line(GUIColor.Red, 1, 3)]
    [Header("Enemy Stats")]
    public float EmergeTime = 3f;

    public Boat_Space_Manager SpaceManager { get; set; }
    private Transform _boatTransform;

    public void OverrideStats(Section_Enemy_Object.EnemyData.EnemyOverrideStats overrideStats)
    {
        EmergeTime = overrideStats.EmergeTime;
        // TODO Override Health!
        // Health = overrideStats.Health;
        print($"{name} stats were overrided");
    }

    public void InjectBoatSpaceManager(Boat_Space_Manager bsm)
    {
        if (bsm == null) Debug.LogError($"Missing {bsm}");
        SpaceManager = bsm;
        _boatTransform = SpaceManager.transform;
    }

    // private void FixedUpdate()
    // {
    //     if (_isMoving)
    //     {
    //         // TODO: Detect when close to the players boat
    //         return;
    //     }
    // }
}
