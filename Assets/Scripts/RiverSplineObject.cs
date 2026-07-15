using System;
using EditorAttributes;
using UnityEngine;

public class RiverSplineObject : MonoBehaviour
{
    [SerializeField, Range(0, 2)] private int lane = 1;
    public float speedMultiplier = 1f;
    [Tooltip("Optional offset of this object on the river spline")]
    [SerializeField] private float offset;

    [SerializeField, ReadOnly] private float distanceOnSpline = 0f;
    /// <summary> The current distance this object has travelled on the current spline curve section </summary>
    public float DistanceOnSpline => distanceOnSpline;
    [SerializeField, ReadOnly] private float totalDistanceTravelled = 0f;
    /// <summary>
    /// The total distance this object has travelled
    /// </summary>
    public float TotalDistanceTravelled => totalDistanceTravelled;
    [SerializeField, ReadOnly] private float globalDistanceTravelled = 0f;
    /// <summary> The overall distance this object has travelled during the playthrough </summary>
    public float GlobalDistanceTravelled => globalDistanceTravelled;
    public bool ignorePause;
    public bool ignoreRiverSpeed;

    private void OnEnable()
    {
        GameLevelManager.OnLevelLoaded += Reset;
    }

    private void OnDisable()
    {
        GameLevelManager.OnLevelLoaded -= Reset;
    }

    private void Update()
    {
        if (River_Manager.Instance.IsPaused && !ignorePause) return;

        float speed = (ignoreRiverSpeed ? 
            River_Manager.Instance.StoredRiverSpeed : River_Manager.Instance.currentRiverSpeed) * speedMultiplier;

        // move forward
        totalDistanceTravelled += speed * Time.deltaTime;
        distanceOnSpline += speed * Time.deltaTime;
        globalDistanceTravelled += speed * Time.deltaTime;
        distanceOnSpline %= River_Manager.SplineTotalLength; // Modulo to loop forever!
        
        // Debug.Log($"{name} Update Frame:{Time.frameCount} Dist:{distanceOnSpline} Speed:{River_Manager.Instance.currentRiverSpeed}");

        River_Manager.Instance.AssignToCurveSection(totalDistanceTravelled + offset, lane,
            out Vector3 pos, out Quaternion rot);
        transform.SetPositionAndRotation(pos, rot);
    }

    public void StopMoving()
    {
        speedMultiplier = 0f;
    }

    public void StartMoving()
    {
        speedMultiplier = 1f;
    }

    public void Reset()
    {
        speedMultiplier = 1f;
        totalDistanceTravelled = 0f;
        distanceOnSpline = 0f;
        
        // Debug.Log($"{name} Reset Frame:{Time.frameCount} Dist:{distanceOnSpline} Speed:{River_Manager.Instance.currentRiverSpeed}");
        
        River_Manager.Instance.AssignToCurveSection(totalDistanceTravelled, lane, out Vector3 pos, out Quaternion rot);
        transform.SetPositionAndRotation(pos, rot);
    }
}