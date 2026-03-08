using UnityEngine;

public class DemoMenuManager : MonoBehaviour
{
    public void ReturnToMainMenu()
    {
        // Load to Main Menu Scene
        GameManager.SceneManager.LoadScene(MainSceneManager.GameScenes.MainMenu);
    }
}
