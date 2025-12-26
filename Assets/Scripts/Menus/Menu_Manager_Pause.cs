using EditorAttributes;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class Menu_Manager_Pause : Menu_Manager
{

    [Button("Sort Pause Screens")]
    public void SortScreens()
    {
        screenDatas = GetComponentsInChildren<MenuScreenContent>(true);

        // Filter to only MenuScreenContent_Pause and sort by PauseMenuScreenTypes order
        var sorted = screenDatas
            .OfType<MenuScreenContent_Pause>()
            .OrderBy(s => s.MainScreenTypes)
            .Cast<MenuScreenContent>()
            .ToArray();

        screenDatas = sorted;
    }

    private void Start()
    {
        SortScreens();

        if (!_audioSource) _audioSource = GetComponent<AudioSource>();

        foreach (var item in screenDatas)
            item.ScreenRoot.SetActive(false);

        if (screenDatas.Length > _startScreen)
            screenDatas[_startScreen].ScreenRoot.SetActive(true);
        else
            Debug.LogError("Start screen index out of range!");
        screenDatas[_startScreen].ScreenRoot.SetActive(true);

        currentScreen = _startScreen;
        //GameManager.Instance.CurrentEventSystem.SetSelectedGameObject(screenDatas[currentScreen].EnterButton.gameObject);
        EventSystem.current.SetSelectedGameObject(screenDatas[currentScreen].EnterButton.gameObject);


        _canvas.gameObject.SetActive(false);
        GameManager.GameLogic.onGameResume += ClosePauseMenu;
        GameManager.GameLogic.onGamePause += ShowPauseMenu;
    }

    private void OnEnable()
    {
        GameManager.GameLogic.onGameResume += ClosePauseMenu;
        GameManager.GameLogic.onGamePause += ShowPauseMenu;
    }

    private void OnDisable()
    {
        GameManager.GameLogic.onGameResume -= ClosePauseMenu;
        GameManager.GameLogic.onGamePause -= ShowPauseMenu;
    }

    private void OnDestroy()
    {
		GameManager.GameLogic.onGameResume -= ClosePauseMenu;
		GameManager.GameLogic.onGamePause -= ShowPauseMenu;
	}

#if UNITY_EDITOR

    protected override void Validation()
    {
        base.Validation();

        //screenDatas = GetComponentsInChildren<MenuScreenContent>();

        //// Filter to only MenuScreenContent_Pause and sort by PauseMenuScreenTypes order
        //var sorted = screenDatas
        //    .OfType<MenuScreenContent_Pause>()
        //    .OrderBy(s => s.MainScreenTypes)
        //    .Cast<MenuScreenContent>()
        //    .ToArray();

        //screenDatas = sorted;

        //if (!_audioSource) _audioSource = GetComponent<AudioSource>();
    }
#endif
    public void Resume()
    {
        GameManager.GameLogic.SetPauseState(false);
        ClosePauseMenu();
    }

    public void QuitToMenu()
    {
        GameManager.SceneManager.LoadScene(MainSceneManager.GameScenes.MainMenu);
        
        print("Quit to Menu...");
    }

    void ShowPauseMenu()
    {
        _canvas.gameObject.SetActive(true);
        
    }

    void ClosePauseMenu()
    {
        _canvas.gameObject.SetActive(false);
    }

    protected override void ToggleScreen(int openingScreen, int closingScreen)
    {
        base.ToggleScreen(openingScreen, closingScreen);
    }
}
