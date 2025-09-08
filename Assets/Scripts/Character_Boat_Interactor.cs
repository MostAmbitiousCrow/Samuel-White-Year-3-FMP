using UnityEngine;

public class Character_Boat_Interactor : MonoBehaviour
{
    [Header("Components")]
    public Boat_Controller boatController;

    [Header("Character Settings")]
    [SerializeField] float weight = 1;
    [SerializeField] bool canMoveBoat;

    public void ImpactBoat(int space)
    {
        //TODO: Move the boat in the direction of the side of the boat the character is stood on
        if (space > 1)
        {
            boatController.SteerBoat(canMoveBoat? 1: 0, weight);
        }
        else if (space < 1) boatController.SteerBoat(canMoveBoat? -1: 0, weight);
    }
}
