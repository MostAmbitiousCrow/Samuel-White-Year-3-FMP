using System.Collections;
using EditorAttributes;
using UnityEngine;
using UnityEngine.Events;

public class CharacterHealth : MonoBehaviour, IDamageable
{
    #region Variables
    [SerializeField] private int currentHealth;
    public int CurrentHealth => currentHealth;

    [SerializeField] private int maxHealth;
    public int MaxHealth => maxHealth;

    [SerializeField, ReadOnly] private bool isDead;
    public bool IsDead => isDead;
    [SerializeField, ReadOnly] private bool isInvincible;
    public bool IsInvincible => isInvincible;

    [Space] 
    [SerializeField] private bool showEvents;
    [SerializeField, ShowField(nameof(showEvents))] private UnityEvent deathEvent;
    [SerializeField, ShowField(nameof(showEvents))] private UnityEvent healthRestoredEvent;
    [SerializeField, ShowField(nameof(showEvents))] private UnityEvent tookDamageEvent;
    #endregion

    public void Die()
    {
        isDead = true;
        deathEvent?.Invoke();
    }

    public void RestoreHealth()
    {
        isDead = false;
        currentHealth = MaxHealth;
        healthRestoredEvent?.Invoke();
    }

    public void TakeDamage(DamageType type = DamageType.Standard, int amount = 1)
    {
        if (isInvincible || isDead) return;
        
        currentHealth -= amount;
        if (CurrentHealth <= 0) Die();
        else
        {
            tookDamageEvent.Invoke();
            StartCoroutine(DamageInvincibilityRoutine());
        }
    }

    private IEnumerator DamageInvincibilityRoutine()
    {
        isInvincible = true;
        yield return new WaitForSeconds(.25f);
        isInvincible = false;
    }
}
