using UnityEngine;

public class EnemyDefeatedState : EnemyState
{
    private readonly float _deathDuration = 2.5f;
    private float _time = 0f;
    
    public override void OnEnter()
    {
        _time = 0f;
        Sc.Animator.SetTrigger("Defeated");
        Sc.CharacterCollider.enabled = false;
    }

    public override void OnExit()
    {
        _time = 0f;
        Sc.CharacterCollider.enabled = true;
    }

    public override void OnHurt()
    {

    }

    public override void UpdateState()
    {
        
    }
    
    /// <summary>
    /// Note: This method automatically runs a timer to determine this enemies time until returning to its object pool
    /// </summary>
    public override void FixedUpdateState()
    {
        _time += Time.deltaTime;
        if (_time > _deathDuration) Sc.ReturnToPool();
        
        // Debug.Log($"{Sc.name} is defeated: {_time}");
    }
}
