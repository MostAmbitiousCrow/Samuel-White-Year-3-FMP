using UnityEngine;

public interface IRiverLaneMovement
{
    /// <summary>
    /// The current lane on the river this character is on
    /// </summary>
    public River_Manager.RiverLane CurrentLane { get; set; }

    /// <summary>
    /// Moves the object towards the lane next to its current lane based on a given direction
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="speed"></param>
    public void MoveToLaneFromDirection(int direction);

    /// <summary>
    /// Updates the objects lane target and allows it to move towards that lane space.
    /// </summary>
    /// <param name="lane"></param>
    public void MoveToLane(int lane);
    /// <summary>
    /// Immediately updates the objects lane target without triggering movement.
    /// </summary>
    /// <param name="lane"></param>
    public void GoToLane(int lane);
}
