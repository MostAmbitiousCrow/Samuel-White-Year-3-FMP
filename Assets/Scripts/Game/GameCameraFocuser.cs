using EditorAttributes;
using Game;
using GameCharacters;
using UnityEngine;

public class GameCameraFocuser : MonoBehaviour
{
    private Camera _gameCamera;
    
    [Header("Targets")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform boatTransform;
    [SerializeField] private Transform lookAtTarget;
    [Header("Strength")]
    [SerializeField, Range(0f, 1f)] private float playerFollowStrength = .2f;
    [SerializeField, Range(0f, 1f)] private float boatFollowStrength = .6f;

    [SerializeField, ReadOnly] private Vector3 originPosition;
    
    [Header("Smoothing")]
    [SerializeField] private float smoothTime = 0.25f;

    private float _currentXVelocity;
    private float _currentXOffset;

    [Header("Camera FOV Sliding")]
    [SerializeField] private Vector2 fovSlideRange = new (60f, 120f);
    private bool _isDisabled;
    private float _storedSlideValue;

    
    private void Start()
    {
        if (!playerTransform)
        {
            var p = FindFirstObjectByType<PlayerCharacter>();
            if (p) playerTransform = p.transform;
        }
        if (!boatTransform)
        {
            var b = FindFirstObjectByType<Boat_Controller>();
            boatTransform = b.transform;
        }

        _slideValue = _targetFOV = fovSlideRange.x;

        _gameCamera = Camera.main;
        if (_gameCamera) _gameCamera.fieldOfView = _targetFOV;
        
        UpdateFOV();
        // Store origin position
        originPosition = transform.localPosition;
    }

    private void OnEnable()
    {
        River_Manager.Instance.OnRiverSpeedUpdate += UpdateFOV;
        GameSettingsManager.GameplayChanged += UpdateFOV;
    }

    private void OnDisable()
    {
        River_Manager.Instance.OnRiverSpeedUpdate -= UpdateFOV;
        GameSettingsManager.GameplayChanged -= UpdateFOV;
    }

    private void LateUpdate()
    {
        LeanToTargets();
        if (GameSettingsManager.DoFovSliding) FOVSlideCamera();
    }

    #region Camera Leaning
    private void LeanToTargets()
    {
        float boatLean = GetTargetLean(boatTransform, boatFollowStrength);
        float playerLean = GetTargetLean(playerTransform, playerFollowStrength);

        float targetX = boatLean + playerLean;

        // Smooth toward target
        _currentXOffset = Mathf.SmoothDamp(
            _currentXOffset,
            targetX,
            ref _currentXVelocity,
            smoothTime
        );

        transform.localPosition = originPosition + new Vector3(_currentXOffset, 0f, 0f); //TODO: Fix NaN error

        if (lookAtTarget) transform.LookAt(lookAtTarget, transform.up);
    }    
    
    private float GetTargetLean(Transform target, float strength)
    {
         if (!target) return 0f;

         Vector3 localTargetPos = transform.parent.InverseTransformPoint(target.position);
         return localTargetPos.x * strength;
    }

    /// <summary>
    /// forces the camera position to a given side 
    /// </summary>
    // private void SlamCamera()
    // {
    //  
    // }
    #endregion

    #region Camera FOV Sliding

    private float _slideValue;
    private float _currentSlideVelocity;
    private float _targetFOV;

    private void FOVSlideCamera()
    {
        // Smooth towards FOV
        _slideValue = Mathf.SmoothDamp(_slideValue, _targetFOV, ref _currentSlideVelocity, smoothTime);
        _gameCamera.fieldOfView = _slideValue;
    }

    private void UpdateFOV()
    {
        if (!GameSettingsManager.DoFovSliding)
        {
            _gameCamera.fieldOfView = fovSlideRange.x;
            _storedSlideValue = _slideValue;
        }
        else //if (!_isDisabled)
        {
            float speed = River_Manager.Instance.TargetRiverSpeed;
            Vector2 minMax = River_Manager.Instance.minMaxSpeed;
            float t = Mathf.InverseLerp(minMax.x, minMax.y, speed);
        
            _targetFOV = Mathf.Lerp(fovSlideRange.x, fovSlideRange.y, t);
        }
    }
    #endregion
}
