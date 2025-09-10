public interface IBoatSpaceMovement
{
    public void MoveToSpace(int direction, float speed);
    public void GoToSpace(int lane, int space);
    public void VaultToSpace(int lane, int space, float speed);

    public int GetCurrentSpace();
    public int GetCurrentLane();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="goToCurrentSpace"></param> <summary>
    /// Should the character snap to their currently assigned space upon leaving the boat?
    /// </summary>
    /// <param name="goToCurrentSpace"></param>
    public void EnterBoat(bool goToCurrentSpace);
    public void ExitBoat(bool goToCurrentSpace);

    public void InjectBoatSpaceManager(Boat_Space_Manager manager);
}
