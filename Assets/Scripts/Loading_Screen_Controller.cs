using System.Collections;
using CarterGames.Assets.AudioManager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Loading_Screen_Controller : MonoBehaviour
{
    private static readonly int Close = Animator.StringToHash("Close");
    private static readonly int Inactive = Animator.StringToHash("Inactive");
    private static readonly int Enter = Animator.StringToHash("Enter");
    private static readonly int Loading = Animator.StringToHash("Loading");

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
    public static bool IsOpening { get; private set; }
    public static bool IsProcessing { get; private set; }
    public static bool IsClosing { get; private set; }

    public static bool IsTransitioning => IsOpening || IsClosing || IsProcessing;

    private void Start()
    {
        _transitionScreen.SetActive(false);
    }

    #region Loading
    public void StartLoadingScreen()
    {
        if (IsTransitioning) return;
        IsOpening = true;
        _transitionScreen.SetActive(true);
        UpdateLoadingMeter(0f);
        StartCoroutine(EnterLoadingScreenProcess());
    }

    public void EndLoadingScreen()
    {
        IsClosing = true;
        IsProcessing = false;
        StartCoroutine(CloseLoadingScreenProcess());
    }
    #endregion

    #region Loading Routine

    private IEnumerator EnterLoadingScreenProcess()
    {
        GameManager.GameLogic.CanPauseGame = false;
        
        float t = startTransitionTime;

        // Update Animator Speed and trigger the Enter Animation
        _transitionAnimator.speed = 1f / startTransitionTime;
        _transitionAnimator.SetTrigger(Enter);

        _loadingMeterGroup.alpha = 0f;
        
        // Play Enter SFX
        AudioManager.Play(Clip.Flushed);

        while (t > 0f)
        {
            // Update Loading Meter Alpha
            _loadingMeterGroup.alpha = Mathf.Lerp(1f, 0f, t);
            yield return t -= Time.unscaledDeltaTime;
        }

        // Reset Animator Speed and trigger Loading Animation
        _transitionAnimator.speed = 1f;
        _transitionAnimator.SetTrigger(Loading);

        UpdateLoadingMeter(0f);
        _loadingMeterGroup.alpha = 1f;

        var waitForFrame = new WaitForEndOfFrame();
        for (int i = 0; i < 8; i++) yield return waitForFrame; // Kinda prevents lag, but isn't the best atm
        
        IsOpening = false;
        IsProcessing = true;
    }

    private IEnumerator CloseLoadingScreenProcess()
    {
        
        float t = 0f;

        // Update Animator Speed and trigger the Close Animation
        _transitionAnimator.speed = 1f / endTransitionTime;
        _transitionAnimator.SetTrigger(Close);

        // Play Exit SFX
        AudioManager.Play(Clip.Forward);

        while (t < endTransitionTime)
        {
            // Update Loading Meter Alpha
            // Debug.Log($"Closing. Time: {t}. Duration: {endTransitionTime}");
            _loadingMeterGroup.alpha = Mathf.Lerp(1f, 0f, t);
            yield return t += Time.unscaledDeltaTime;
        }

        // Reset speed and bring to inactive
        _transitionAnimator.speed = 1f;
        _transitionAnimator.SetTrigger(Inactive);
        
        _transitionScreen.SetActive(false);

        GameManager.GameLogic.CanPauseGame = true;
        IsClosing = false;
    }
    #endregion

    public void UpdateLoadingMeter(float amount)
    {
        return; // TODO: Disabling until Canvas Material problem is fixed... 
        _loadingMeter.value = amount;
        amount = Mathf.Round(amount * 100f);
        _loadingText.SetText($"{amount}%");
    }
}
