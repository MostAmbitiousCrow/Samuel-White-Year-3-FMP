public interface ITargetsBoat
{
    public Boat_Space_Manager SpaceManager { get; set; }

    public void InjectBoatSpaceManager(Boat_Space_Manager bsm);
}
