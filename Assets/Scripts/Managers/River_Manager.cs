using System.Collections.Generic;
using UnityEngine;
using EditorAttributes;
using System;
using System.Linq;

public class River_Manager : MonoBehaviour
{
    [Header("River Stats")]
    public float RiverSpeed = 1f;
    public SO_RiverLaneData RiverLaneData; //TODO: Implement this somehow

    [Header("River Lanes Info")]
    [SerializeField] Transform _lanesParent;

    [Serializable]
    public class RiverLane
    {
        public int ID;
        public Vector2 axis;
    }
    public List<RiverLane> RiverLanes;
    public List<IAffectedByRiver> riverInfluencedObjects = new();
    
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

    #region Injection
    void Awake()
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
}
