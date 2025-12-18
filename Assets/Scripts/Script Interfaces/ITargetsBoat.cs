public interface ITargetsBoat // Scrapped
{
    /// <summary>
    /// Used by the Boat Space Manager to inject the script that includes the ITargetsBoat interface with a reference to the Boat Space Manager.
    /// Don't forget to add a Boat_Space_Manager variable to the script with this interface.
    /// </summary>
    public void InjectBoatSpaceManager(Boat_Space_Manager bsm);
}
