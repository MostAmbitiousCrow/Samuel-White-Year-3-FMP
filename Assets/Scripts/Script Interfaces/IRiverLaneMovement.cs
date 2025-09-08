public interface IRiverLaneMovement
{
    /// <summary>
    /// Moves the object towards the lane next to its current lane based on a given direction
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="speed"></param>
    public void MoveTowardsLane(int direction);
    public void GoToLane(int lane);

    public int GetCurrentLane();
}
