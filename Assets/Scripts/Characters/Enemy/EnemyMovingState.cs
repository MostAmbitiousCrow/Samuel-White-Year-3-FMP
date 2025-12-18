using UnityEngine;

public class EnemyMovingState : EnemyState
{
    public override void OnEnter()
    {
        Sc.IsMoving = true;
        Sc.Animator.SetTrigger(2);

    }

    public override void OnExit()
    {
        Sc.IsMoving = false;
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
