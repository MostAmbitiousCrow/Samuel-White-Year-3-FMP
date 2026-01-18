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
    public bool targetSideSpaces;
    
    [Tooltip("The Boats Side Space for the Enemy to target")]
    [ShowField(nameof(targetSideSpaces)), Range(0, 1)] public int targetSideSpace;

    [Tooltip("Should the enemy target the left side space of the boat")]
    [ShowField(nameof(targetSideSpaces))] public bool targetLeftSide;
    
    [ShowField(nameof(targetSideSpaces))] 
    public Character.MoveDirection startFacingDirection = Character.MoveDirection.Right;
    
    [Line,Header("Boat Space Targeting")]
    [Tooltip("Should this enemy target the boats space")]
    public bool targetBoatSpaces;
    
    [Tooltip("The space on the boat to target")]
    [ShowField(nameof(targetBoatSpaces)), Range(0, 4)] public int targetSpace;
    
    [Tooltip("The side of the boat to target")]
    [ShowField(nameof(targetBoatSpaces)), Range(0, 1)] public int targetBoatSide;
    
    [Tooltip("The direction the enemy should face upon landing on the boat")]
    [ShowField(nameof(targetBoatSpaces))] 
    public Character.MoveDirection boardingFacingDirection = Character.MoveDirection.Right;
}