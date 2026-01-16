using EditorAttributes;
using UnityEngine;
using UnityEngine.Events;

public class CharacterHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private int currentHealth;
    public int CurrentHealth { get { return currentHealth; } set { currentHealth = value; } }

    [SerializeField] int maxHealth;
    public int MaxHealth { get { return maxHealth; } set { maxHealth = value; } }

    [SerializeField, ReadOnly] bool isDead;
    public bool IsDead { get { return isDead; } set { isDead = value; } }

    [Space] 
    [SerializeField] private bool showEvents;
    [SerializeField, ShowField(nameof(showEvents))] private UnityEvent deathEvent;
    [SerializeField, ShowField(nameof(showEvents))] private UnityEvent healthRestoredEvent;
    [SerializeField, ShowField(nameof(showEvents))] private UnityEvent tookDamageEvent;

    public void Die()
    {
        deathEvent?.Invoke();
    }

    public void RestoreHealth()
    {
        CurrentHealth = MaxHealth;
        healthRestoredEvent?.Invoke();
    }

    public void TakeDamage(int amount = 1)
    {
        CurrentHealth -= amount;

        if (CurrentHealth <= 0) Die();
    }
}
