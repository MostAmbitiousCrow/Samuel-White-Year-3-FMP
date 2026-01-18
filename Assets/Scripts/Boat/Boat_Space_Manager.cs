using System.Collections.Generic;
using UnityEngine;
using EditorAttributes;
using System;
using System.Linq;
using GameCharacters;
using UnityEngine.Serialization;

public class Boat_Space_Manager : MonoBehaviour
{
    //[SerializeField] Boat_Controller _boatController;
    //public Boat_Controller BoatController { get { return _boatController; } }

    public static Boat_Space_Manager Instance { get; private set; }
    //public List<IBoatSpaceMovement> boatSpaceObjects = new();

    [Serializable]
    public class BoatSide
    {
        [Serializable]
        public class SpaceData
        {
            public Transform t;
            public int spaceID;
            public int sideID;
            /// <summary> Is this space inside the boat? </summary>
            public bool insideBoat = true;

            [ReadOnly] public bool isOccupied = false;
        }
        public List<SpaceData> SpaceDatas = new();
    }
    public List<BoatSide> BoatSides = new();
    public int SpaceCount { get; private set; }

    [Header("Data")]
    [SerializeField] GlobalRiverValues _globalRiverValues;

    [Button]
    public void UpdateSpaceDatas()
    {
        BoatSides.Clear();

        for (int i = 0; i < transform.childCount; i++)
        {
            // Loop through each Boat side
            BoatSide bs = new();

            // Get side
            Transform side = transform.GetChild(i);

            // Get spaces from the -Spaces- Folder
            Transform currentSpaces = side.GetChild(0);

            // Cycle through the spaces in the -Spaces- folder
            for (int j = 0; j < currentSpaces.childCount; j++)
            {
                // Add new space to the current side
                Transform space = currentSpaces.GetChild(j);
                print(space);
                BoatSide.SpaceData sd = new()
                {
                    spaceID = j,
                    sideID = i,
                    t = space,
                };
                bs.SpaceDatas.Add(sd);
            }

            bs.SpaceDatas.First().insideBoat = false;
            bs.SpaceDatas.Last().insideBoat = false;

            BoatSides.Add(bs);
        }
    }

    #region Injection
    //[Button]
    //public void GetAndInjectBoatAffectedObjects()
    //{
    //    boatSpaceObjects = new List<IBoatSpaceMovement>(FindObjectsOfType<MonoBehaviour>().OfType<IBoatSpaceMovement>());
    //    foreach (var item in boatSpaceObjects)
    //    {
    //        item.InjectBoatSpaceManager(this);
    //    }
    //    print($"Injected {this} into {boatSpaceObjects.Count} objects");
    //}
    #endregion

    private void Awake()
    {
        //GetAndInjectBoatAffectedObjects();
        Instance = this;
        SpaceCount = BoatSides[0].SpaceDatas.Count;
    }

    private void OnValidate()
    {
        Instance = this;
        SpaceCount = BoatSides[0].SpaceDatas.Count;
    }

    #region Lane and Space Checks
    /// <summary> 
    /// Returns a true/false if the space exists within the lane
    /// </summary>
    public bool CheckAvailableSpace(int side, int space)
    {
        if (space > BoatSides[side].SpaceDatas.Count || space < 0) return false;
        else return true;
    }

    /// <summary>
    /// Get Space Data based on a given direction.
    /// Checks if there is a space available, will otherwise return the initial provided space
    /// </summary>
    public BoatSide.SpaceData GetSpaceFromDirection(int currentSide, int currentSpace, int direction)
    {
        int spaces = GetSpaces(currentSide).Count;
        int targetSpace = currentSpace + direction;

        // If target is within bounds of the space, return the targeted boat space
        if (targetSpace < spaces && targetSpace > -1) return BoatSides[currentSide].SpaceDatas[targetSpace];
        // Otherwise, just return the current space
        else return BoatSides[currentSide].SpaceDatas[currentSpace];
    }

    /// <summary>
    /// Obtain the space of the opposite side of the boat from a given side and space
    /// </summary>
    public BoatSide.SpaceData GetSpaceFromOppositeLane(int currentSide, int currentSpace)
    {
        int newSide = currentSide == 0 ? 1 : 0;
        return BoatSides[newSide].SpaceDatas[currentSpace];
    }

