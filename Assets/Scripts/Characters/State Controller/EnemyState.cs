public abstract class EnemyState : IState
{
    public EnemyStateController Sc { get; set; }
    /// <summary>
    /// Is called whenever this state has been entered
    /// </summary>
    public abstract void OnEnter();
    /// <summary>
    /// Is called whenever this state has been exited
    /// </summary>
    public abstract void OnExit();
    /// <summary>
    /// On Hurt is called whenever the enemy is damaged. It will check if the health has reached below zero, so call the base last
    /// </summary>
    public virtual void OnHurt()
    {
        if (Sc.HealthComponent.CurrentHealth <= 0)
            Sc.ChangeState(Sc.DefeatedState);
    }
    /// <summary>
    /// Called every frame from the State Controller
    /// </summary>
    public abstract void UpdateState();
    /// <summary>
    /// Called every fixed frame from the State Controller
    /// </summary>
    public abstract void FixedUpdateState();
}
