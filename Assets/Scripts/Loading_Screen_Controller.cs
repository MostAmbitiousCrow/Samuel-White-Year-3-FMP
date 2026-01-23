using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Loading_Screen_Controller : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] Canvas _canvas;
    [SerializeField] CanvasScaler _canvasScaler;
    [Space]
    [SerializeField] CanvasGroup _loadingMeterGroup;
    [SerializeField] TextMeshProUGUI _loadingText;
    [SerializeField] Slider _loadingMeter;
    [Space]
    [SerializeField] Animator _transitionAnimator;
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

        // Update Animator Speed and trigger the Enter Animation
        _transitionAnimator.speed = 1f / startTransitionTime;
        _transitionAnimator.SetTrigger("Enter");

        _loadingMeterGroup.alpha = 0f;

        while (t > 0f)
        {
            // Update Loading Meter Aplha
            _loadingMeterGroup.alpha = Mathf.Lerp(1f, 0f, t);
            yield return t -= Time.unscaledDeltaTime;
        }

        // Reset Animator Speed and trigger Loading Animation
        _transitionAnimator.speed = 1f;
        _transitionAnimator.SetTrigger("Loading");

        UpdateLoadingMeter(0f);
        _loadingMeterGroup.alpha = 1f;

        IsTransitioning = false;
        yield break;
    }

    IEnumerator CloseLoadingScreenProcess()
    {
        // while (!_canvas.worldCamera)
        // {
        //     _canvas.worldCamera = Camera.main;
        //     yield return new WaitForEndOfFrame();
        // }
        
        yield return new WaitUntil(() => _canvas.worldCamera = Camera.main);
        Debug.Log($"Render Camera was set to: {_canvas.worldCamera}");
        
        float t = 0f;

        // Update Animator Speed and trigger the Close Animation
        _transitionAnimator.speed = 1f / endTransitionTime;
        _transitionAnimator.SetTrigger("Close");

        while (t < endTransitionTime)
        {
            // Update Loading Meter Aplha
            Debug.Log($"Closing. Time: {t}. Duration: {endTransitionTime}");
            _loadingMeterGroup.alpha = Mathf.Lerp(1f, 0f, t);
            yield return t += Time.unscaledDeltaTime;
        }

        // Reset speed and bring to inactive
        _transitionAnimator.speed = 1f;
        _transitionAnimator.SetTrigger("Inactive");
        
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

    //void UpdateFadeTransition(float prog, float timeRef)
    //{
    //    Color pCol = Color.Lerp(Color.black, Color.clear, Mathf.InverseLerp(0, timeRef, prog));
    //    _loadingFadeTransition.color = pCol;
    //}
}
