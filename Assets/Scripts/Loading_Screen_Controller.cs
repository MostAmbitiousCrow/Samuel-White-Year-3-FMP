using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Loading_Screen_Controller : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] Canvas _canvas;
    [SerializeField] CanvasScaler _canvasScaler;
    [SerializeField] TextMeshProUGUI _loadingText;

    [Header("Stats")]
    [SerializeField] float startTransitionTime, endTransitionTime = 1.5f;
    public bool IsTransitioning { get; private set; }

    private void OnEnable()
    {
        GameManager.Instance.UserSettings.onSettingsUpdated += OnSettingsUpdated;
    }
    private void OnDisable()
    {
        GameManager.Instance.UserSettings.onSettingsUpdated -= OnSettingsUpdated;
    }

    private void OnSettingsUpdated(GameManager.GameUserSettings.GameSettings gameSettings)
    {
        _canvasScaler.referenceResolution = gameSettings.TargetAspectResolution.resolution;
    }

    #region 
    public void StartLoadingScreen()
    {
        StartCoroutine(EnterLoadingScreenProcess());
    }

    public void EndLoadingScreen()
    {
        StartCoroutine(CloseLoadingScreenProcess());
    }
    #endregion

    #region 
    IEnumerator LoadingScreenProcess()
    {

        yield break;
    }

    IEnumerator EnterLoadingScreenProcess()
    {

        yield break;
    }

    IEnumerator CloseLoadingScreenProcess()
    {

        yield break;
    }
    #endregion
}
