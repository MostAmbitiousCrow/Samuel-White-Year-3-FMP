using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Game_UI : MonoBehaviour
{
    #region Subscriptions
    private void Start()
    {
        GameManager.GameLogic.OnGemstoneCollected += UpdateGemstoneCounter;
        GameManager.GameLogic.OnPlayerDamaged += UpdatePlayerHealthMeter;
    }
    private void OnEnable()
    {
        GameManager.GameLogic.OnGemstoneCollected += UpdateGemstoneCounter;
        GameManager.GameLogic.OnPlayerDamaged += UpdatePlayerHealthMeter;
    }

    private void OnDisable()
    {
        GameManager.GameLogic.OnGemstoneCollected -= UpdateGemstoneCounter;
        GameManager.GameLogic.OnPlayerDamaged -= UpdatePlayerHealthMeter;
    }

    #endregion

    [Header("Player UI")]
    #region Player Health UI
    [SerializeField] Slider _playerHealthSlider;
    [SerializeField] TextMeshProUGUI _debugHealthText;

    /// <summary> Fuction to update the UI for the players health bar. Parameter must be the players current health. </summary>
    public void UpdatePlayerHealthMeter(int health)
    {
        if (health < (int)_playerHealthSlider.minValue || health > (int)_playerHealthSlider.maxValue) return;
        
        _playerHealthSlider.value = health;
    }

    #endregion

    #region Gemstone Counter
    [SerializeField] TextMeshProUGUI _gemstoneCounterText;

    /// <summary> Fuction to update the UI for the Gemstone Counter. Parameter must be the current Gemstone count. </summary>
    public void UpdateGemstoneCounter(int gemstones)
    {
        if(gemstones <= 0) _gemstoneCounterText.SetText($"Gemstones: {0}");

        _gemstoneCounterText.SetText($"Gemstones: {gemstones}");
        //print($"Updated Gemstone Text: {gemstones}");
    }

    #endregion

    [Header("Game UI")]
    #region Player Progress Meter
    [SerializeField] Slider _playerProgressSlider;

    /// <summary> Fuction to update the UI for the Players progress meter. Parameter must be the Players current progress </summary>
    public void UpdatePlayerProgressMeter(float progress)
    {
        _playerProgressSlider.value = progress;


    }
    #endregion

    #region Tsunami Meter
    [SerializeField] Slider _tsunamiProgressSlider;

    /// <summary> Fuction to update the UI for the Tsunamis progress meter. Parameter must be the Tsunamis current progress </summary>
    public void UpdateTsunamiMeter(float progress)
    {
        _tsunamiProgressSlider.value = progress;


    }
    #endregion

    #region UI
    /// <summary> Fuction to completely reset the Game and Player UI to defaults. </summary>

    public void ResetUI()
    {
        UpdateGemstoneCounter(0);
        UpdatePlayerHealthMeter(3); // TODO: expose players max health so UI can be updated accordingly
        UpdatePlayerProgressMeter(0);
        UpdateTsunamiMeter(0);
    }

    #endregion
}
