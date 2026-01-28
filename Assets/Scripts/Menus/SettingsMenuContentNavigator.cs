using EditorAttributes;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SettingsMenuContentNavigator : ScreenContentNavigator
{
    #region Variables
    
    [FoldoutGroup("Audio", nameof(audioMixer), 
        nameof(masterVolumeSlider), nameof(musicVolumeSlider), nameof(sfxVolumeSlider))]
    [SerializeField] private Void audioFolder;
    [SerializeField, HideProperty] private AudioMixer audioMixer;
    [SerializeField, HideProperty] private Slider masterVolumeSlider;
    [SerializeField, HideProperty] private Slider musicVolumeSlider;
    [SerializeField, HideProperty] private Slider sfxVolumeSlider;
    
    [FoldoutGroup("Gameplay", nameof(invincibilityToggle), 
        nameof(dyslexicToggle), nameof(rainbowModeToggle))]
    [SerializeField] private Void gameplayFolder;
    [SerializeField, HideProperty] private Toggle invincibilityToggle;
    [SerializeField, HideProperty] private Toggle dyslexicToggle;
    [SerializeField, HideProperty] private Toggle rainbowModeToggle;
    
    #endregion
    #region Settings
    #region Gameplay Settings

    public void ToggleInvincibility()
    {
        
    }

    public void ToggleRainbowMode()
    {
        
    }
    

    #endregion
    
    #region System Settings

    public void UpdateVolume(float volume)
    {
        
    }
    
    #endregion
    #endregion
}
