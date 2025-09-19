public interface IDamageable
{
    public int CurrentHealth { get; set; }
    public int MaxHealth { get; set; }

    public void TakeDamage(float amount = 1f);
    public void Die();
    public void RestoreHealth();
}
