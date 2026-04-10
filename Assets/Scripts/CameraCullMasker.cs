using System;
using GameCharacters;
using UnityEngine;

public class CameraCullMasker : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    private int _originalMask;

    private void Awake()
    {
        if (!targetCamera)
            targetCamera = Camera.main;

        if (targetCamera) _originalMask = targetCamera.cullingMask;

        _showLayersEvent = () => ShowOnlyLayer("Player");
    }

    private PlayerCharacter.PlayerDied _showLayersEvent;

    private void OnEnable() => PlayerCharacter.OnPlayerDied += _showLayersEvent;
    private void OnDisable() => PlayerCharacter.OnPlayerDied -= _showLayersEvent;

    public void ShowOnlyLayer(string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);
        targetCamera.cullingMask = 1 << layer;
        Debug.Log("Camera Mask set to " + layerName);
    }

    public void RestoreCamera()
    {
        targetCamera.cullingMask = _originalMask;
        Debug.Log("Camera Masks Restored");
    }
}