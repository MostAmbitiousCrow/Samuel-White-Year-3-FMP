using UnityEngine;

//[CreateAssetMenu(fileName = "EnemyData", menuName = "ScriptableObjects/EnemyData", order = 1)]
public abstract class SO_EnemyData : ScriptableObject
{
    [Header("Health")]
    [Min(0)] public int MaxHealth = 1;

    [Header("Attack Data")]
    [Min(0)] public float AttackDelay = .45f;
    [Min(0)] public float AttackSpeedMultiplier = 1f;
}
