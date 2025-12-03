public abstract class EnemyState : IState
{
    public EnemyStateController Sc { get; set; }
    public abstract void OnEnter();
    public abstract void OnExit();
    public abstract void OnHurt();
    public abstract void UpdateState();
    public abstract void FixedUpdateState();
}
