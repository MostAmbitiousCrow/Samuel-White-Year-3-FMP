public class EnemyIdleState : EnemyState
{
    public override void OnEnter()
    {
        // Sc.Animator.SetTrigger("Idle"); //TODO: The reference to the State Controller stops existing for some reason...

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
