using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.InputSystem.XInput;
using UnityEngine.UI;

public class GraphicUpdater : MonoBehaviour
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

    /*private void OnEnable()
    {
        InputSystem.onDeviceChange += UpdateGraphic;
    }
    
    private void OnDisable()
    {
        InputSystem.onDeviceChange += UpdateGraphic;
    }

    // Note: Graphic update is triggered by the InputGraphicController
    public void UpdateGraphic(InputDevice device, InputDeviceChange change)
    {
        graphicRenderer.sprite = device switch
        {
            DualShockGamepad => graphics.psGraphics[_currentSpriteIndex],
            Gamepad => graphics.xboxGraphics[_currentSpriteIndex],
            Joystick => graphics.gamepadGraphics[_currentSpriteIndex],
            Keyboard => graphics.keyboardGraphics[_currentSpriteIndex],
        };
        _currentSpriteIndex++;
        if (_currentSpriteIndex >= graphics.psGraphics.Length) _currentSpriteIndex = 0;
    }*/
    
    private void Awake() => UpdateGraphic();
    public void UpdateGraphic()
    {
        // Note: Graphic update is triggered by the InputGraphicController
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
    
    // No worky... :(
    /*private InputDevice _lastDevice;
    
    private void Awake() => InputSystem.onAnyButtonPress.CallOnce(OnAnyInput);

    private void OnEnable()
    {
        InputSystem.onDeviceChange += UpdateGraphic;
    }

    private void OnDisable()
    {
        InputSystem.onDeviceChange -= UpdateGraphic;
    }
    
    private void OnAnyInput(InputControl control)
    {
        _lastDevice = control.device;
        UpdateGraphic();
    }

    private void UpdateGraphic(InputDevice device, InputDeviceChange change)
    {
        _lastDevice = device.device;
        UpdateGraphic();
    }
    
    public void UpdateGraphic()
    {
        graphicRenderer.sprite = _lastDevice switch
        {
            Keyboard or Mouse => graphics.keyboardGraphics[_currentSpriteIndex],
            XInputController => graphics.xboxGraphics[_currentSpriteIndex],
            DualShockGamepad => graphics.psGraphics[_currentSpriteIndex],
            Gamepad => graphics.gamepadGraphics[_currentSpriteIndex],
            _ => graphics.keyboardGraphics[_currentSpriteIndex]
        };

        _currentSpriteIndex++;
        if (_currentSpriteIndex >= graphics.psGraphics.Length)
            _currentSpriteIndex = 0;
    }*/

}
