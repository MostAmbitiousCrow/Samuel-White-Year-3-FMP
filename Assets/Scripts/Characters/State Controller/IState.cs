public interface IState
{
    /// <summary>
    /// Is called whenever this state has been entered
    /// </summary>
    public void OnEnter();

    /// <summary>
    /// Is called every Update by the State Controller
    /// </summary>
    public void UpdateState();

    /// <summary>
    /// Is called every Fixed Update by the State Controller
    /// </summary>
    public void FixedUpdateState();

    /// <summary>
    /// Is called whenever the character has been damaged
    /// </summary>
    public void OnHurt();

    /// <summary>
    /// Is called whenever this state has been exited
    /// </summary>
    public void OnExit();
}
