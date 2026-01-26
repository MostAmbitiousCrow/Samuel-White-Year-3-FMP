using UnityEngine;
public interface IPooledObject
{
    [Header("Pooling")]
    [Tooltip("The ID of this objects pool")]
    public int PoolObjectID { get; set; }

    /// <summary> Method to return this object to the pool </summary>
    public void ReturnToPool();

    /// <summary> Method called whenever this object is spawned </summary>
    public void OnSpawn();
}
