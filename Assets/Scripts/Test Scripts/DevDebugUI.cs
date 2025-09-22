using TMPro;
using UnityEngine;

public class DevDebugUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Player_Controller player_Controller;
    [SerializeField] Boat_Controller boat_Controller;
    [SerializeField] River_Manager river_Manager;

    [Header("Components")]
    [SerializeField] TextMeshProUGUI _riverText;

    // Start is called before the first frame update
    void Start()
    {
        if (player_Controller == null || boat_Controller == null || river_Manager == null)
        {
            player_Controller = FindObjectOfType<Player_Controller>();
            boat_Controller = FindObjectOfType<Boat_Controller>();
            river_Manager = FindObjectOfType<River_Manager>();
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

    public void SpeedUpRiver()
    {
        river_Manager.SpeedUpRiver();
    }

    public void SlowDownRiver()
    {
        river_Manager.SlowDownRiver();
    }

    public void ResetRiver()
    {
        river_Manager.ResetRiver();
    }

    private void Update()
    {
        if (river_Manager != null)
        {
            _riverText.SetText($"River Speed: {river_Manager.RiverSpeed}");
        }
    }
}
