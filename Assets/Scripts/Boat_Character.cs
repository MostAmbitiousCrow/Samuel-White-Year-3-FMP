using UnityEngine;

public class Boat_Character : MonoBehaviour
// Boat Characters are any characters that are able to interact and move with the players boat
{
    [Tooltip("How fast the character will step towards an open space.")]
    public float stepSpeed = 1f;
    [Tooltip("How fast the character will vault over the wall to the opposite lane.")]
    public float vaultSpeed = 1f;
    [Tooltip("The current space on the boat the character is standing on")]
    [SerializeField] int _currentSpace;
    [Tooltip("The current lane of the boat the character is standing on")]
    [SerializeField] int _currentLane;
    [Tooltip("What space on the boat should the character start on (if applicable)")]
    public int startSpace = 1;

    public bool _isVaulting;
    #region Space Movement Logic

    public void MoveToSpace(int direction, float speed)
    {
        Boat_Space_Manager.BoatSide.SpaceData sd = Boat_Space_Manager.GetSpaceFromDirection(_currentLane, _currentSpace, direction);
        _currentSpace = sd.ID;
        transform.position = sd.position; //TODO: Add movement interpolation
        print($"Moved {direction} to Space Position: {sd.position}, ID {sd.ID}");
    }

    public void VaultToSpace(int lane, int space, float speed)
    {
        Boat_Space_Manager.BoatSide.SpaceData sd = Boat_Space_Manager.GetSpace(lane, space);
        _currentSpace = sd.ID;
        _currentLane = Boat_Space_Manager.GetOppositeLaneID(_currentLane);
        transform.position = sd.position; //TODO: Add vaulting movement interpolation
    }

    public void GoToSpace(int lane, int space)
    {
        Boat_Space_Manager.BoatSide.SpaceData sd = Boat_Space_Manager.GetSpace(lane, space);
        _currentSpace = sd.ID;
        transform.position = sd.position;
    }

    public int GetCurrentSpace()
    {
        return _currentSpace;
    }

    public int GetCurrentLane()
    {
        return _currentLane;
    }
    #endregion
}
