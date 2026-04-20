using System.Collections;
using EditorAttributes;
using GameCharacters;
using UnityEngine;
using UnityEngine.Events;
using Void = EditorAttributes.Void;

public class CharacterHealth : MonoBehaviour, IDamageable
{
    private static readonly int HealthParameter = Animator.StringToHash("Health");

    #region Variables
    [SerializeField] private int currentHealth;
    public int CurrentHealth => currentHealth;
    [SerializeField] private int maxHealth;
    public int MaxHealth => maxHealth;
    [SerializeField, ReadOnly] private bool isDead;
    public bool IsDead => isDead;
    [Space]
    [SerializeField] private float invincibilityDuration = .25f;
    public bool IsInvincible { get; set; }

    [Space] 
    [FoldoutGroup("Damage Events", nameof(deathEvent), nameof(healthRestoredEvent), nameof(tookDamageEvent))]
    [SerializeField] private Void showEvents;
    [SerializeField, HideProperty] private UnityEvent deathEvent;
    [SerializeField, HideProperty] private UnityEvent healthRestoredEvent;
    [SerializeField, HideProperty] private UnityEvent tookDamageEvent;
    
    [Space]
    [SerializeField] private bool doInvincibilityAnimation;
    [SerializeField, ShowField(nameof(doInvincibilityAnimation))] private MeshRenderer[]  renderers;

    [Header("Components")]
    [SerializeField] private Character character;
    [SerializeField] private Animator animator;
    #endregion

    private void Awake()
    {
        if (!animator) animator = GetComponentInChildren<Animator>(true);
        
        RestoreHealth();
    }

    private void OnEnable()
    {
        RestoreHealth();
    }

    public void Die(DamageType type = DamageType.Standard)
    {
        isDead = true;
        
        character.OnDied(type);
        deathEvent?.Invoke();
    }

    public void RestoreHealth()
    {
        isDead = false;
        currentHealth = MaxHealth;
        
        var normalisedHealth = CurrentHealth / (float)MaxHealth;
        animator.SetFloat(HealthParameter, normalisedHealth);
        
        character.OnHealthRestored();
        healthRestoredEvent?.Invoke();
    }

    public void TakeDamage(DamageType type = DamageType.Standard, int amount = 1)
    {
        // Skip damage if invincible or already dead. Pass if the damage was from the tsunami.
        if ((IsInvincible || isDead) && (type != DamageType.Tsunami)) return;
        
        currentHealth -= amount;
        if (CurrentHealth <= 0) Die(type);
        else
        {
            tookDamageEvent?.Invoke();
            character.OnTookDamage(type);
            StartCoroutine(DamageInvincibilityRoutine());
        }
        
        // Note: Health is a normalised value in the Animator.
        
        var normalisedHealth = CurrentHealth / (float)MaxHealth;
        animator.SetFloat(HealthParameter, normalisedHealth);
        Debug.Log($"{name} damaged by type: {type}, for {amount} damage");
    }

    private IEnumerator DamageInvincibilityRoutine()
    {
        IsInvincible = true;
        if (doInvincibilityAnimation)
        {
            float t = 0f;
            float phases = (invincibilityDuration * .25f) / 3f;
            
            while (t < invincibilityDuration)
            {
                foreach (var item in renderers)
                {
                    item.enabled = false;
                }
                
                yield return new WaitForSeconds(phases);
                t += phases;
                
                foreach (var item in renderers)
                {
                    item.enabled = true;
                }
    
                yield return new WaitForSeconds(phases);
                t += phases;
            }
            
            foreach (var item in renderers)
            {
                item.enabled = true;
            }
        }
        else yield return new WaitForSeconds(invincibilityDuration);

        IsInvincible = false;
    }
}
