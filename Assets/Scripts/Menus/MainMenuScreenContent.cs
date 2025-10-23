using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using EditorAttributes;

public class MainMenuScreenContent : MonoBehaviour
{
    /// <summary> The type of Screen </summary>
    public MenuScreenTypes MenuScreen { get { return _menuScreen; } }
    [Tooltip("The type of Screen")]
    [SerializeField] private MenuScreenTypes _menuScreen;

    [Header("Page contents")]
    /// <summary> The root object of the screen </summary>
    [SerializeField, ReadOnly] private GameObject _screenRoot;
    [Tooltip("The root object of the screen")]
    public GameObject ScreenRoot { get { return _screenRoot; } }

    /// <summary> The GameObject(s) containing the contents of the Screen </summary>
    public GameObject[] AdditionalScreenContent { get { return _additionalScreenContent; } }
    [Tooltip("The GameObject(s) containing the contents of the Screen")]
    [SerializeField] GameObject[] _additionalScreenContent;

    /// <summary> The UI Button to be selected upon entering the Screen </summary>
    public Button EnterButton { get { return _enterButton; } }
    [Tooltip("The UI Button to be selected upon entering the Screen")]
    [SerializeField] Button _enterButton;

    /// <summary> Should there be a button to select upon exiting this screen </summary>
    public bool UseExitButton { get { return _useExitButton; } }
    [SerializeField] bool _useExitButton;
    public Button ExitButton { get { return _exitButton; } }
    [SerializeField, ShowField(nameof(_useExitButton))] Button _exitButton;

    [Header("Sounds")]
    [Tooltip("The sound to play upon entering this screen")]
    [SerializeField] AudioClip _enterSFX;
    /// <summary> The sound to play upon entering this screen </summary>
    public AudioClip EnterSFX { get { return _enterSFX; } }

    /// <summary> The sound to play upon exiting this screen </summary>
    public AudioClip ExitSFX { get { return _exitSFX; } }
    [Tooltip("The sound to play upon exiting this screen")]
    [SerializeField] AudioClip _exitSFX;

    [Space]

    [Tooltip("An additional event that will trigger upon entering this screen")]
    [SerializeField] UnityEvent _triggerEvent;
    /// <summary> An additional event that will trigger upon entering this screen </summary>
    public UnityEvent TriggerEvent { get { return _triggerEvent; } }

    private void OnValidate()
    {
        if (!_screenRoot)
            _screenRoot = gameObject;
        _screenRoot.name = new string($"--- Screen ({_menuScreen} = {(int)_menuScreen}) ---");
    }
}

// NOTE: Any additional titles you want to add, just add a value to this enum
public enum MenuScreenTypes
{
    StartScreen = 0, TitleScreen = 1, SettingsScreen = 2, CreditsScreen = 3
}
