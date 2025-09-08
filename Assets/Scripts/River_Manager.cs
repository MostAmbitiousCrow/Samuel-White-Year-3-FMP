using System.Collections.Generic;
using UnityEngine;
using EditorAttributes;
using System;

public class River_Manager : MonoBehaviour
{
    public static River_Manager instance;

    [SerializeField] Transform _lanesParent;

    [Serializable]
    public class RiverLane
    {
        public int ID;
        public Vector2 axis;
    }
    public List<RiverLane> RiverLanes;
    
    [Button]
    public void UpdateSpaceDatas()
    {
        RiverLanes.Clear();

        for (int i = 0; i < _lanesParent.childCount; i++)
        {
            RiverLane rl = new()
            {
                axis = _lanesParent.GetChild(i).position,
                ID = i
            };
            RiverLanes.Add(rl);
        }
        print($"Updated River Lanes to {RiverLanes.Count} lanes");
    }

    private void Awake()
    {
        instance = this;
    }

    #region Lane and Space Checks

    // Returns a true/false if a lane exists within the list of lanes
    public static bool CheckAvailableLane(int lane)
    {
        if (lane > instance.RiverLanes.Count || lane < 0) return false;
        else return true;
    }

    // Get Lane Data based on a given direction
    // Checks if there is a lane available, will otherwise return the initial provided lane
    public static RiverLane GetLaneFromDirection(int currentLane, int direction)
    {
        int spaces;
        int targetLane;

        spaces = GetLanes().Count;
        targetLane = currentLane + direction;

        if (targetLane < spaces && targetLane > -1) return instance.RiverLanes[targetLane];
        else return instance.RiverLanes[currentLane];
    }

    // Obtain the ID number of the opposite lane
    public static int GetOppositeLaneID(int currentLane)
    {
        return currentLane == 0 ? 1 : 0;
    }

    // Get Lane Data
    public static RiverLane GetLane(int lane)
    {
        return instance.RiverLanes[lane];
    }

    // Get All Lane Datas
    public static List<RiverLane> GetLanes()
    {
        return instance.RiverLanes;
    }
    #endregion
}
