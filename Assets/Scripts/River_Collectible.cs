using UnityEngine;
using EditorAttributes;

[RequireComponent(typeof(Rigidbody))]
// Collider will only register enemies, the player and their boat
[RequireComponent(typeof(BoxCollider))]
/// <summary>
/// Base class for collectibles. Derives from the River_Object class.
/// </summary>
public abstract class River_Collectible : River_Object, ICollectible
{
    [Line(GUIColor.Yellow, 1, 3)]
    #region Variables
    [SerializeField] protected int _bankVaulue;
    [SerializeField] int _defautlBankValue;

    public int BankValue
    {
        get { return _bankVaulue; }
        set { _bankVaulue = value; }
    }

    public bool IsCollected { get; set; }

    public void OverrideStats(Section_Collectible_Object.CollectibleData.CollectibleOverrideStats overrideStats)
    {
        BankValue = overrideStats.BankValue;
        print($"{name} stats were overrided");
    }
    #endregion

    #region Events
    /// <summary>
    /// Event called when the collectible has been collected by the player
    /// </summary>
    public virtual void OnCollected() { }

    /// <summary>
    /// Resets the collectible
    /// </summary>
    public virtual void Reset()
    {
        IsCollected = false;
        BankValue = _defautlBankValue;
    }

    #endregion

    #region Trigger
    void OnTriggerEnter(Collider other)
    {
        if (IsCollected)
        OnCollected();
    }
    #endregion
}

// TODO: Introduce a cleaner data system by storing object data in their own classes for other classes to get from
// public class CollectibleData
// {
//     public int BankValue = 1;
// }