    /// <summary>
    /// Get the specific space from the given side of the boat
    /// </summary>
    public BoatSide.SpaceData GetSpace(int side, int space)
    {
        return BoatSides[side].SpaceDatas[space];
    }

    /// <summary>
    /// Get all spaces from the given side of the boat
    /// </summary>
    public List<BoatSide.SpaceData> GetSpaces(int side)
    {
        return BoatSides[side].SpaceDatas;
    }

    /// <summary>
    /// Gets the specific space from the given Side Follow Space of the boat
    /// </summary>
    public BoatSide.SpaceData GetSideSpace(int side, bool getLeftSide)
    {
        if (getLeftSide) return BoatSides[side].SpaceDatas.First();
        else return BoatSides[side].SpaceDatas.Last();
    }

    /// <summary>
    /// Gets the specific space from the spaces directly on the boat. Sides start at zero.
    /// </summary>
    public BoatSide.SpaceData GetBoatSpace(int side, int space)
    {
        print($"Getting Space: {side} and Space: {space}");
        print($"Space Datas =  {BoatSides[side].SpaceDatas.Count}.");
        if (space < 1) return BoatSides[side].SpaceDatas[1];
        if (space > SpaceCount - 2) return BoatSides[side].SpaceDatas[SpaceCount - 2];
        return BoatSides[side].SpaceDatas[space];
    }
    #endregion

    #region Get Boat Centre
    /// <summary>
    /// Get the position of the boat (aka the centre)
    /// </summary>
    public Vector3 GetBoatCentre()
    {
        return transform.position;
    }

    /// <summary>
    /// Returns true or false based on if the boat space is inside, or outside, the boat and whether
    /// the character can access it with given outer and inner side access checks
    /// </summary>
    public bool CheckSpaceAccess(bool outerSideAccess, bool innerSideAccess, BoatSide.SpaceData spaceData)
    {
        bool result = spaceData.insideBoat ? innerSideAccess : outerSideAccess;

        //print($"Result: {result} | Can Access Inner = {innerSideAccess} | Can Acess Outer = {outerSideAccess}");
        return result;
    }
    #endregion

    /// <summary>
    /// The distance from the given distance value to the players boat.
    /// </summary>
    public float GetDistanceToBoat(float targetDistance)
    {
        return targetDistance - transform.position.z;
    }

    #region Boat Passenger Checks

    [Space(10)]
    public List<BoatCharacter> boatPassengers = new();
    [FormerlySerializedAs("_passengerFolder")] [SerializeField] Transform passengerFolder;

    public void AddPassenger(BoatCharacter passenger)
    {
        boatPassengers.Add(passenger);
        passenger.transform.SetParent(passengerFolder);
    }

    public void RemovePassenger(BoatCharacter passenger)
    {
        boatPassengers.Remove(passenger);
        passenger.transform.SetParent(null);
    }
    #endregion

    // Adjust the side spaces of the boat to match the global River Boat Side Space Distance
    private void OnDrawGizmos()
    {
        if (BoatSides == null || _globalRiverValues == null)
        {
            Debug.LogWarning("Missing Global River Values or Boat Sides");
            return;
        }
        if (Application.isPlaying) return;

        for (int i = 0; i < BoatSides.Count; i++)
        {
            BoatSide bs = BoatSides[i];
            // Sides
            bs.SpaceDatas.First().t.position = new Vector3(_globalRiverValues.boatSideSpaceDistance, 0, bs.SpaceDatas[1].t.position.z);
            bs.SpaceDatas.Last().t.position = new Vector3(_globalRiverValues.boatSideSpaceDistance * -1, 0, bs.SpaceDatas[^1].t.position.z);
            
            // Boat Spaces //TODO
            for (int j = 0; j < bs.SpaceDatas.Count; j++)
            {
                if (j == 0 || j == bs.SpaceDatas.Count - 1) continue; // Skip first and last space data
                bs.SpaceDatas[j].t.position = new Vector3(_globalRiverValues.boatSpaceDistance, 0, .5f);
            }

        }

    }

}