using EditorAttributes;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Main_Menu_Transition_Controller))]
public class Main_Menu_Manager : MonoBehaviour // By Samuel White
{
    /*
    ========================================
    Main Menu Manager:
    Manages the collection of Screens along
    with additional sounds for buttons

    Features:
    Sounds that play upon entering/exiting a
    scene.
    Toggles for requierd menu sections.
    Sounds that will play upon selecting
    buttons
    Navigation button selection for 
    Gamepad/Keyboard controls.
    
    Depends on the Main_Menu_Transition_Controller
    for triggering any transitions you wish
    to add to your main menu. You will need
    to add the code yourself, but message
    me (mostambitiouscrow) on Discord if
    you need help!
    ========================================
    */

    #region Variables
    [Header("Components")]
    [SerializeField] EventSystem _eventSystem;
    [SerializeField] AudioSource _audioSource;

    [Header("Transition Components")]
    //[SerializeField] Settings_Menu_Manager settingsMenuManager;
    [SerializeField] Main_Menu_Transition_Controller _transitionArtController;

    [Header("Menu Data")]
    [SerializeField, ReadOnly] MainMenuScreenContent[] screenDatas;
    [SerializeField, ReadOnly] MenuScreenTypes currentScreen;

    #endregion

    private void Awake()
    {
        Main_Menu_Transition_Controller.OnTransitionStarted += ScreenOpened;
        Main_Menu_Transition_Controller.OnTransitionWaiting += ToggleScreen;
        Main_Menu_Transition_Controller.OnTransitionWaitCompleted += ScreenClosed;
        //Main_Menu_Transition_Controller.OnTransitionCompleted +=  // Something
    }

    private void Start()
    {
        foreach (var item in screenDatas)
            item.ScreenRoot.SetActive(false);

        screenDatas[(int)currentScreen].ScreenRoot.SetActive(true);

        currentScreen = MenuScreenTypes.StartScreen;
        _eventSystem.SetSelectedGameObject(screenDatas[(int)currentScreen].EnterButton.gameObject);
    }

    #region Screen Methods
    public void InvokeScreen(int type)
    {
        MainMenuScreenContent screen = screenDatas[(int)currentScreen];
        MainMenuScreenContent newScreen = screenDatas[type];
        //MainMenuScreenContent newScreen = screenDatas[(int)(MenuScreenTypes)Enum.Parse(typeof(MenuScreenTypes), type)];


        ScreenClosed(screen);
        _transitionArtController.TriggerTransition(newScreen, screen);
    }

    void ToggleScreen(MainMenuScreenContent openingScreen, MainMenuScreenContent closingScreen)
    {
        // Disable closing and Enable opening additional screen content
        foreach (var item in openingScreen.AdditionalScreenContent) { item.SetActive(true); }
        foreach (var item in closingScreen.AdditionalScreenContent) { item.SetActive(false); }
        
        // Disable closing scene and enable opening screen
        openingScreen.ScreenRoot.SetActive(true);
        closingScreen.ScreenRoot.SetActive(false);

        /* // Optionally choose to determine if the pages main button should be selected if using Keyboard or Gamepad controls
        if (// Insert check for player input here)
        {
            
        }
        */

        // Select the opening pages button
        currentScreen = openingScreen.MenuScreen;
        if (closingScreen.UseExitButton) _eventSystem.SetSelectedGameObject(closingScreen.ExitButton.gameObject);
        else _eventSystem.SetSelectedGameObject(openingScreen.EnterButton.gameObject);
    }

    void ScreenOpened(MainMenuScreenContent screen)
    {
        if(screen.EnterSFX) _audioSource.PlayOneShot(screen.EnterSFX);
        ToggleInput(false);
        screen.TriggerEvent.Invoke();
    }

    void ScreenClosed(MainMenuScreenContent screen)
    {
        if(screen.ExitSFX) _audioSource.PlayOneShot(screen.ExitSFX);
        ToggleInput(true);
    }
    #endregion

    #region Quit Game
    public void QuitGame()
    {
        if (Application.platform != RuntimePlatform.WebGLPlayer)
            Application.Quit(); // Quit the game
        Debug.Log("Player has quit the game.");
    }
    #endregion

    #region Game Initialisation
    public void PlayGame()
    {
        Debug.Log("Play Game Button triggered");
        ToggleInput(false);

        // Load the main game scene or trigger your scene load transition here
    }
    #endregion

    #region Input Toggle
    void ToggleInput(bool state)
    {
        _eventSystem.enabled = state;
    }
    #endregion

    #region Play Sounds
    // ============================= Play Sounds =============================

    /*
    public void PlaySound_UIHover() => AudioManager.PlayInterfaceSound(InterfaceCategory.InterfaceSoundTypes.Button_Hover, .5f);
    public void PlaySound_UIPress() => AudioManager.PlayInterfaceSound(InterfaceCategory.InterfaceSoundTypes.Button_Press, .5f);
    public void PlaySound_UIBack() => AudioManager.PlayInterfaceSound(InterfaceCategory.InterfaceSoundTypes.Button_Back, .5f);
    public void PlaySound_UIStartGame() => AudioManager.PlayInterfaceSound(InterfaceCategory.InterfaceSoundTypes.Button_GameStart, .5f);
    */

    #endregion

    #region OnValidation
    private void OnDrawGizmos()
    {
        if (Application.isPlaying) return;

        screenDatas = null;
        screenDatas = FindObjectsOfType<MainMenuScreenContent>();

        // Sort screenDatas by the MenuScreenTypes enum order
        Array.Sort(screenDatas, (a, b) => a.MenuScreen.CompareTo(b.MenuScreen));

        if (!_audioSource) _audioSource = GetComponent<AudioSource>();
    }

    #endregion
}