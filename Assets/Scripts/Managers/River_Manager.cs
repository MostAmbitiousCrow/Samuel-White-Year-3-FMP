using EditorAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using CarterGames.Assets.AudioManager;
using GameCharacters;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Splines;

public class River_Manager : MonoBehaviour
{
    #region Variables
    [Header("Components")]
    [SerializeField] private SplineContainer worldSplineContainer;
    public SplineContainer WorldSplineContainer => worldSplineContainer;
    [SerializeField] private Boat_Controller boatController;
    public Boat_Controller BoatController => boatController;
    [SerializeField] private TsunamiController tsunamiController;

    [Header("Audio")] 
    [SerializeField] private InspectorAudioClipPlayer speedIncreaseSound;
    [SerializeField] private InspectorAudioClipPlayer speedDecreaseSound;

    [Header("River Stats")]

    [Tooltip("The current speed of the rivers flow. Affects the rivers animation.")]
    public float riverFlowSpeed;
    
    [Tooltip("The min/max values of the levels of speed the river can reach")]
    [MinMaxSlider(0f, 50f)] public Vector2 minMaxSpeed = new(0f, 50f);
    /// <summary> The default speed of the river. Default value is: 1 </summary>
    public int startingRiverSpeed = 50;
    /// <summary> The speed of the river shared across river objects </summary>
    public float currentRiverSpeed = 5f;
    /// <summary> The minimum amount of distance river objects can spawn in the z axis </summary>
    public int riverObjectSpawnDistance = 45;
    /// <summary> Is the river currently paused? </summary>
    public bool IsPaused { get; private set; } = false;
    /// <summary> Is the river currently speeding up or slowing down? </summary>
    public bool IsTransitioning { get; private set; }

    [FormerlySerializedAs("_lanesParent")]
    [Header("River Lanes Info")]
    [SerializeField]
    private Transform lanesParent;

    [Serializable]
    public class RiverLane
    {
        public int id;
        public Transform transform;
    }
    public List<RiverLane> riverLanes;
    public List<IAffectedByRiver> RiverInfluencedObjects = new();

    /// <summary> Action that updates all subcribed events whenever the river speed is updated </summary>
    public event Action OnRiverSpeedUpdate;

    /// <summary> Instance of the River Manager </summary>
    public static River_Manager Instance { get; private set; }

    #endregion

    private void Awake()
    {
        Instance = this;
        
        UpdateSplineLengths();
        if (!boatController) boatController = FindFirstObjectByType<Boat_Controller>();
        if (!tsunamiController) tsunamiController = FindFirstObjectByType<TsunamiController>();
        UpdateSpaceDatas();
        ResetRiver();
    }

    #region Data Update Methods
    [Button]
    public void UpdateSpaceDatas()
    {
        riverLanes.Clear();

        for (int i = 0; i < lanesParent.childCount; i++)
        {
            RiverLane rl = new() { transform = lanesParent.GetChild(i).transform, id = i };
            riverLanes.Add(rl);
        }
        print($"Updated River Lanes to {riverLanes.Count} lanes");
    }
    #endregion
    

    private void Start()
    {
        OnRiverSpeedUpdate?.Invoke();
    }

    private void OnEnable()
    {
        SewerEnvironmentArtManager.OnEnvironmentUpdated += UpdateSplineLengths;
        PlayerCharacter.OnPlayerDied += _ => PauseRiver();
    }

    private void OnDisable()
    {
        SewerEnvironmentArtManager.OnEnvironmentUpdated -= UpdateSplineLengths;
        PlayerCharacter.OnPlayerDied -= _ => PauseRiver();
    }

    #region Lane and Space Checks

    /// <summary>
    /// Returns a true/false if a lane exists within the list of lanes
    /// </summary>
    public bool CheckAvailableLane(int lane)
    {
        return lane <= riverLanes.Count && lane >= 0;
    }

