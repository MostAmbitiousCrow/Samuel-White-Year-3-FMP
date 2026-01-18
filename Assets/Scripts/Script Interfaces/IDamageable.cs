using System;public interface IDamageable
{
    public int CurrentHealth { get; }
    public int MaxHealth { get; }
    public bool IsDead { get; }
    public bool IsInvincible { get; }

    /// <summary> Cause the character to take a set amount of damage </summary>
    public void TakeDamage(DamageType type = DamageType.Standard, int amount = 1);

    /// <summary> Kills the character </summary>
    public void Die();

    /// <summary> Restores the health of the character to its default amount </summary>
    public void RestoreHealth();
}

public enum DamageType
{
    Stomp, Standard
}