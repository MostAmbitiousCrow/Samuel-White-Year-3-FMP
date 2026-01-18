using UnityEngine;

public class EnemyMovingState : EnemyState
{
    public override void OnEnter()
    {
        Sc.Animator.SetTrigger("Move");

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
