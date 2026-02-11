using EditorAttributes;
using GameCharacters;
using UnityEngine;

public class GameCameraFocuser : MonoBehaviour
{
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
        
        // Store origin position
        originPosition = transform.localPosition;
    }

    private void LateUpdate()
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
}
