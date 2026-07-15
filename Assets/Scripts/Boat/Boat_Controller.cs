using System;
using System.Collections;
using CameraShake;
using CarterGames.Assets.AudioManager;
using EditorAttributes;
using GameCharacters;
using UnityEngine;
using static Boat_Space_Manager.BoatSide;
using static River_Manager;

public class Boat_Controller : MonoTimeBehaviour, IRiverLaneMovement
{
    #region Variables
    [Line(GUIColor.Green)]
    [Header("Boat Settings")]
    public RiverLane CurrentLane { get; set; }
    private RiverLane _returningLane;
    public float steerSpeed = 1;
    public AnimationCurve steerInterpolationCurve;
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
    private bool _willReturn;

    private Vector3 _currentMoveTarget;
    private Vector3 _startMovePosition;
    private float _moveElapsed;
    [SerializeField] private float steerDuration = 0.35f;
    [SerializeField] private float returnSteerMultiplier = 2f;
    private float _steerMultiplier = 1f;

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
    [SerializeField] private ParticleSystem smokeParticles;

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
        GameLevelManager.OnLevelLoaded += TriggerLevelEnterSpeedUp;
    }

    private void OnEnable()
    {
        PlayerCharacter.OnPlayerDied += DestroyBoat;
        TsunamiController.OnTsunamiUpdated += UpdateSmokeParticles;
    }

    private void OnDisable()
    {
        PlayerCharacter.OnPlayerDied -= DestroyBoat;
        TsunamiController.OnTsunamiUpdated -= UpdateSmokeParticles;
        GameLevelManager.OnLevelLoaded -= TriggerLevelEnterSpeedUp;
    }

    /// <summary>
    /// The main method to steer the players boat in a given direction and animates the force.
    /// </summary>
    public void SteerBoat(SpaceData spaceData, float force)
    {
        // Prevent the player from steering if their boat is returning
        if (_willReturn) return;
        
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

    public void MoveToLaneFromDirection(int direction)
    {
        throw new NotImplementedException();
    }
    
    public void MoveToLane(RiverLane lane)
    {
        if (lane == null) return;

        // If the river lane is block, set lane to return to
        _willReturn = lane.isBlocked;
        if (_willReturn) _returningLane = CurrentLane;
        
        CurrentLane = lane;

        Vector3 lanePos = lane.transform.localPosition;

        // Use current position as new start (allows mid-steer blending)
        _startMovePosition = transform.localPosition;


        _currentMoveTarget = new Vector3(lanePos.x, lanePos.y, transform.localPosition.z);

        _moveElapsed = 0f;
        isMoving = true;
    }

    public void MoveToLane(int direction)
    {
        RiverLane rl = Instance.GetLaneFromDirection(CurrentLane.id, direction);
        if (rl == null) return;

        // If the river lane is block, set lane to return to
        _willReturn = rl.isBlocked;
        if (_willReturn) _returningLane = CurrentLane;
        
        CurrentLane = rl;

        Vector3 lanePos = rl.transform.localPosition;

        // Use current position as new start (allows mid-steer blending)
        _startMovePosition = transform.localPosition;


        _currentMoveTarget = new Vector3(lanePos.x, lanePos.y, transform.localPosition.z);

        _moveElapsed = 0f;
        isMoving = true;
    }
    
    public void GoToLane(int lane)
    {
        RiverLane rl = Instance.GetLane(lane);

        var pos = rl.transform.localPosition;
        CurrentLane = rl;
        transform.localPosition = new(pos.x, pos.y, transform.localPosition.z);
    }

    #region Movement
    protected override void TimeUpdate()
    {
        if (isMoving) SteerMovement();
        AnimatePropeller();
    }
    
    private void SteerMovement()
    {
        _moveElapsed += Time.deltaTime / Mathf.Max(steerDuration / _steerMultiplier, 0.0001f);

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

        if (_willReturn)
        {
            _willReturn = false;
            CurrentLane = _returningLane;
            _returningLane = null;
            _direction *= -1; // Revert Direction
            _steerMultiplier = returnSteerMultiplier;
            MoveToLane(CurrentLane);
            isMoving = true;
        }
        else
        {
            isMoving = false;
            _steerMultiplier = 1f;
        }
    }

    private void AnimatePropeller()
    {
        propellerArt.Rotate(Vector3.up, Instance.currentRiverSpeed * 1000f * Time.deltaTime);
    }
    #endregion

    #region Events

    [Header("Level Enter Data")]
    [SerializeField] private int levelEnterSpeed = 30;
    [SerializeField] private float levelExitSpeedResetTime = 1.5f;

    private void TriggerLevelEnterSpeedUp()
    {
        StartCoroutine(LevelEnterSpeedUpRoutine());
    }

    private IEnumerator LevelEnterSpeedUpRoutine()
    {
        var cachedSpeed = River_Manager.Instance.TargetRiverSpeed;

        var enterSpeed = GameLevelManager.Instance.CalculateDifficulty().difficulty switch
        {
            GameDifficulty.Easy => 20,
            GameDifficulty.Medium => 30,
            GameDifficulty.Hard => 30,
            _ => throw new ArgumentOutOfRangeException()
        };
        
        if (cachedSpeed > enterSpeed) enterSpeed = cachedSpeed;
        if (enterSpeed < levelEnterSpeed) enterSpeed = levelEnterSpeed;
        
        Instance.SetRiverSpeed(enterSpeed, true, false);
        
        yield return new WaitForSeconds(levelExitSpeedResetTime);
        
        Instance.SetRiverSpeed(cachedSpeed, true, false);
    }

    
    private void UpdateSmokeParticles(int steps)
    {
        if (steps <= 0) smokeParticles.Stop();
        else smokeParticles.Play();
        
        var rot = smokeParticles.emission;
        rot.rateOverTime = steps * 10;
        Debug.Log($"Updated SmokeParticles. Value = {steps} Emission = {rot.rateOverTime.constant}");
    }
    
    #endregion

    #region Damage Events

    public void TakeDamage()
    {
        Instance.HaltRiver(10);
        AudioManager.Play(Clip.Crash);
        CameraShaker.Presets.Explosion3D();
        // print("Boat hit, halted river");
    }

    public void DestroyBoat(DamageType damageType)
    {
        // Only destroy the boat if it was the Tsunami that killed the player
        if (damageType != DamageType.Tsunami) return;
        
        riverSplineObject.StopMoving();
        artExplode.ExplodeArt();
    }

    #endregion
}
