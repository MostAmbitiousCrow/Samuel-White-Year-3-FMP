using UnityEngine;

//[CreateAssetMenu(fileName = "EnemyData", menuName = "ScriptableObjects/EnemyData", order = 1)]
public abstract class SO_EnemyData : ScriptableObject
{
    [Header("Health")]
    [Min(0)] public int MaxHealth = 1;

    [Header("Attack Data")]
    [Tooltip("The time in seconds from the start of the attack until the attack happens")]
    [Min(0)] public float AttackDelay = .45f;
    [Tooltip("The cooldown in seconds after the enemy has attacked")]
    [Min(0)] public float AttackCooldown = 1f;
    [Tooltip("The distance of the attack")]
    // TODO: Maybe replace with just how many spaces on the boat the attack can reach along with the size of the attack?
    [Min(0)] public float AttackDistance = 1.5f;

    [Header("Emerge Data")]
    [Tooltip("Time in seconds it takes for the enemy to emerge from its pool")]
    [Min(0)] public float TimeToEmerge = 2.25f;

    [Header("Movement (if applicable)")]
    [Tooltip("Time in seconds before each step can activate")]
    [Min(0)] public float TimeUntilStep = 3f;
    [Tooltip("Time in seconds of the delay before the step happens after activation")]
    [Min(0)] public float DelayBeforeStep = .5f;
    [Tooltip("Time in seconds after a step before the next step can start its countdown")]
    [Min(0)] public float CoolDownPerStep = 2.5f;
}
