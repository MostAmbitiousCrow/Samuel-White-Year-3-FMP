using Game;
using TMPro;
using UnityEngine;
using GameColours;

public class TextUpdater : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textMesh;
    
    private void Awake()
    {
        textMesh = GetComponentInChildren<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        GameSettingsManager.GameplayChanged += ActivateFont;
        ActivateFont();
    }

    private void OnDisable()
    {
        GameSettingsManager.GameplayChanged -= ActivateFont;
    }

    private void ActivateFont()
    {
        if (!textMesh || !GameSettingsManager.Instance) return;
        textMesh.font = GameSettingsManager.DoDyslexiaFont? 
            GameSettingsManager.Instance.dyslexicFont : GameSettingsManager.Instance.pixelFont;

        // Set text colour as the global highlight colour
        textMesh.color = GameColoursManager.CurrentColours.MaterialColours[0].HighlightColour;
    }
}
