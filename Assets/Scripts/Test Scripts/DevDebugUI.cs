using GameCharacters;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class DevDebugUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerCharacter playerController;
    [SerializeField] private Boat_Controller boatController;
    [SerializeField] private River_Manager riverManager;

    [Header("Components")]
    [SerializeField] TextMeshProUGUI riverText;

    // Start is called before the first frame update
    void Start()
    {
        if (playerController == null || boatController == null || riverManager == null)
        {
            playerController = FindFirstObjectByType<PlayerCharacter>();
            boatController = FindFirstObjectByType<Boat_Controller>();
            riverManager = FindFirstObjectByType<River_Manager>();
        }
    }

    public void DamagePlayer(int amount)
    {
        playerController.HealthComponent.TakeDamage(amount: amount);
    }

    public void DamageBoat(int amount)
    {
        playerController.HealthComponent.TakeDamage(amount: amount);
    }

    public void ResetCharacters()
    {
        playerController.HealthComponent.RestoreHealth();
        playerController.HealthComponent.RestoreHealth();
    }

    public void SpeedUpRiver()
    {
        riverManager.SpeedUpRiver();
    }

    public void SlowDownRiver()
    {
        riverManager.SlowDownRiver();
    }

    public void ResetRiver()
    {
        riverManager.ResetRiver();
    }

    private void Update()
    {
        if (riverManager != null)
        {
            riverText.SetText($"River Speed: {riverManager.riverFlowSpeed}");
        }
    }
}
