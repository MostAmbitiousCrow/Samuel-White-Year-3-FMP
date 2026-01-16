using UnityEngine;

public class EnemyDefeatedState : EnemyState
{
    public override void OnEnter()
    {
        Sc.Animator.SetTrigger("Defeated");

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
