using GameCharacters;
using TMPro;
using UnityEngine;

public class Game_UI : MonoBehaviour
{
    public static Game_UI Instance;
    private static readonly int Alpha = Shader.PropertyToID("_Alpha");

    #region Subscriptions
    private void Awake()
    {
        Instance = this;
        _storedTime = Time.time;
    }
    private void Start()
    {
        GameManager.GameLogic.OnGemstoneCollected += UpdateGemstoneCounter;
        PlayerCharacter.OnPlayerDamaged += CheckPlayerHealth;
        PlayerCharacter.OnPlayerDied += _ => ResetHealthBorder();
        GameLevelManager.OnLevelLoaded += ResetHealthBorder;
    }
    private void OnEnable()
    {
        GameManager.GameLogic.OnGemstoneCollected += UpdateGemstoneCounter;
        PlayerCharacter.OnPlayerDamaged += CheckPlayerHealth;
        PlayerCharacter.OnPlayerDied += _ => ResetHealthBorder();
        GameLevelManager.OnLevelLoaded += ResetHealthBorder;
    }

    private void OnDisable()
    {
        GameManager.GameLogic.OnGemstoneCollected -= UpdateGemstoneCounter;
        PlayerCharacter.OnPlayerDamaged -= CheckPlayerHealth;
        PlayerCharacter.OnPlayerDied -= _ => ResetHealthBorder();
        GameLevelManager.OnLevelLoaded -= ResetHealthBorder;
    }

    #endregion

    [Header("Player UI")]
    #region Player Health UI
    [SerializeField] private Material visualHealthBorder;
    [SerializeField] private float fadeDuration = 3.25f;
    private float _storedTime;
    private bool _isHealthInDanger;

    /// <summary>  </summary>
    private void CheckPlayerHealth(int health)
    {
        // If on their last hit point, set health in danger true
        _isHealthInDanger = health == 1;
        _storedTime = Time.time + fadeDuration; // Duration of the fade based on player invincibility duration
        // TODO: ^ Link this up to the player invincibility duration?
    }

    private void ResetHealthBorder()
    {
        visualHealthBorder.SetFloat(Alpha, 0f);
        _isHealthInDanger = false;
    }

    private void Update()
    {
        var progress = _storedTime - Time.time;

        if (progress < 0f) return;
        
        var value = Mathf.InverseLerp(0f, fadeDuration, progress);

        var target = _isHealthInDanger ? .15f : 0f;
        var start = _isHealthInDanger ? 0f : .15f;
        var lerp = Mathf.Lerp(target, start, value);
        visualHealthBorder.SetFloat(Alpha, lerp);
        
        // Debug.Log($"Health in Danger = {_isHealthInDanger}. Updating Health Border: Progress = {progress} Lerp = {lerp}");
    }

    #endregion

    #region Gemstone Counter
    [SerializeField] private TextMeshProUGUI gemstoneCounterText;

    /// <summary> Fuction to update the UI for the Gemstone Counter. Parameter must be the current Gemstone count. </summary>
    private void UpdateGemstoneCounter(int gemstones)
    {
        if (!gemstoneCounterText)
        {
            Debug.LogError("Gemstone Counter Text is Missing!");
            return;
        }
        if(gemstones <= 0) gemstoneCounterText.SetText("Gemstones: 0");

        gemstoneCounterText.SetText($"Gemstones: {gemstones}");
        //print($"Updated Gemstone Text: {gemstones}");
    }

    #endregion
}
