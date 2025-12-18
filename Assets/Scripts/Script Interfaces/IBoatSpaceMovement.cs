public interface IBoatSpaceMovement
{
    public void MoveToSpace(int side, int space);
    public void GoToSpace(int side, int space);
    public void GoToSideSpace(int side, bool goLeftSide = true);
    public void GoToBoatSpace(int side, int space);
    public void MoveToSpaceInDirection(int direction);
    public void VaultToSpace(Boat_Space_Manager.BoatSide.SpaceData spaceData);

    public Boat_Space_Manager.BoatSide.SpaceData GetCurrentSpaceData();
    //public int GetCurrentSide();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="goToCurrentSpace"></param> <summary>
    /// Should the character snap to their currently assigned space upon leaving the boat?
    /// </summary>
    /// <param name="goToCurrentSpace"></param>
    public void EnterBoat(bool goToCurrentSpace);
    public void ExitBoat(bool goToCurrentSpace);

    //public void InjectBoatSpaceManager(Boat_Space_Manager manager); // Removing Dependency Injection
}
