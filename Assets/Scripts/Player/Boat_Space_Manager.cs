using System.Collections.Generic;
using UnityEngine;
using EditorAttributes;
using System;
using System.Linq;

public class Boat_Space_Manager : MonoBehaviour
{
    public Boat_Controller BoatController { get; private set; }
    public List<IBoatSpaceMovement> boatSpaceObjects = new();

    [Serializable]
    public class BoatSide
    {
        [Serializable]
        public class SpaceData
        {
            public Transform t;
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
                    t = o.GetChild(l),
                };
                bs.SpaceDatas.Add(sd);
            }
            BoatSides.Add(bs);
        }
    }
    [Button]
    public void GetAndInjectBoatAffectedObjects()
    {
        boatSpaceObjects = new List<IBoatSpaceMovement>(FindObjectsOfType<MonoBehaviour>().OfType<IBoatSpaceMovement>());
        foreach (var item in boatSpaceObjects)
        {
            item.InjectBoatSpaceManager(this);
        }
        print($"Injected {this} into {boatSpaceObjects.Count} objects");
    }

    #region Injection
    void Awake()
    {
        GetAndInjectBoatAffectedObjects();
        BoatController = FindAnyObjectByType<Boat_Controller>();
    }
    #endregion

    #region Lane and Space Checks
    // Returns a true/false if the space exists within the lane
    public bool CheckAvailableSpace(int side, int space)
    {
        if (space > BoatSides[side].SpaceDatas.Count || space < 0) return false;
        else return true;
    }

    // Get Space Data based on a given direction
    // Checks if there is a space available, will otherwise return the initial provided space
    public BoatSide.SpaceData GetSpaceFromDirection(int currentLane, int currentSpace, int direction)
    {
        int spaces;
        int targetSpace;

        spaces = GetSpaces(currentLane).Count;
        targetSpace = currentSpace + direction;

        if (targetSpace < spaces && targetSpace > -1) return BoatSides[currentLane].SpaceDatas[targetSpace];
        else return BoatSides[currentLane].SpaceDatas[currentSpace];
    }

    // Obtain the ID number of the opposite lane
    public int GetOppositeLaneID(int currentLane)
    {
        return currentLane == 0 ? 1 : 0;
    }

    // Get Space Data
    public BoatSide.SpaceData GetSpace(int lane, int space)
    {
        return BoatSides[lane].SpaceDatas[space];
    }

    // Get All Space Data
    public List<BoatSide.SpaceData> GetSpaces(int lane)
    {
        return BoatSides[lane].SpaceDatas;
    }
    #endregion

    #region Get Boat Centre
    public Vector3 GetBoatCentre()
    {
        return transform.position;
    }
    #endregion

    public float GetDistanceToBoat(float targetDistance)
    {
        return targetDistance - transform.position.z;
    }

    #region Boat Passenger Checks

    [Space(10)]
    public List<IBoatSpaceMovement> BoatPassengers = new();
    [SerializeField] Transform _passengerFolder;

    public void AddPassenger(Boat_Character passenger)
    {
        BoatPassengers.Add(passenger);
        passenger.transform.SetParent(_passengerFolder);
    }

    public void RemovePassenger(Boat_Character passenger)
    {
        BoatPassengers.Remove(passenger);
        passenger.transform.SetParent(null);
    }
    #endregion
}
