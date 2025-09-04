using System.Collections.Generic;
using UnityEngine;
using EditorAttributes;
using System;

public class Boat_Space_Manager : MonoBehaviour
{
    public static Boat_Space_Manager instance;

    [Serializable]
    public class BoatSide
    {
        // public List<Vector3> SpaceDatas = new();
        [Serializable]
        public class SpaceData
        {
            public Vector3 position = new();
            public int ID;
        }
        public List<SpaceData> SpaceDatas = new();
    }
    public List<BoatSide> BoatSides;

    [Button]
    public void UpdateSpaceDatas()
    {
        BoatSides.Clear();

        Transform o;

        for (int i = 0; i < transform.childCount; i++)
        {
            BoatSide bs = new();

            o = transform.GetChild(i);
            print(o);
            for (int l = 0; l < o.childCount; l++)
            {
                print(o.GetChild(l));
                BoatSide.SpaceData sd = new()
                {
                    ID = l,
                    position = o.GetChild(l).position,
                };
                bs.SpaceDatas.Add(sd);
            }
            BoatSides.Add(bs);
        }
    }

    private void Awake()
    {
        instance = this;
    }

    #region Lane and Space Checks
    // Returns a true/false if the space exists within the lane
    public static bool CheckAvailableSpace(int side, int space)
    {
        if (space > instance.BoatSides[side].SpaceDatas.Count || space < 0) return false;
        else return true;
    }

    // Get Space Data based on a given direction
    // Checks if there is a space available, will otherwise return the initial provided space
    public static BoatSide.SpaceData GetSpaceFromDirection(int currentLane, int currentSpace, int direction)
    {
        int spaces;
        int targetSpace;

        spaces = GetSpaces(currentLane).Count;
        targetSpace = currentSpace + direction;

        if (targetSpace < spaces && targetSpace > -1) return instance.BoatSides[currentLane].SpaceDatas[targetSpace];
        else return instance.BoatSides[currentLane].SpaceDatas[currentSpace];
    }

    // Obtain the ID number of the opposite lane
    public static int GetOppositeLaneID(int currentLane)
    {
        return currentLane == 0 ? 1 : 0;
    }

    // Get Space Data
    public static BoatSide.SpaceData GetSpace(int lane, int space)
    {
        return instance.BoatSides[lane].SpaceDatas[space];
    }

    // Get All Space Data
    public static List<BoatSide.SpaceData> GetSpaces(int lane)
    {
        return instance.BoatSides[lane].SpaceDatas;
    }
    #endregion
}
