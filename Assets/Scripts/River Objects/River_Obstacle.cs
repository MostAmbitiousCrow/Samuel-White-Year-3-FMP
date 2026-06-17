using UnityEngine;
using System;
using UnityEngine.Serialization;

// Collider will only register enemies, the player and their boat
/// <summary>
/// Base class for obstacles. Derives from the River_Object class.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
public class River_Obstacle : River_Object
{
    /// <summary>
    /// Overrided Stats Data
    /// </summary>
    [EditorAttributes.Line(EditorAttributes.GUIColor.Cyan, 1, 3)]
    [Header("Obstacle Stats")]
    public ObstacleData obstacleData; //TODO can be private

    [FormerlySerializedAs("IsHit")] [SerializeField] protected bool isHit; //{ get; protected set; }
    
    [SerializeField] protected BoxCollider boxCollider;

    public void OverrideData(ObstacleData overridedData)
    {
        obstacleData = overridedData;
        // print($"{name} stats were overrided");
    }

    // When collided with an object (player or enemy), damage it and destroy this obstacle
    private void OnTriggerEnter(Collider other)
    {
        OnHit(other.gameObject);
    }
    
    protected void OnHit(GameObject other)
    {
        if (isHit)
        {
            print($"{name} Was already hit");
            return;
        }
        print($"{name} hit: {other.gameObject.name}");

        if (other.TryGetComponent<IDamageable>(out var character))
        {
            character.TakeDamage(DamageType.Standard, obstacleData.ImpactDamage);
            isHit = true;
            if (explodesOnHit) artExploder.ExplodeArt();
            
        }
        else if (other.CompareTag("Boat"))
        {
            other.GetComponent<Boat_Controller>().TakeDamage();
            isHit = true;
            if (explodesOnHit) artExploder.ExplodeArt();
        }
    }

    // TODO: Add animation / Sink or destroy obstacle after damaging something


    #region Pooling Methods

    protected override void OnObjectPlaced()
    {
        base.OnObjectPlaced();
        isHit = false;
    }

    #endregion
}

[Serializable]
public class ObstacleData
{
    public int ImpactDamage = 1;
}