    /// <summary>
    /// Checks if there is a lane available based on a given direction, will otherwise return the initial provided lane, and returns Lane Data.
    /// </summary>
    public RiverLane GetLaneFromDirection(int currentLane, int direction)
    {
        int spaces;
        int targetLane;

        spaces = GetLanes().Count;
        targetLane = currentLane + direction;

        if (targetLane < spaces && targetLane > -1) return riverLanes[targetLane];
        else return riverLanes[currentLane];
    }

    /// <summary>
    /// Obtain the ID number of the opposite lane
    /// </summary>
    public int GetOppositeLaneID(int currentLane)
    {
        return currentLane == 0 ? 1 : 0;
    }

    /// <summary>
    /// Returns Lane Data based on a given lane ID
    /// </summary>
    public RiverLane GetLane(int lane)
    {
        return riverLanes[lane];
    }

    /// <summary>
    /// Returns the list containing all Lane Datas
    /// </summary>
    public List<RiverLane> GetLanes()
    {
        return riverLanes;
    }
    #endregion

    #region River Modification
    [Header("Animation")]
    [Tooltip("The curve representing the slow down transition")] // Don't know any other way to describe it :sob
    [SerializeField] private AnimationCurve slowCurve;

    [Tooltip("The curve representing the speed up transition")] // Don't know any other way to describe it :sob
    [SerializeField] private AnimationCurve speedCurve;

    private int _previousSpeed;
    public void SetRiverSpeed(int amount = 10, bool bypassCheck = true, bool playSound = true)
    {
        // Regress the Tsunami if the river has sped up
        if (amount > _previousSpeed) tsunamiController.Regress();
        
        if (!bypassCheck)
        {
           if (amount > minMaxSpeed.y)
           {
               print("River speed has reached maximum speed!");
               return;
           }
   
           if (amount < minMaxSpeed.x)
           {
               print("River speed has reached minimum speed!");
               return;
           }
        }

        targetRiverSpeed = amount;
        if (playSound)
        {
            if (amount > _previousSpeed) speedIncreaseSound.Play();
            else if (amount < _previousSpeed) speedDecreaseSound.Play();
        }
        _previousSpeed = amount;
    }

    private int _storedRiverSpeed;
    [Button]
    public void HaltRiver(int amount = 10)
    {
        _capturedTime = Time.time;
        // Skip storing value if halting whilst slowed
        if (!isHalted) _storedRiverSpeed = targetRiverSpeed;
        isHalted = true;
        SetRiverSpeed(targetRiverSpeed / 2, true, false);

        // Progress the Tsunami
        tsunamiController.Progress();
    }

    /// <summary> The method to slow down the global river speed </summary>
    /// <param name="amount"></param>
    [Button]
    public void SlowDownRiver(int amount = 10, bool bypassRange = false)
    {
        var targetSpeed = targetRiverSpeed - amount;
        
        if (targetSpeed < minMaxSpeed.x && !bypassRange) // If target speed is less than the min speed value
        {
            print("River speed has reached minimum speed");
            return;
        }
        targetRiverSpeed = targetSpeed;
        
        OnRiverSpeedUpdate?.Invoke();
        speedDecreaseSound.Play();
    }

    /// <summary> The method to speed up the global river speed </summary>
    [Button]
    public void SpeedUpRiver(int amount = 10)
    {
        var targetSpeed = targetRiverSpeed + amount;
        
        if (targetSpeed > minMaxSpeed.y)
        {
            print("River speed has reached maximum speed!");
            return;
        }

        targetRiverSpeed = targetSpeed;
        IsTransitioning = true;

        OnRiverSpeedUpdate?.Invoke();
        speedIncreaseSound.Play();
        
        // Regress the Tsunami
        tsunamiController.Regress();
    }

    /// <summary> Completely stops the speed of the river with optional smoothing </summary>
    public void PauseRiver(bool smoothing = false, float smoothAmount = 1f) //TODO
    {
        IsPaused = true;
        // TODO add optional smoothing towards stopping
    }

