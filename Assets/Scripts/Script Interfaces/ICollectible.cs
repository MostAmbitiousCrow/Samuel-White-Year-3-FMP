public interface ICollectible
{
    /// <summary>
    /// The value of the collectible that is added to the players bank
    /// </summary>
    public int BankValue { get; set; }
    public bool IsCollected { get; set; }

    /// <summary>
    /// Called whenever this collectible is collected by the player
    /// </summary>
    public void OnCollected();
    /// <summary>
    /// Call to reset the collection status of the collectible
    /// </summary>
    public void Reset();
}
