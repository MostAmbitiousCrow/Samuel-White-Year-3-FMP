using UnityEngine;

public class EnemyDefeatedState : EnemyState
{
    public override void OnEnter()
    {
        Sc.IsDead = true;
        Sc.Animator.SetTrigger(4);

    }

    public override void OnExit()
    {

    }

    public override void OnHurt()
    {

    }

    public override void UpdateState()
    {

    }
    public override void FixedUpdateState()
    {

    }
}
