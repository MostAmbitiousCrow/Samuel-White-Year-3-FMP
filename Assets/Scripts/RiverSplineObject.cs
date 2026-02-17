using UnityEngine;

public class RiverSplineObject : MonoBehaviour
{
    [SerializeField, Range(0, 2)] private int lane = 0;
    [SerializeField] private float speedMultiplier = 1f;

    [SerializeField] private float distanceOnSpline = 0f;
    public float DistanceOnSpline => distanceOnSpline;
    [SerializeField] private float distanceTravelled = 0f;
    public float DistanceTravelled => distanceTravelled;

    private void Update()
    {
        if (River_Manager.Instance.IsPaused) return;

        float speed = River_Manager.Instance.currentRiverSpeed * speedMultiplier;

        // move forward
        distanceTravelled += speed * Time.deltaTime;
        distanceOnSpline +=  speed * Time.deltaTime;
        distanceOnSpline %= River_Manager.SplineTotalLength; // Modulo to loop forever!

        River_Manager.Instance.AssignToCurveSection(distanceTravelled, lane, out Vector3 pos, out Quaternion rot);
        transform.SetPositionAndRotation(pos, rot);
    }
}