using Game;
using TMPro;
using UnityEngine;
using GameColours;

public class TextUpdater : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textMesh;
    
    private void Start()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
        ActivateFont();
    }

    private void OnEnable()
    {
        GameSettingsManager.GameplayChanged += ActivateFont;
        GameColoursManager.OnGameColoursChanged += ActivateFont;
        ActivateFont();
    }

    private void OnDisable()
    {
        GameSettingsManager.GameplayChanged -= ActivateFont;
        GameColoursManager.OnGameColoursChanged -= ActivateFont;
    }

    private void ActivateFont()
    {
        if (!textMesh) textMesh = GetComponent<TextMeshProUGUI>();
        if (!GameSettingsManager.Instance) return;
        
        textMesh.font = 
            GameSettingsManager.DoDyslexiaFont? 
            GameSettingsManager.Instance.dyslexicFont : GameSettingsManager.Instance.pixelFont;

        // Set text colour as the UI highlight colour
        if (!GameColoursManager.CurrentColours) return;
        textMesh.color = GameColoursManager.MaterialTypes[7].materials[0].GetColor(GameColoursManager.NewHighlight);
    }
}