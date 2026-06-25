using System;
using Game;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.XInput;
using UnityEngine.UI;

public class GraphicUpdater : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private Image graphicRenderer;
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private ControllerGraphics graphics;
    [SerializeField] private ControllerGraphics alternativeGraphics;

    [Serializable]
    public class ControllerGraphics
    {
        public string instruction;
        public Sprite[] psGraphics;
        public Sprite[] xboxGraphics;
        public Sprite[] gamepadGraphics;
        public Sprite[] keyboardGraphics;
    }
    [SerializeField] private int _currentSpriteIndex = 0;

    private void Awake()
    {
        promptText?.SetText(graphics.instruction);
        UpdateGraphic();
    }

    private void OnEnable()
    {
        GameSettingsManager.GameplayChanged += UpdateGraphic;
    }
    private void OnDisable()
    {
        GameSettingsManager.GameplayChanged -= UpdateGraphic;
    }

    public void UpdateGraphic()
    {
        // Note: Graphic update is triggered by the InputGraphicController
        var pad = Gamepad.current;
        var alternativeControls = GameSettingsManager.UseAlternativeControlScheme;

        var text = alternativeControls ? alternativeGraphics.instruction : graphics.instruction;
        promptText?.gameObject.SetActive(text.Length != 0);
        promptText?.SetText(alternativeControls? alternativeGraphics.instruction : graphics.instruction);
        
        graphicRenderer.sprite = pad switch
        {
            // Keyboard
            null => alternativeControls? //TODO: this keeps throwing errors despite there being literally no issue with it
                alternativeGraphics.keyboardGraphics[_currentSpriteIndex]
                : 
                graphics.keyboardGraphics[_currentSpriteIndex],
            // Xbox
            XInputController => alternativeControls?
                alternativeGraphics.xboxGraphics[_currentSpriteIndex] 
                : 
                graphics.xboxGraphics[_currentSpriteIndex],
            // Playstation
            DualShockGamepad => alternativeControls?
                alternativeGraphics.psGraphics[_currentSpriteIndex] 
                : 
                graphics.psGraphics[_currentSpriteIndex],
            // Gamepad
            not null =>alternativeControls?
                alternativeGraphics.gamepadGraphics[_currentSpriteIndex] 
                : 
                graphics.gamepadGraphics[_currentSpriteIndex],
        };
        
        _currentSpriteIndex++;
        if (_currentSpriteIndex >= 2) _currentSpriteIndex = 0;
    }

}
