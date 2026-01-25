using UnityEngine;
using EditorAttributes;
using System;

[RequireComponent(typeof(Rigidbody))]
// Collider will only register enemies, the player and their boat
[RequireComponent(typeof(BoxCollider))]
/// <summary>
/// Base class for collectibles. Derives from the River_Object class.
/// </summary>
public abstract class River_Collectible : River_Object
{
    [Line(GUIColor.Yellow, 1, 3)]
    #region Variables
    [SerializeField] int _defautlBankValue;

    [SerializeField] bool _isCollected;
    public bool IsCollected
    {
        get { return _isCollected; }
        set { _isCollected = value; }
    }
    public CollectibleData Data;

    public void OverrideData(CollectibleData overridedData)
    {
        Data = overridedData;
        print($"{name} stats were overrided");
    }
    #endregion

    #region Events
    /// <summary>
    /// Event called when the collectible has been collected by the player
    /// </summary>
    protected virtual void OnCollected()
    {
        IsCollected = true;
    }

    /// <summary>
    /// Resets the collectible
    /// </summary>
    protected virtual void Reset()
    {
        IsCollected = false;
        canMove = true;
    }

    #endregion

    #region Trigger
    void OnTriggerEnter(Collider other)
    {
        if (!IsCollected) OnCollected();
    }
    #endregion
}

[Serializable]
public class CollectibleData
{
    public int BankValue = 1;
}
