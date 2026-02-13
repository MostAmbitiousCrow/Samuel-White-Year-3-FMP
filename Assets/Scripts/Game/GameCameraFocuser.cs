using EditorAttributes;
using Game;
using GameCharacters;
using UnityEngine;

public class GameCameraFocuser : MonoBehaviour
{
    [SerializeField] private Camera camera;
    
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

    
    private void Awake()
    {
        if (playerTransform == null)
        {
            var p = FindFirstObjectByType<PlayerCharacter>();
            if (p) playerTransform = p.transform;
        }
        if (boatTransform == null)
        {
            var b = FindFirstObjectByType<Boat_Controller>();
            boatTransform = b.transform;
        }
        if (camera == null) camera = Camera.main;
        
        // Store origin position
        originPosition = transform.localPosition;
    }

    private void OnEnable()
    {
        River_Manager.Instance.OnRiverSpeedUpdate += UpdateFOV;
    }

    private void OnDisable()
    {
        River_Manager.Instance.OnRiverSpeedUpdate -= UpdateFOV;
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
    
            transform.localPosition = originPosition + new Vector3(_currentXOffset, 0f, 0f);
    
            if (lookAtTarget)
                transform.LookAt(lookAtTarget, transform.up);
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
        private void SlamCamera()
        {
         
        }
    #endregion

    #region Camera FOV Sliding

    private float _slideValue;
    private float _currentSlideVelocity;
    private float _targetFOV;

    private void FOVSlideCamera()
    {
        // Smooth towards FOV
        _slideValue = Mathf.SmoothDamp(
            _slideValue,
            _targetFOV,
            ref _currentSlideVelocity,
            smoothTime
        );

        camera.fieldOfView = _slideValue;
    }

    private void UpdateFOV()
    {
        float speed = River_Manager.Instance.currentRiverSpeed;
        Vector2Int minMax = River_Manager.Instance.minMaxSpeed;
        float t = Mathf.InverseLerp(minMax.x, minMax.y, speed); //TODO: Obtain the minimum and maximum speed of the river!
        
        _targetFOV = Mathf.Lerp(fovSlideRange.x, fovSlideRange.y, t);
    }

    #endregion


}