    /// <summary> Resumes the paused river with optional smoothing </summary>
    public void ResumeRiver(bool smoothing = false, float smoothAmount = 1f) //TODO
    {
        IsPaused = false;
        // TODO add optional smoothing towards stopping
    }

    /// <summary> Completely resets all changes made to the river to their default value and stops any speed transitions </summary>
    public void ResetRiver()
    {
        currentRiverSpeed = startingRiverSpeed;
        targetRiverSpeed = startingRiverSpeed;
    }

    // ==============================================================
    // New River Speed Controls
    // ==============================================================
    
    [SerializeField] private float slowTime = .5f;
    [SerializeField] private float speedRecoveryTime = 2.4f;
    [SerializeField, ReadOnly] private bool isHalted;
    public bool IsHalted => isHalted;
    private float _referenceVelocity;
    private float _capturedTime;
    [SerializeField] private int targetRiverSpeed;
    public int TargetRiverSpeed => targetRiverSpeed;
    [SerializeField] private int slowTargetRiverSpeed;
    
    private void Update()
    {
        if (IsPaused) return;
        
        // Slow down boat to target slow speed
        if (isHalted)
        {
            riverFlowSpeed = currentRiverSpeed = Mathf.SmoothDamp
                (currentRiverSpeed, targetRiverSpeed, ref _referenceVelocity, slowTime / 10f);
            
            if (Time.time > _capturedTime + speedRecoveryTime)
            {
                isHalted = false;
                targetRiverSpeed = _storedRiverSpeed;
                OnRiverSpeedUpdate?.Invoke();
            }
        }
        else
        {
            riverFlowSpeed = currentRiverSpeed = Mathf.SmoothDamp
                (currentRiverSpeed, targetRiverSpeed, ref _referenceVelocity, speedRecoveryTime / 10f);
        }
    }
    #endregion

    [Header("Temp")]
    [SerializeField, Range(0f, 1f)] float evaluation;

    private void OnValidate()
    {
        if (riverLanes == null  || worldSplineContainer == null)
        {
            Debug.LogWarning($"Missing River Lanes {riverLanes} or Spline Container {worldSplineContainer}");
            return;
        }

        // Assign the lane positions to the world spline container
        int i = -1;
        foreach (var item in riverLanes)
        {
            // Get the position of the spline
            var splinePos = 
                worldSplineContainer.EvaluatePosition(evaluation);

            // Get the directions
            var splineTangent = worldSplineContainer.EvaluateTangent(evaluation);
            var splineUp = worldSplineContainer.EvaluateUpVector(Mathf.Repeat(evaluation, 1f));

            var rot = Quaternion.LookRotation(splineTangent, splineUp);

            var pos = (item.transform.right * i) * GlobalRiverValues.RiverLaneDistance + new Vector3(splinePos.x, splinePos.y, splinePos.z);

            item.transform.SetPositionAndRotation(pos, rot);
            i++;
        }

        UpdateSplineLengths();
    }

    #region Curve Evaluations

    private float[] _splineLengths;
    /// <summary>The total length of the World Spline Container. </summary>
    public static float SplineTotalLength;

    /// <summary>Update the SplineTotalLength value based on the length of the current World Spline Container #</summary>
    private void UpdateSplineLengths()
    {
        int count = worldSplineContainer.Splines.Count;
        _splineLengths = new float[count];
        SplineTotalLength = 0f;

        for (int i = 0; i < count; i++)
        {
            float length = worldSplineContainer.CalculateLength(i);
            _splineLengths[i] = length;
            SplineTotalLength += length;
        }
    }
    
    public Vector3 EvaluatePositionOnCurve(float evaluation)
    {
        var splinePos = worldSplineContainer.EvaluatePosition(Mathf.Repeat(evaluation, 1f));

        return new Vector3(splinePos.x, splinePos.y, splinePos.z);
    }

