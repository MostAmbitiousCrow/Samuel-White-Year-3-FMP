using Game;
using GameColours;
using TMPro;
using UnityEngine;

public class ColourPaletteController : MonoBehaviour
{
    [SerializeField] private SO_GameColours[] gameColours;
    [SerializeField] private int currentColour;
    [Space]
    [SerializeField] private TextMeshProUGUI text;

    private void Start()
    {
        text.SetText(GameColoursManager.CurrentColours.name);
    }

    public void LeftItem()
    {
        currentColour--;
        if (currentColour < 0) currentColour = gameColours.Length-1;
        
        UpdateColourManager();
    }

    public void RightItem()
    {
        currentColour++;
        currentColour %= gameColours.Length;
        
        UpdateColourManager();
    }

    private void UpdateColourManager()
    {
        text.SetText(gameColours[currentColour].name);
        
        // Override Colourblindness and assign game colour
        GameSettingsManager.SetColourBlindness(GameSettingsManager.ColourblindType.None);
        GameColoursManager.CurrentColours = gameColours[currentColour];
        GameColoursManager.UpdateColours();
    }
}
