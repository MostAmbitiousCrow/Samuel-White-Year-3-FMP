using UnityEngine;
using System;

[RequireComponent(typeof(Rigidbody))]
// Collider will only register enemies, the player and their boat
[RequireComponent(typeof(BoxCollider))]
/// <summary>
/// Base class for obstacles. Derives from the River_Object class.
/// </summary>
public class River_Obstacle : River_Object
{
    [EditorAttributes.Line(EditorAttributes.GUIColor.Cyan, 1, 3)]
    [Header("Obstacle Stats")]
    /// <summary>
    /// Overrided Stats Data
    /// </summary>
    public ObstacleData obstacleData; //TODO can be private
    public bool IsHit { get; private set; }

    public void OverrideData(ObstacleData overridedData)
    {
        obstacleData = overridedData;
        print($"{name} stats were overrided");
    }

    // When collided with an object (player or enemy), damage it and destroy this obstacle
    private void OnTriggerEnter(Collider other)
    {
        if (IsHit) return;
        print($"{name} hit: {other.name}");

        if (other.TryGetComponent<IDamageable>(out var character))
            character.TakeDamage(amount: obstacleData.ImpactDamage);
        IsHit = true;

        if (explodesOnHit) artExploder.ExplodeArt();
    }

    // TODO: Add animation / Sink or destroy obstacle after damaging something


    #region Pooling Methods

    public override void OnSpawn()
    {
        base.OnSpawn();
    }

    #endregion
}

[Serializable]
public class ObstacleData
{
    public int ImpactDamage = 1;
}