    public Quaternion EvaluateRotationOnCurve(float evaluation)
    {
        // Get the directions
        var splineTangent = worldSplineContainer.EvaluateTangent(Mathf.Repeat(evaluation, 1f));
        var splineUp = worldSplineContainer.EvaluateUpVector(Mathf.Repeat(evaluation, 1f));

        var rot = Quaternion.LookRotation(splineTangent, splineUp);
        return rot;
    }

    /// <summary>
    /// Progresses the object on the world spline using delta time
    /// </summary>
    /// <param name="speed"> The speed of the object</param>
    /// <param name="currentProgress"> The current progress of the object on the spline</param>
    /// <param name="lane"> The current lane this object is on</param>
    /// <param name="updatedProgress">The new progress of the object on the spline</param>
    /// <param name="newPosition"> The output position of the object</param>
    /// <param name="newRotation"> The output rotation of the object</param>
    public void ProgressOnWorldSpline(float speed, float currentProgress, int lane, out float updatedProgress, 
        out Vector3 newPosition, out Quaternion newRotation)
    {
        var length = worldSplineContainer.CalculateLength();

        currentProgress += (speed * Time.deltaTime) / length;

        worldSplineContainer.Evaluate(currentProgress, out float3 position, out float3 tangent, out float3 upVector);

        updatedProgress = currentProgress;
        newPosition = new Vector3(position.x, position.y, position.z);
        newRotation = Quaternion.LookRotation(tangent, upVector);
    }

    /// <summary>
    /// Moves the object to the position on the world spline.
    /// </summary>
    /// <param name="currentProgress"> The current progress of the object on the spline</param>
    /// <param name="lane"> The current lane this object is on</param>
    /// <param name="newPosition"> The output position on the spline</param>
    /// <param name="newRotation"> The output rotation on the spline</param>
    public void AssignToWorldSpline(float currentProgress, int lane, out Vector3 newPosition, out Quaternion newRotation) // Note: Idk if this even does anything...
    {
        worldSplineContainer.Evaluate(currentProgress, out float3 position, out float3 tangent, out float3 upVector);

        newPosition = new Vector3(position.x, position.y, position.z);
        newRotation = Quaternion.LookRotation(tangent, upVector);
    }
    
    public void AssignToCurveSection(
        float distanceFromStart,
        int lane,
        out Vector3 newPosition,
        out Quaternion newRotation)
    {
        // Wrap distance along looping spline
        float wrappedDistance = Mathf.Repeat(distanceFromStart, SplineTotalLength);

        // Find which spline segment this falls on
        int splineIndex = 0;

        for (int i = 0; i < _splineLengths.Length; i++)
        {
            if (wrappedDistance <= _splineLengths[i])
            {
                splineIndex = i;
                break;
            }

            wrappedDistance -= _splineLengths[i];
        }
        
        // Convert distance to normalized t
        float t = SplineUtility.GetNormalizedInterpolation(
            worldSplineContainer.Splines[splineIndex],
            wrappedDistance,
            PathIndexUnit.Distance);

        // Evaluate position & rotation
        worldSplineContainer.Evaluate(
            splineIndex,
            t,
            out float3 pos,
            out float3 tangent,
            out float3 up);

        // lane offset
        Vector3 right = Vector3.Cross(up, tangent).normalized;
        Vector3 laneOffset = right * ((lane - 1) * GlobalRiverValues.RiverLaneDistance);
        
        // Prevent Look Rotation being zero (causing debug logs...)
        Vector3 forward = tangent;
        if (forward.sqrMagnitude < 0.1f)
            forward = Vector3.forward;
        
        newPosition = (Vector3)pos + laneOffset;
        newRotation = Quaternion.LookRotation(forward, up);
    }
    #endregion

    #region World Spline Methods

    public void UpdateWorldSpline(Spline newSpline)
    {
        worldSplineContainer.Spline = newSpline;
    }

    #endregion
}
