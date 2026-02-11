using UnityEngine;
using static Boat_Space_Manager.BoatSide;

public class Character_Boat_Interactor : MonoBehaviour
{
    [Header("Components")]
    public Boat_Controller boatController;

    [Header("Character Settings")]
    [SerializeField] float weight = 1;
    [SerializeField] bool canMoveBoat;


    private void OnEnable()
    {
        if (boatController == null)
        {
            boatController = FindObjectOfType<Boat_Controller>();
            Debug.LogWarning($"{name} was missing {boatController}, located and injected");
        }
    }

    public void ImpactBoat(SpaceData spaceData)
    {
        if (!canMoveBoat) return;
        
        boatController.SteerBoat(spaceData, weight);
    }
}
