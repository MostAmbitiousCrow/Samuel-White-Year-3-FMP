using UnityEngine;
using EditorAttributes;

[RequireComponent(typeof(Rigidbody))]
// Collider will only register enemies, the player and their boat
[RequireComponent(typeof(BoxCollider))]
/// <summary>
/// Base class for obstacles. Derives from the River_Object class.
/// </summary>
public class River_Obstacle : River_Object
{
    [Line(GUIColor.Cyan, 1, 3)]
    [Header("Obstacle Stats")]
    /// <summary>
    /// Overrided Stats Data
    /// </summary>
    public float impactDamage = 1f; // TODO: private get/set

    public void OverrideStats(Section_Obstacle_Object.ObstacleData.ObstacleOverrideStats overrideStats)
    {
        impactDamage = overrideStats.ImpactDamage;
        print($"{name} stats were overrided");
    }

    // When collided with an object (player or enemy), damage it and destroy this obstacle
    void OnTriggerEnter(Collider other)
    {
        other.GetComponent<IDamageable>().TakeDamage(impactDamage);

        riverObjectAnimator.TriggerDestroyAnimation();
    }

    // TODO: Add animation / Sink or destroy obstacle after damaging something
}
