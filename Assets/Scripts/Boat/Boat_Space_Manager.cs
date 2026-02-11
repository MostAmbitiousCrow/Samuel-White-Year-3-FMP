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
        public List<SpaceData> spaceDatas = new();
    }
    public List<BoatSide> boatSides = new();
    public int SpaceCount { get; private set; }
    
    [Header("Data")]
    [SerializeField] private GlobalRiverValues globalRiverValues;

    [Button]
    public void UpdateSpaceDatas()
    {
        boatSides.Clear();

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
                // print(space);
                BoatSide.SpaceData sd = new()
                {
                    spaceID = j,
                    sideID = i,
                    t = space,
                };
                bs.spaceDatas.Add(sd);
            }

            bs.spaceDatas.First().insideBoat = false;
            bs.spaceDatas.Last().insideBoat = false;

            boatSides.Add(bs);
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
        SpaceCount = boatSides[0].spaceDatas.Count;
    }

    private void OnValidate()
    {
        Instance = this;
        SpaceCount = boatSides[0].spaceDatas.Count;
    }

    #region Lane and Space Checks
    /// <summary> 
    /// Returns a true/false if the space exists within the lane
    /// </summary>
    public bool CheckAvailableSpace(int side, int space)
    {
        if (space > boatSides[side].spaceDatas.Count || space < 0) return false;
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
        if (targetSpace < spaces && targetSpace > -1) return boatSides[currentSide].spaceDatas[targetSpace];
        // Otherwise, just return the current space
        else return boatSides[currentSide].spaceDatas[currentSpace];
    }

    /// <summary>
    /// Obtain the space of the opposite side of the boat from a given side and space
    /// </summary>
    public BoatSide.SpaceData GetSpaceFromOppositeLane(int currentSide, int currentSpace)
    {
        int newSide = currentSide == 0 ? 1 : 0;
        return boatSides[newSide].spaceDatas[currentSpace];
    }

    /// <summary>
    /// Get the specific space from the given side of the boat
    /// </summary>
    public BoatSide.SpaceData GetSpace(int side, int space)
    {
        return boatSides[side].spaceDatas[space];
    }

    /// <summary>
    /// Get all spaces from the given side of the boat
    /// </summary>
    public List<BoatSide.SpaceData> GetSpaces(int side)
    {
        return boatSides[side].spaceDatas;
    }

    /// <summary>
    /// Gets the specific space from the given Side Follow Space of the boat
    /// </summary>
    public BoatSide.SpaceData GetSideSpace(int side, bool getLeftSide)
    {
        if (getLeftSide) return boatSides[side].spaceDatas.First();
        else return boatSides[side].spaceDatas.Last();
    }

    /// <summary>
    /// Gets the specific space from the spaces directly on the boat. Sides start at zero.
    /// </summary>
    public BoatSide.SpaceData GetBoatSpace(int side, int space)
    {
        print($"Getting Space: {side} and Space: {space}");
        print($"Space Datas =  {boatSides[side].spaceDatas.Count}.");
        if (space < 1) return boatSides[side].spaceDatas[1];
        if (space > SpaceCount - 2) return boatSides[side].spaceDatas[SpaceCount - 2];
        return boatSides[side].spaceDatas[space];
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
        if (boatSides == null || globalRiverValues == null)
        {
            Debug.LogWarning("Missing Global River Values or Boat Sides");
            return;
        }
        if (Application.isPlaying) return;
        
        UpdateSpaceDatas();

        for (int i = 0; i < boatSides.Count; i++)
        {
            BoatSide bs = boatSides[i];
            
            // Boat Spaces (not including first and last)
            int boatSpaceCount = bs.spaceDatas.Count - 2;
            float spacing = globalRiverValues.boatSpaceDistance;

            // float distance = i * 2f;

            // Boat Spaces
            for (int j = 1; j < bs.spaceDatas.Count - 1; j++)
            {
                int localIndex = j - 1;
                var data = bs.spaceDatas[j];

                float zOffset = (localIndex - (boatSpaceCount - 1) / 2f) * spacing;

                Vector3 pos = data.t.localPosition;
                // pos.z = 0f;
                pos.x = zOffset; // keep centered on the boat
                pos.y = .5f;

                data.t.localPosition = pos;
                data.insideBoat = true;
            }

            // Side Spaces
            float sideOffset = globalRiverValues.boatSideSpaceDistance;

            Transform first = bs.spaceDatas.First().t;
            first.localPosition = new Vector3(sideOffset, 0, bs.spaceDatas[^1].t.localPosition.z);
            bs.spaceDatas.Last().insideBoat = false;

            Transform last = bs.spaceDatas.Last().t;
            last.localPosition = new Vector3(-sideOffset, 0, last.localPosition.z);
            bs.spaceDatas.Last().insideBoat = false;
        }

    }

}