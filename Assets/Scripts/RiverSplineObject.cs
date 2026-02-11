using UnityEngine;
using EditorAttributes;
using UnityEngine.Splines;

/// <summary>
/// The base script for all objects that are following the Global River Spline
/// </summary>
[RequireComponent(typeof(SplineAnimate))]
public class RiverSplineObject : MonoBehaviour
{
    [Line(GUIColor.White)]
    [Header("Dependencies")]
    [SerializeField] private River_Manager riverManager;
    [SerializeField] private SplineAnimate splineAnimate;

    private void Awake()
    {
        if (riverManager == null) riverManager = FindFirstObjectByType<River_Manager>();
        if (splineAnimate != null) return;
        if (gameObject.TryGetComponent<SplineAnimate>(out var animate))
            splineAnimate = animate;
        else Debug.LogError($"{gameObject.name} requires a SplineAnimate component");
    }

    private void OnEnable()
    {
        riverManager.OnRiverSpeedUpdate += UpdateSpeed;
    }

    private void OnDisable()
    {
        riverManager.OnRiverSpeedUpdate -= UpdateSpeed;
    }

    private void UpdateSpeed()
    {
        splineAnimate.MaxSpeed = riverManager.CurrentRiverSpeed;
    }
}
