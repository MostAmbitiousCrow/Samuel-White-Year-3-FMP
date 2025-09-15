public interface IDamageable
{
    public int CurrentHealth { get; set; }
    public int MaxHealth { get; set; }

    public void TakeDamage(int amount);
    public void Die();
    public void RestoreHealth();
}
