using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.XInput;
using UnityEngine.UI;

public class TutorialGraphicUpdater : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private Image graphicRenderer;
    [SerializeField] private ControllerGraphics graphics = new();

    [Serializable]
    public class ControllerGraphics
    {
        public Sprite[] psGraphics;
        public Sprite[] xboxGraphics;
        public Sprite[] gamepadGraphics;
        public Sprite[] keyboardGraphics;
    }

    private int _currentSpriteIndex = 0;
    public void UpdateGraphic()
    {
        var pad = Gamepad.current;
        graphicRenderer.sprite = pad switch
        {
            null => graphics.keyboardGraphics[_currentSpriteIndex],
            XInputController => graphics.xboxGraphics[_currentSpriteIndex],
            DualShockGamepad => graphics.psGraphics[_currentSpriteIndex], 
            not null => graphics.gamepadGraphics[_currentSpriteIndex]
        };
        _currentSpriteIndex++;
        if (_currentSpriteIndex >= graphics.psGraphics.Length) _currentSpriteIndex = 0;
    }
}
