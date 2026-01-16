using System;
using UnityEngine;
using EditorAttributes;
using GameCharacters;

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
    [Range(0, 1)] public int targetSideSpace;

    [Tooltip("Should the enemy target the left side space of the boat")]
    public bool targetLeftSide;

    [Tooltip("The direction the enemy should face upon landing on the boat")]
    public Character.MoveDirection boardingMoveDirection = Character.MoveDirection.Right;
    
    [Line]
    [Tooltip("Should this enemy target the boats space")]
    public bool targetBoatSpaces;
    
    [Tooltip("The space on the boat to target")]
    [ShowField(nameof(targetBoatSpaces)), Range(0, 4)] public int targetSpace;
    
    [Tooltip("The side of the boat to target")]
    [ShowField(nameof(targetBoatSpaces)), Range(0, 1)] public int targetBoatSide;
}