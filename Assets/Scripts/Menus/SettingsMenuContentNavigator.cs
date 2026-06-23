using EditorAttributes;
using Game;
using UnityEngine;
using UnityEngine.UI;
using Void = EditorAttributes.Void;

public class SettingsMenuContentNavigator : ScreenContentNavigator
{
    #region Variables
    
    // [FoldoutGroup("Audio", nameof(audioMixer), 
    //     nameof(masterVolumeSlider), nameof(musicVolumeSlider), nameof(sfxVolumeSlider))]
    // [SerializeField] private Void audioFolder;
    // [SerializeField, HideProperty] private AudioMixer audioMixer;
    // [SerializeField, HideProperty] private Slider masterVolumeSlider;
    // [SerializeField, HideProperty] private Slider musicVolumeSlider;
    // [SerializeField, HideProperty] private Slider sfxVolumeSlider;
    
    [FoldoutGroup("Gameplay", nameof(invincibilityToggle), 
        nameof(dyslexicToggle), nameof(fovToggle), nameof(screenShakeToggle), nameof(hitFreezeToggle),
        nameof(depthAssistToggle), nameof(rainbowModeToggle), nameof(alternativeControlSchemeToggle))]
    [SerializeField] private Void gameplayFolder;
    [SerializeField, HideProperty] private Toggle invincibilityToggle, dyslexicToggle, fovToggle, screenShakeToggle,
        hitFreezeToggle, depthAssistToggle, rainbowModeToggle, alternativeControlSchemeToggle;
    
    #endregion
    
    #region Settings
    #region Gameplay Settings
    
    private void Awake()
    {
        UpdateToggles();
    }

    private void OnEnable()
    {
        UpdateToggles();
    }

    private void UpdateToggles()
    {
        // Invincibility
        invincibilityToggle.isOn = GameSettingsManager.DoPlayerInvincibility;
        invincibilityToggle.onValueChanged.RemoveAllListeners();
        invincibilityToggle.onValueChanged.AddListener(OnPlayerInvincibilityValueChange);
        
        // Dyslexic Font
        dyslexicToggle.isOn = GameSettingsManager.DoDyslexiaFont;
        dyslexicToggle.onValueChanged.RemoveAllListeners();
        dyslexicToggle.onValueChanged.AddListener(OnDyslexicFontValueChange);
        
        // FOV
        fovToggle.isOn = GameSettingsManager.DoFovSliding;
        fovToggle.onValueChanged.RemoveAllListeners();
        fovToggle.onValueChanged.AddListener(OnFovSlideValueChange);
        
        // Screen Shake
        screenShakeToggle.isOn = GameSettingsManager.DoScreenShake;
        screenShakeToggle.onValueChanged.RemoveAllListeners();
        screenShakeToggle.onValueChanged.AddListener(OnScreenShakeValueChange);
        
        // Hit Freeze
        hitFreezeToggle.isOn = GameSettingsManager.DoHitFreeze;
        hitFreezeToggle.onValueChanged.RemoveAllListeners();
        hitFreezeToggle.onValueChanged.AddListener(OnHitFreezeValueChange);
        
        // Depth Assist
        depthAssistToggle.isOn = GameSettingsManager.DoDepthAssist;
        depthAssistToggle.onValueChanged.RemoveAllListeners();
        depthAssistToggle.onValueChanged.AddListener(OnDoDepthAssistValueChange);
        
        // Alternative Control Scheme
        alternativeControlSchemeToggle.isOn = GameSettingsManager.UseAlternativeControlScheme;
        alternativeControlSchemeToggle.onValueChanged.RemoveAllListeners();
        alternativeControlSchemeToggle.onValueChanged.AddListener(OnUseAlternativeControlSchemeValueChange);
        
        // Rainbow Mode
        rainbowModeToggle.isOn = GameSettingsManager.DoRainbowMode;
        rainbowModeToggle.onValueChanged.RemoveAllListeners();
        rainbowModeToggle.onValueChanged.AddListener(OnRainbowModeValueChange);
    }

    public void OnPlayerInvincibilityValueChange(bool value)
    {
        GameSettingsManager.DoPlayerInvincibility = value;
    }

    public void OnDyslexicFontValueChange(bool value)
    {
        GameSettingsManager.DoDyslexiaFont = value;
        Debug.Log("Dyslexic font changed");
    }
    
    public void OnFovSlideValueChange(bool value)
    {
        GameSettingsManager.DoFovSliding = value;
    }

    public void OnScreenShakeValueChange(bool value)
    {
        GameSettingsManager.DoScreenShake = value;
    }

    public void OnHitFreezeValueChange(bool value)
    {
        GameSettingsManager.DoHitFreeze = value;
    }

    public void OnDoDepthAssistValueChange(bool value)
    {
        GameSettingsManager.DoDepthAssist = value;
    }

    public void OnUseAlternativeControlSchemeValueChange(bool value)
    {
        GameSettingsManager.UseAlternativeControlScheme = value;
    }
    
    public void OnRainbowModeValueChange(bool value)
    {
        GameSettingsManager.DoRainbowMode = value;
    }
    

    #endregion
    
    #region System Settings

    public void UpdateVolume(float volume)
    {
        
    }
    
    #endregion
    #endregion
}
