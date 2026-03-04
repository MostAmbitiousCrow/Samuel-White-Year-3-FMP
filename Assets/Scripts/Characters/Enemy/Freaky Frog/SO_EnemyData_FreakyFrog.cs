using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FreakyFrog", menuName = "ScriptableObjects/EnemyData/FreakyFrog", order = 1)]
public class SO_EnemyData_FreakyFrog : SO_EnemyData
{
    [Header("Tongue Data")]
    public AnimationCurve TongueCurve;
    public float TongueSpeedMultiplier = 1f;
}
