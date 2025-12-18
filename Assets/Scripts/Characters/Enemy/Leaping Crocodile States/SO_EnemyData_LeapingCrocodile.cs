using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LeapingCrocodile", menuName = "ScriptableObjects/EnemyData/LeapingCrocodile", order = 1)]
public class SO_EnemyData_LeapingCrocodile : SO_EnemyData
{
    [Header("Leap Data")]
    public AnimationCurve LeapCurve;
    public float LeapSpeedMultiplier = 1f;
}
