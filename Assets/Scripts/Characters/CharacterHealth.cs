using UnityEngine;
using UnityEngine.Events;

public class CharacterHealth : MonoBehaviour, IDamageable
{
    [SerializeField] int _currentHealth;
    public int CurrentHealth { get { return _currentHealth; } set { _currentHealth = value; } }

    [SerializeField] int _maxHealth;
    public int MaxHealth { get { return _maxHealth; } set { _maxHealth = value; } }

    [SerializeField] bool _isDead;
    public bool IsDead { get { return _isDead; } set { _isDead = value; } }

    [Space]

    [SerializeField] UnityEvent _deathEvent;
    [SerializeField] UnityEvent _healthRestoredEvent;
    [SerializeField] UnityEvent _tookDamageEvent;

    public void Die()
    {
        _deathEvent?.Invoke();
    }

    public void RestoreHealth()
    {
        CurrentHealth = MaxHealth;
    }

    public void TakeDamage(int amount = 1)
    {
        CurrentHealth -= amount;

        if (CurrentHealth <= 0) _deathEvent?.Invoke();
    }
}
