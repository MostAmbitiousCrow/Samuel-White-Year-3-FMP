using System;
using CarterGames.Assets.AudioManager;
using EditorAttributes;
using UnityEngine;
using UnityEngine.Serialization;
using static Boat_Space_Manager.BoatSide;

public class Boat_Controller : MonoTimeBehaviour, IRiverLaneMovement //, IDamageable
{
    #region Variables
    [Line(GUIColor.Green)]
    [Header("Boat Settings")]
    public float steerSpeed = 1;
    public AnimationCurve steerInterpolationCurve;
    [FormerlySerializedAs("_currentLane")]
    [Space(10)]
    [Tooltip("The current lane of the boat the character is standing on")]
    [SerializeField, ReadOnly] private int currentLane;
    [Tooltip("What lane should this object start on? (if applicable)")]
    public int startLane = 1;
    [Space(10)]
    [Tooltip("The duration of the stun after hitting an obstacle")]
    [SerializeField] private float stunDuration = 1.5f;
    [Tooltip("How much of the current boats speed is decreased when an obstacle is hit")]
    [SerializeField] private float stunSlowMultiplier = .5f;
    [Space(10)]
    [SerializeField, ReadOnly] private bool isMoving;
    public bool IsMoving => isMoving;

    private Vector3 _currentMoveTarget;
    private Vector3 _startMovePosition;
    private float _moveElapsed;
    [SerializeField] private float steerDuration = 0.35f;

    private int _direction = 0;

    [Header("Roll Settings")] 
    [SerializeField] private float rollAmount = 15f;
    [SerializeField] private AnimationCurve rollCurve;
    
    [Header("Spline Movement")]
    [SerializeField] private RiverSplineObject riverSplineObject;
    public RiverSplineObject RiverSplineObject => riverSplineObject;

    [Header("Sound")] 
    [SerializeField] private InspectorAudioClipPlayer boatSteerSfx;

    [Header("Components")]
    [SerializeField] private Transform propellerArt;
    [SerializeField] private ArtExplode artExplode;
    [SerializeField] private RiverSplineObject splineObject;

    #region Event Listeners
    public static event Action OnBoatMoved;
    public static event Action OnSteeredLeftAction;
    public static event Action OnSteeredRightAction;
    #endregion
    #endregion

    private void Awake()
    {
        splineObject = GetComponentInParent<RiverSplineObject>();
        artExplode = GetComponent<ArtExplode>();
    }

    private void Start()
    {
        GoToLane(startLane);
    }

    /// <summary>
    /// The main method to steer the players boat in a given direction and animates the force.
    /// </summary>
    public void SteerBoat(SpaceData spaceData, float force)
    {
        Transform spaceTransform = spaceData.t;
        Vector3 localPos = transform.InverseTransformPoint(spaceTransform.position);

        _direction = Mathf.RoundToInt(Mathf.Sign(localPos.x));

        // Skip if the direction wasn't either left or right
        if (_direction == 0) return;

        // Trigger Steered Action Direction (for the tutorial)
        if (_direction > 0f) OnSteeredRightAction?.Invoke();
        else OnSteeredLeftAction?.Invoke();
        
        MoveToLane(_direction);
        
        // Play Steer SFX
        boatSteerSfx.Stop();
        boatSteerSfx.Play();
        
        // Trigger OnBoatMoved listeners
        OnBoatMoved?.Invoke();
        // Debug.Log("Boat was steered!");
    }

    public void MoveToLane(int direction)
    {
        River_Manager.RiverLane rl = River_Manager.Instance.GetLaneFromDirection(currentLane, direction);
        if (rl == null) return;

        currentLane = rl.ID;

        Vector3 lanePos = rl.transform.localPosition;

        // IMPORTANT:
        // Use CURRENT position as new start (allows mid-steer blending)
        _startMovePosition = transform.localPosition;
        _currentMoveTarget = new Vector3(lanePos.x, lanePos.y, transform.localPosition.z);

        _moveElapsed = 0f;
        isMoving = true;
    }
    
    public void GoToLane(int lane)
    {
        River_Manager.RiverLane rl = River_Manager.Instance.GetLane(lane);

        var pos = rl.transform.localPosition;
        currentLane = rl.ID;
        transform.localPosition = new(pos.x, pos.y, transform.localPosition.z);
    }

    public int GetCurrentLane()
    {
        return currentLane;
    }

    #region Movement
    protected override void TimeUpdate()
    {
        if (isMoving) SteerMovement();
        AnimatePropeller();
    }
    
    private void SteerMovement()
    {
        _moveElapsed += Time.deltaTime / Mathf.Max(steerDuration, 0.0001f);

        float t = Mathf.Clamp01(_moveElapsed);
        float steerT = steerInterpolationCurve?.Evaluate(t) ?? t;

        // Move the boat
        Vector3 newPosition = Vector3.Lerp(_startMovePosition, _currentMoveTarget, steerT);
        transform.localPosition = newPosition;
        
        float rollT = rollCurve.Evaluate(_moveElapsed);
        // Roll the boat!
        float roll = _direction > 0? 
            Mathf.Lerp(-rollAmount, 0f, rollT) // Roll Left
            : 
            Mathf.Lerp(rollAmount, 0f, rollT); // Roll Right
        transform.localRotation = Quaternion.Euler(0f, 0f, roll);

        // Set the boat to its move target once travel time has ended
        if (!(t >= 1f)) return;
        transform.localPosition = _currentMoveTarget;
        transform.localRotation = Quaternion.identity;

        isMoving = false;
    }

    private void AnimatePropeller()
    {
        propellerArt.Rotate(Vector3.up, River_Manager.Instance.currentRiverSpeed * 1000f * Time.deltaTime);
    }

    public void DestroyBoat()
    {
        riverSplineObject.StopMoving();
        artExplode.ExplodeArt();
    }
    #endregion

    #region Injection
    //public void InjectRiverManager(River_Manager manager)
    //{
    //    riverManager = manager;
    //    print($"Injected {manager} into {name}");
    //}
    #endregion

    #region Damage Events

    public void TookDamage()
    {
        print("Boat hit, slowing river");
        River_Manager.Instance.SlowDownRiver();
    }

    public void Died()
    {
        return;
    }

    public void HealthRestored()
    {
        return;
    }


    #endregion
}
