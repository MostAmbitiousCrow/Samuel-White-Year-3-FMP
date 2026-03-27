using System;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class FPSDisplayCounter : MonoBehaviour
{
    private TextMeshProUGUI _textMeshProUGUI;
    private void Start()
    {
        _textMeshProUGUI = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        _textMeshProUGUI.SetText($"FPS: { (int)(1f / Time.unscaledDeltaTime)}");
    }
}
