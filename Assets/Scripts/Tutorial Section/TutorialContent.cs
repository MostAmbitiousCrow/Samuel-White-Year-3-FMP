using TMPro;
using UnityEngine;

public class TutorialContent : MonoBehaviour
{
    [SerializeField] private int actionRequirement = 3;
    private int _actionsPerformed;
    public CanvasGroup CanvasGroup { get; private set; }
    private Animation _checkAnimation;
    [SerializeField] private TextMeshProUGUI checkCounterText;
    public GraphicUpdater[] TutorialGraphicUpdaters { get; private set; }

    private void Awake()
    {
        CanvasGroup = GetComponent<CanvasGroup>();
        _checkAnimation = GetComponentInChildren<Animation>();
        TutorialGraphicUpdaters = GetComponentsInChildren<GraphicUpdater>();
        
        _checkAnimation.gameObject.SetActive(false);
        
        checkCounterText.text = $"{_actionsPerformed} / {actionRequirement}";
    }

    #region Canvas Group
    /// <summary> Reveals this Tutorial Contents Canvas alpha and returns whether it has reached one </summary>
    public bool RevealGroupContent(float multiplier = 1f)
    {
        CanvasGroup.alpha += Time.deltaTime * multiplier; // Increase alpha amount

        // Check if the alpha group has been fully revealed
        return CanvasGroup.alpha < 1f;
    }
    
    /// <summary> Fades this Tutorial Contents Canvas alpha and returns whether it has reached zero </summary>
    public bool FadeGroupContent(float multiplier = 1f)
    {
        CanvasGroup.alpha -= Time.deltaTime * multiplier; // Decrease alpha amount

        // Check if the alpha group has disappeared
        return CanvasGroup.alpha > 0f;
    }
    #endregion

    /// <summary> Updates all of this tutorial contents visual input graphics </summary>
    public void UpdateInputGraphics()
    {
        foreach (var graphic in TutorialGraphicUpdaters) graphic.UpdateGraphic();
    }
    
    /// <summary> Updates the tutorial content check visual state.
    /// Returns true or false if the count condition has been met </summary>
    public bool UpdateContentCheckState()
    {
        _actionsPerformed++;
        // If the amount of actions has surpassed the requirement, enable the checker.
        if (_actionsPerformed >= actionRequirement)
        {
            checkCounterText.text = $"{ProgressLimit()} / {actionRequirement}";
            _checkAnimation.gameObject.SetActive(true);
            
            // Play the check animation once the actions performed have met the requirement
            _checkAnimation.Stop();
            _checkAnimation.Play();
            
            return true;
        }
        // Otherwise, update the check counter text
        checkCounterText.text = $"{ProgressLimit()} / {actionRequirement}";
        return false;
    }

    private int ProgressLimit()
    {
        return _actionsPerformed >= actionRequirement ? actionRequirement : _actionsPerformed;
    }
}
