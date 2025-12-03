public class EnemyIdleState : EnemyState
{
    public override void OnEnter()
    {
        Sc.IsMoving = false;
        Sc.Animator.SetTrigger(0);

    }

    public override void OnExit()
    {

    }

    public override void OnHurt()
    {
        Sc.ChangeState(Sc.DefeatedState);
    }

    public override void UpdateState()
    {

    }
    public override void FixedUpdateState()
    {

    }
}
