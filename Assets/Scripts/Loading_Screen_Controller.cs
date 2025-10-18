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
    [SerializeField] Slider _loadingMeter;
    [SerializeField] Image _loadingFadeTransition;
    [SerializeField] GameObject _transitionScreen;

    [Header("Stats")]
    [SerializeField] float startTransitionTime = .5f;
    [SerializeField] float endTransitionTime = 1.5f;
    public bool IsTransitioning { get; private set; }

    private void Start()
    {
        _transitionScreen.SetActive(false);
    }

    //private void OnEnable() //TODO Triggers before the Gamemanager instance exists, try and fix this
    //{
    //    GameManager.Instance.UserSettings.onSettingsUpdated += OnSettingsUpdated;
    //}
    //private void OnDisable()
    //{
    //    GameManager.Instance.UserSettings.onSettingsUpdated -= OnSettingsUpdated;
    //}

    //private void OnSettingsUpdated(GameManager.GameUserSettings.GameSettings gameSettings)
    //{
    //    //_canvasScaler.referenceResolution = gameSettings.TargetAspectResolution.resolution; // Not necessary
    //}

    #region 
    public void StartLoadingScreen()
    {
        IsTransitioning = true;
        _transitionScreen.SetActive(true);
        UpdateLoadingMeter(0f);
        StartCoroutine(EnterLoadingScreenProcess());
    }

    public void EndLoadingScreen()
    {
        StartCoroutine(CloseLoadingScreenProcess());
    }
    #endregion

    #region 
    IEnumerator EnterLoadingScreenProcess()
    {
        float t = startTransitionTime;

        while (t > 0f)
        {
            UpdateFadeTransition(t, startTransitionTime);
            yield return t -= Time.deltaTime;
        }
        UpdateLoadingMeter(0f);
        IsTransitioning = false;
        yield break;
    }

    IEnumerator CloseLoadingScreenProcess()
    {
        float t = 0f;

        while (t < endTransitionTime)
        {
            UpdateFadeTransition(t, endTransitionTime);
            yield return t += Time.deltaTime;
        }

        IsTransitioning = false;
        _transitionScreen.SetActive(false);

        yield break;
    }
    #endregion

    public void UpdateLoadingMeter(float amount)
    {
        _loadingMeter.value = amount;
        amount = Mathf.Round(amount * 100f);
        _loadingText.SetText($"{amount}%");
    }

    void UpdateFadeTransition(float prog, float timeRef)
    {
        Color pCol = Color.Lerp(Color.black, Color.clear, Mathf.InverseLerp(0, timeRef, prog));
        _loadingFadeTransition.color = pCol;
    }
}
