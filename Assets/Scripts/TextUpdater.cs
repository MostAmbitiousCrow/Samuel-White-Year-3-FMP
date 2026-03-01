using Game;
using TMPro;
using UnityEngine;
using GameColours;

public class TextUpdater : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textMesh;
    
    private void Start()
    {
        textMesh = GetComponentInChildren<TextMeshProUGUI>();
        ActivateFont();
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
        if (!textMesh) textMesh = GetComponentInChildren<TextMeshProUGUI>();
        textMesh.font = GameSettingsManager.DoDyslexiaFont? 
            GameSettingsManager.Instance.dyslexicFont : GameSettingsManager.Instance.pixelFont;

        // Set text colour as the global highlight colour
        if (GameColoursManager.CurrentColours == null) return;
        textMesh.color = GameColoursManager.CurrentColours.MaterialColours[0].HighlightColour;
    }
}
