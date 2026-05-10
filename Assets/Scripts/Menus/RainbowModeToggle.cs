using Game;
using GameColours;
using UnityEngine;
using UnityEngine.UI;

public class RainbowModeToggle : MonoBehaviour
{
    [SerializeField] private Toggle targetElement;

    private void Reset()
    {
        targetElement = GetComponentInChildren<Toggle>();
    }

    private void Start()
    {
        if (targetElement == null)
        {
            targetElement = GetComponentInChildren<Toggle>();
            if (!targetElement)
            {
                Debug.Log("[RainbowMode] Could not find any Toggle component on the GameObject.", gameObject);
                return;
            }
        }

        targetElement.isOn = GameSettingsManager.DoRainbowMode;
        targetElement.onValueChanged.AddListener(OnValueChange);
    }

    private void OnValueChange(bool isOn)
    {
        GameSettingsManager.DoRainbowMode = isOn;
    }
}

