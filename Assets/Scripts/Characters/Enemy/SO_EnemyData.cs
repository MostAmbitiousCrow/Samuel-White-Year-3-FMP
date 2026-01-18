using UnityEngine;
using UnityEngine.Serialization;

//[CreateAssetMenu(fileName = "EnemyData", menuName = "ScriptableObjects/EnemyData", order = 1)]
public abstract class SO_EnemyData : ScriptableObject
{
    [Header("Health")]
    [Min(0)] public int maxHealth = 1;
    
    [Header("Attack Data")]
    [Tooltip("The time in seconds from the start of the attack until the attack happens")]
    [Min(0)] public float attackDelay = .45f;
    [Tooltip("The time in seconds for the attack hitbox to appear after the delay")]
    [Min(0)] public float attackAtTime = 0.33f;
    [Tooltip("The cooldown in seconds after the enemy has attacked")]
    [Min(0)] public float attackCooldown = 1f;
    [Tooltip("The distance of the attack")]
    // TODO: Maybe replace with just how many spaces on the boat the attack can reach along with the size of the attack?
    [Min(0)] public float attackDistance = 1.5f;
    
    [Header("Emerge Data")]
    [Tooltip("Time in seconds it takes for the enemy to emerge from its pool")]
    [Min(0)] public float timeToEmerge = 2.25f;
    
    [Header("Movement (if applicable)")]
    [Tooltip("Time in seconds before each step can activate")]
    [Min(0)] public float timeUntilStep = 3f;
    [Tooltip("Time in seconds of the delay before the step happens after activation")]
    [Min(0)] public float delayBeforeStep = .5f;
    [Tooltip("Time in seconds after a step before the next step can start its countdown")]
    [Min(0)] public float coolDownPerStep = 2.5f;
}
