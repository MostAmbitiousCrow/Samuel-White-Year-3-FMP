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
    
    private void Awake()
    {
        if (playerTransform != null)
        {
            var p = FindFirstObjectByType<PlayerCharacter>();
            playerTransform = p.transform;
        }

        if (boatTransform != null)
        {
            var b = FindFirstObjectByType<Boat_Controller>();
            boatTransform = b.transform;
        }
        
        // Store origin position
        originPosition = transform.position;
    }

    private void LateUpdate()
    {
        var boatLean = LeanToTarget(boatTransform, boatFollowStrength);
        var playerLean = LeanToTarget(playerTransform, playerFollowStrength);
        transform.position = originPosition + boatLean + playerLean;
        
        if (lookAtTarget) transform.LookAt(lookAtTarget);
    }

    private Vector3 LeanToTarget(Transform target, float strength)
    {
        var leanVector = Vector3.Lerp(Vector3.zero, target.position, strength); // TODO: Future proof for tunnel switches
        return leanVector;
    }

    /// <summary>
    /// forces the camera position to a given side 
    /// </summary>
    private void SlamCamera()
    {
        
    }
}
