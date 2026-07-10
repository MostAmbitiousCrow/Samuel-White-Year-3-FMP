using System;
using Game;
using TMPro;
using UnityEngine;

public class ExhibitModeManager : MonoBehaviour
{
    [Header("Controls")]
    [Tooltip("The time of each play session in minutes before force-completing the game")]
    [SerializeField] private float playTime = 10;
    [SerializeField] private float _currentTime;
    [SerializeField] private bool _isCompleted;
    [SerializeField] private float timerTextScale = 100;

    [Header("Components")]
    [SerializeField] private TextMeshProUGUI visibleTimer;
    
    public static ExhibitModeManager Instance;

    private void Start()
    {
        if (!Instance) Instance = this;
        else { Destroy(gameObject); return; }
        
        visibleTimer.enabled = GameSettingsManager.DoExhibitMode;
        _isCompleted = true;
        _currentTime = playTime * 60f;
        
        UpdateTimer();
        GameManager.GameLogic.onGameInitialised += StartTimer;
        GameManager.GameLogic.OnGameCompleted += TriggerForceGameOver;
    }

    // private void OnEnable()
    // {
    //     GameManager.GameLogic.onGameInitialised += StartTimer;
    // }

    private void OnDisable()
    {
        GameManager.GameLogic.onGameInitialised -= StartTimer;
        GameManager.GameLogic.OnGameCompleted -= TriggerForceGameOver;
    }

    public void UpdateExhibitMode()
    {
        visibleTimer.enabled = GameSettingsManager.DoExhibitMode;
        UpdateTimer();
        Debug.Log($"Exhibitor Mode: {GameSettingsManager.DoExhibitMode}");
    }

    public void StartTimer()
    {
        // if (!GameSettingsManager.DoExhibitMode) return;
        
        _isCompleted = false;
        _currentTime = playTime * 60f;
        Debug.Log($"Started Exhibitor Timer. Is Exhibiting: {GameSettingsManager.DoExhibitMode}");
    }

    /*private void ResetTimer()
    {
        _isCompleted = false;
        _currentTime = playTime * 60f;
        
        Debug.Log($"Reset Exhibitor Timer. Is Exhibiting: {GameSettingsManager.DoExhibitMode}");
    }*/

    private void Update()
    {
        if (!GameSettingsManager.DoExhibitMode || _isCompleted) return;
        
        _currentTime -= Time.unscaledDeltaTime;

        if (_currentTime < 0f)
        {
            TriggerForceGameOver();
        }

        UpdateTimer();
    }

    private void TriggerForceGameOver()
    {
        _isCompleted = true;
        _currentTime = 0f;

        GameManager.GameLogic.EndGame();

        if (!River_Manager.Instance) return;
        
        // River_Manager.Instance.SetRiverSpeed(0);
        River_Manager.Instance.PauseRiver();
    }

    private void UpdateTimer()
    {
        var mins = TimeSpan.FromSeconds(_currentTime);
        // var secs = TimeSpan.FromSeconds(_currentTime);
        var txt = $"Exhibitor Mode:\n<size={timerTextScale}%>{mins.Minutes}:{mins.Seconds}";
        
        visibleTimer.SetText(txt);
    }

    #if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying) return;
        
        _currentTime = playTime * 60;
        UpdateTimer();
    }
    #endif
}
