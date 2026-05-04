using Game;
using GameCharacters;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class CharacterGroundTargetLaser : MonoBehaviour
{
    [SerializeField] private BoatCharacter boatCharacter;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private bool targetBoatSpace;

    private void Awake()
    {
        if (!boatCharacter) boatCharacter = GetComponentInParent<BoatCharacter>();
        if (!lineRenderer) lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.useWorldSpace = true;
        lineRenderer.positionCount = 2;
    }

    private void Update()
    {
        // Only allow target laser if Depth Assist is enabled
        if (!GameSettingsManager.DoDepthAssist) { lineRenderer.enabled = false; return; }
        
        if (boatCharacter.IsGrounded || !boatCharacter.CurrentSpace.t) { lineRenderer.enabled = false; return; }
        
        // Stretch the line under the characters stomp position to the surface of their current space on the boat
        lineRenderer.enabled = true;
        var stompPosition = boatCharacter.StompPosition.position;
        lineRenderer.SetPosition(0, stompPosition);

        var boatSpace = boatCharacter.CurrentSpace.t.position;
        var space = targetBoatSpace? boatSpace : new Vector3(stompPosition.x, boatSpace.y, stompPosition.z);
        lineRenderer.SetPosition(1, space);
    }
}
