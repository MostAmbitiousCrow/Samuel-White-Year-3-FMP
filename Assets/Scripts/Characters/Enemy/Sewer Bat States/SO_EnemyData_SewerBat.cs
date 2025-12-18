using UnityEngine;

[CreateAssetMenu(fileName = "SewerBat", menuName = "ScriptableObjects/EnemyData/SewerBat", order = 1)]
public class SO_EnemyData_SewerBat : SO_EnemyData
{
    [Header("Dive Data")]
    public AnimationCurve DiveCurve;
    public float DiveSpeedMultiplier = 1f;
}
