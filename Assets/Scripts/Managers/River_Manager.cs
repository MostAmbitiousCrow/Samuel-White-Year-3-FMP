using System.Collections.Generic;
using UnityEngine;
using EditorAttributes;
using System;
using System.Linq;
using System.Collections;

public class River_Manager : MonoBehaviour
{
    #region Variables
    /// <summary>
    /// The default speed of the river. Default value is: 1
    /// </summary>
    public float DefaultRiverSpeed { get; private set; } = 5f;
    /// <summary>
    /// The speed of the river shared across river objects
    /// </summary>
    public float RiverSpeed { get; private set; } = 5f;
    /// <summary>
    /// The minimum amount of distance river objects can spawn in the z axis
    /// </summary>
    public int RiverObjectSpawnDistance { get; private set; } = 45;
    /// <summary>
    /// Is the river currently paused?
    /// </summary>
    /// <value></value>
    public bool IsPaused { get; private set; } = false;
    /// <summary>
    /// Is the river currently speeding up or slowing down?
    /// </summary>
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
        GetAndInjectAffectedRiverObjects();
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

    /// <summary>
    /// The method to slow down the global river speed
    /// </summary>
    /// <param name="amount"></param>
    /// <param name="multiplier"></param>
    public void SlowDownRiver(float amount = 2f, float multiplier = 10f)
    {
        float targetSpeed = RiverSpeed / amount;

        speedRoutine = StartCoroutine(RiverSpeedroutine(targetSpeed, true, multiplier));
    }

    /// <summary>
    /// The method to speed up the global river speed
    /// </summary>
    /// <param name="amount"></param>
    /// <param name="multiplier"></param>
    public void SpeedUpRiver(float amount = 2f, float multiplier = 1f)
    {
        float targetSpeed = RiverSpeed * amount;

        speedRoutine = StartCoroutine(RiverSpeedroutine(targetSpeed, false, multiplier));
    }

    private Coroutine speedRoutine;
    //private Coroutine fastRoutine;
    //private Coroutine slowRoutine;
    IEnumerator RiverSpeedroutine(float targetspeed, bool reversed, float multiplier)
    {
        float startSpeed = RiverSpeed;
        float t = 0f;

        if (reversed)
        {
            yield return new WaitUntil(() => !IsTransitioning); // Wait until the fast routine has finished before slowing down

            t = 1f;

            while (t > 0f) // Slowing down
            {
                yield return new WaitUntil(() => !IsPaused); // Wait if paused
                
                t -= Time.deltaTime * multiplier;

                RiverSpeed = Mathf.Lerp(targetspeed, startSpeed, Mathf.Round(slowCurve.Evaluate(t) * 100f) / 100f);

                print($"River Speed: {RiverSpeed}");
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

                t += Time.deltaTime * multiplier;

                RiverSpeed = Mathf.Lerp(startSpeed, targetspeed, Mathf.Round(slowCurve.Evaluate(t) * 100f) / 100f);

                print($"River Speed: {RiverSpeed}");
                yield return null;
            }
            IsTransitioning = false;
        }
        RiverSpeed = targetspeed;
    }

    /// <summary>
    /// Completely stops the speed of the river with optional smoothing
    /// </summary>
    public void StopRiver(bool smoothing = false, float smoothAmount = 1f)
    {
        IsPaused = true;
        // TODO add optional smoothing towards stopping
    }

    /// <summary>
    /// Resumes the paused river with optional smoothing
    /// </summary>
    public void ResumeRiver(bool smoothing = false, float smoothAmount = 1f)
    {
        IsPaused = false;
    }

    /// <summary>
    /// Completely resets all changes made to the river to their default value and stops any speed transitions
    /// </summary>
    public void ResetRiver()
    {
        //if (fastRoutine != null) StopCoroutine(fastRoutine);
        //if (slowRoutine != null) StopCoroutine(slowRoutine);
        if (speedRoutine != null) StopCoroutine(speedRoutine);
        RiverSpeed = DefaultRiverSpeed;
    }
    #endregion
}
