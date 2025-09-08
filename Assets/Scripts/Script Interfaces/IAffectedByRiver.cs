
public interface IAffectedByRiver
{
    /// <summary>
    /// Used by the River Manager to inject the script that includes the IAffectedByRiver interface with a reference to the River Manager
    /// </summary>
    public void InjectRiverManager(River_Manager manager);
}
