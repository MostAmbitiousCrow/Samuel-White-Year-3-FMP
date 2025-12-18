public interface IDamageable
{
    public int CurrentHealth { get; set; }
    public int MaxHealth { get; set; }
    public bool IsDead { get; set; }

    /// <summary> Cause the character to take a set amount of damage </summary>
    public void TakeDamage(int amount = 1);

    /// <summary> Kills the character </summary>
    public void Die();

    /// <summary> Restores the health of the character to its default amount </summary>
    public void RestoreHealth();
}
