using System;
using UnityEngine;
using EditorAttributes;

/// <summary> Override data for the enemy from creating levels </summary>
//[Serializable]
//public class Enemy_Data
//{
//    //public float EmergeTime = 3f;
//    public int Health = 1;
//    //public bool FollowsBoat = false;
//}

[Serializable]
public class BoatEnemy_Data
{
    [Header("Boat Side Space Targeting")]
    [Tooltip("The Boats Side Space for the Enemy to target")]
    [Range(0, 1)] public int TargetSideSpace;

    [Tooltip("Should the enemy target the left side space of the boat")]
    public bool TargetLeftSide;

    [Tooltip("The direction the enemy should face upon landing on the boat")]
    public EnemyStateController.MoveDirection BoardingMoveDirection = EnemyStateController.MoveDirection.Right;

    [Line]
    [Tooltip("Should this enemy target the boats space")]
    public bool TargetBoatSpaces;

    //[Header("Boat Space Targeting")]
    [Tooltip("The space on the boat to target")]
    [ShowField(nameof(TargetBoatSpaces)), Range(0, 4)] public int TargetSpace;

    [Tooltip("The side of the boat to target")]
    [ShowField(nameof(TargetBoatSpaces)), Range(0, 1)] public int TargetBoatSide;
}