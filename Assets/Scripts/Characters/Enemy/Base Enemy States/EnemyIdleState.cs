public class EnemyIdleState : EnemyState
{
    public override void OnEnter()
    {
        Sc.Animator.SetTrigger("Idle");

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
