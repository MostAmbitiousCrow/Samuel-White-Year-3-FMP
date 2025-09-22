using UnityEngine;

public class PlayerAndBoatTriggersTestScript : MonoBehaviour
{
    [SerializeField] Player_Controller player_Controller;
    [SerializeField] Boat_Controller boat_Controller;

    // Start is called before the first frame update
    void Start()
    {
        if (player_Controller == null || boat_Controller == null)
        {
            player_Controller = FindObjectOfType<Player_Controller>();
            boat_Controller = FindObjectOfType<Boat_Controller>();
        }
    }

    public void DamagePlayer(int amount)
    {
        player_Controller.TakeDamage(amount);
    }

    public void DamageBoat(int amount)
    {
        boat_Controller.TakeDamage(amount);
    }

    public void ResetCharacters()
    {
        player_Controller.RestoreHealth();
        boat_Controller.RestoreHealth();
    }
}
