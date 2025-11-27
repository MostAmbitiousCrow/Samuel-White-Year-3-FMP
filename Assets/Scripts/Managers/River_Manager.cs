using System.Collections.Generic;
using UnityEngine;
using EditorAttributes;
using System;
using System.Linq;
using System.Collections;

public class River_Manager : MonoBehaviour
{
    #region Variables
    public float RiverFlowSpeed { get { return _riverFlowSpeed; } }
    [Tooltip("The current speed of the rivers flow. Affects the rivers animation.")]
    [SerializeField] float _riverFlowSpeed = 5f;

    [Tooltip("The min/max values of the levels of speed the river can reach")]
    [MinMaxSlider(0, 5)] public Vector2Int MinMaxSpeed = new(0, 5);
    /// <summary> The default speed of the river. Default value is: 1 </summary>
    public int StartingRiverSpeed { get; private set; } = 5;
    /// <summary> The speed of the river shared across river objects </summary>
    public int CurrentRiverSpeed { get; private set; } = 5;
    /// <summary> The minimum amount of distance river objects can spawn in the z axis </summary>
    public int RiverObjectSpawnDistance { get; private set; } = 45;
    /// <summary> Is the river currently paused? </summary>
    public bool IsPaused { get; private set; } = false;
    /// <summary> Is the river currently speeding up or slowing down? </summary>
    public bool IsTransitioning { get; private set; }

    [Header("River Lanes Info")]
    [SerializeField] Transform _lanesParent;

    [Serializable]
    public class RiverLane
    {
        public int ID;
        public Vector3 axis;
    }
    public List<RiverLane> RiverLanes;
    public List<IAffectedByRiver> riverInfluencedObjects = new();

    /// <summary> Action that updates all subcribed events whenever the river speed is updated </summary>
    public event Action OnRiverSpeedUpdate;

    #endregion

    private void Awake()
    {
        UpdateSpaceDatas();
        GetAndInjectAffectedRiverObjects();
    }

    #region Data Update Methods
    [Button]
    public void UpdateSpaceDatas()
    {
        RiverLanes.Clear();

        for (int i = 0; i < _lanesParent.childCount; i++)
        {
            RiverLane rl = new() { axis = _lanesParent.GetChild(i).position, ID = i };
            RiverLanes.Add(rl);
        }
        print($"Updated River Lanes to {RiverLanes.Count} lanes");
    }
    #endregion

    #region Injection
    [Button]
    public void GetAndInjectAffectedRiverObjects()
    {
        riverInfluencedObjects = new List<IAffectedByRiver>(FindObjectsOfType<MonoBehaviour>().OfType<IAffectedByRiver>());
        foreach (var item in riverInfluencedObjects)
        {
            item.InjectRiverManager(this);
        }
        print($"Injected {this} into {riverInfluencedObjects.Count} objects");
    }

    void Start()
    {
        OnRiverSpeedUpdate?.Invoke();
    }
    #endregion

    #region Lane and Space Checks

    /// <summary>
    /// Returns a true/false if a lane exists within the list of lanes
    /// </summary>
    public bool CheckAvailableLane(int lane)
    {
        if (lane > RiverLanes.Count || lane < 0) return false;
        else return true;
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

        if (targetLane < spaces && targetLane > -1) return RiverLanes[targetLane];
        else return RiverLanes[currentLane];
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
        return RiverLanes[lane];
    }

    /// <summary>
    /// Returns the list containing all Lane Datas
    /// </summary>
    public List<RiverLane> GetLanes()
    {
        return RiverLanes;
    }
    #endregion

    #region River Modification
    [Header("Animation")]
    [Tooltip("The curve representing the slow down transition")] // Don't know any other way to describe it :sob
    [SerializeField] AnimationCurve slowCurve;

    [Tooltip("The curve representing the speed up transition")] // Don't know any other way to describe it :sob
    [SerializeField] AnimationCurve speedCurve;

    /// <summary> The method to slow down the global river speed </summary>
    /// <param name="amount"></param>
    /// <param name="multiplier"></param>
    public void SlowDownRiver(int amount = 1, float multiplier = 10f)
    {
        int targetSpeed = CurrentRiverSpeed - amount;
        CurrentRiverSpeed = targetSpeed;

        if (targetSpeed < MinMaxSpeed.x) // If target speed is less than the min speed value
        {
            print("River speed has reached minimum speed");
            return;
        }
        OnRiverSpeedUpdate.Invoke();
        speedRoutine = StartCoroutine(RiverSpeedIncreaseRoutine(targetSpeed, true, multiplier));
    }

    /// <summary> The method to speed up the global river speed </summary>
    public void SpeedUpRiver(int amount = 1, float multiplier = 5f)
    {
        int targetSpeed = CurrentRiverSpeed + amount;

        if (targetSpeed > MinMaxSpeed.y) // If target speed is less than the max speed value
        {
            print("River speed has reached maximum speed!");
            return;
        }
        CurrentRiverSpeed = targetSpeed;

        OnRiverSpeedUpdate.Invoke();
        speedRoutine = StartCoroutine(RiverSpeedIncreaseRoutine(targetSpeed, false, multiplier));
    }

    private Coroutine speedRoutine;
    IEnumerator RiverSpeedIncreaseRoutine(int targetspeed, bool decrease, float multiplier)
    {
        float startSpeed = CurrentRiverSpeed;

        float t = 0f;

        if (decrease)
        {
            yield return new WaitUntil(() => !IsTransitioning); // Wait until the fast routine has finished before slowing down

            t = 1f;

            while (t > 0f) // Slowing down
            {
                yield return new WaitUntil(() => !IsPaused); // Wait if paused
                
                t -= Time.deltaTime * multiplier * GameManager.GameLogic.GamePauseInt;

                _riverFlowSpeed = Mathf.Lerp(targetspeed, startSpeed, Mathf.Round(slowCurve.Evaluate(t) * 100f) / 100f);
                yield return null;
            }
            IsTransitioning = false;
        }
        else
        {
            yield return new WaitUntil(() => !IsTransitioning); // Wait until the slow routine has finished before speeding up

            t = 0f;

            while (t < 1f) // Speeding up
            {
                yield return new WaitUntil(() => !IsPaused); // Wait if paused

                t += Time.deltaTime * multiplier * GameManager.GameLogic.GamePauseInt;

                _riverFlowSpeed = Mathf.Lerp(startSpeed, targetspeed, Mathf.Round(slowCurve.Evaluate(t) * 100f) / 100f);
                yield return null;
            }
            IsTransitioning = false;
        }
        _riverFlowSpeed = targetspeed;
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
        if (speedRoutine != null) StopCoroutine(speedRoutine);
        CurrentRiverSpeed = StartingRiverSpeed;
    }
    #endregion

    /*
    #region Player Progress

    [Header("Player Progress")]
    [SerializeField, ProgressBar, Range(0f, 100f)] float _visualProgress = 0f;
    [SerializeField, ProgressBar, Range(0f, 100f)] float _actualProgress = 0f;

    void UpdateProgress(float multiplier = 1f)
    {
        (_actualProgress, _visualProgress) = CalculateProgress(multiplier);
        UpdateProgressElements();
    }

    void UpdateProgressElements()
    {
        Game_UI.Instance.UpdatePlayerProgressMeter(_visualProgress);
    }

    (float, float) CalculateProgress(float multiplier)
    {
        float a = _actualProgress += Time.deltaTime * RiverSpeed *
             multiplier * GameManager.GameLogic.GamePauseInt;
        float v = _visualProgress = Mathf.Round(a * 10f) / 10f;
        return (a, v);
    }

    #endregion
    */
}
