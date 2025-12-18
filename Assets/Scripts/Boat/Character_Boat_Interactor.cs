using UnityEngine;

public class Character_Boat_Interactor : MonoBehaviour
{
    [Header("Components")]
    public Boat_Controller boatController;

    [Header("Character Settings")]
    [SerializeField] float weight = 1;
    [SerializeField] bool canMoveBoat;


    void OnEnable()
    {
        if (boatController == null)
        {
            boatController = FindObjectOfType<Boat_Controller>();
            Debug.LogWarning($"{name} was missing {boatController}, located and injected");
        }
    }

    public void ImpactBoat(int space)
    {
        //TODO: Move the boat in the direction of the side of the boat the character is stood on
        int count = Boat_Space_Manager.Instance.SpaceCount;
        if (space == count - 2) // Steer Left
        {
            boatController.SteerBoat(canMoveBoat ? -1 : 0, weight);
        }
        else if (space == 1) // Steer Right
        {
            boatController.SteerBoat(canMoveBoat ? 1 : 0, weight);
        }
    }
}
