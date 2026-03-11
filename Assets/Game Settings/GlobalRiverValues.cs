using UnityEngine;
using EditorAttributes;

/*
/// <summary> Scriptable Object data representing the global values for the river. </summary>
[CreateAssetMenu(fileName = "GlobalRiverValues", menuName = "ScriptableObjects/GameSettings/Global River Values")]
public class GlobalRiverValues : ScriptableObject
{
    /// <summary> Distance representing the width of each river lane. </summary>
    [Header("River Values")]
    [Min(3.5f)] public float riverLaneDistance = 8.5f;
    /// <summary> Number of lanes in the river. </summary>
    [Min(1)] public int riverLaneCount = 3;
    
    /// <summary>  Distance representing the Side Space on the boat (Enemies will linger here when following the players boat) </summary>
    [Header("Boat Values")]
    [Min(2.75f)] public float boatSideSpaceDistance = 6f;
    /// <summary>  Distance representing how much distance between each space the boat can move to </summary>
    [Min(1.5f)] public float boatSpaceDistance = 2f;

    /// <summary> The instance of the Global River Values </summary>
    public static GlobalRiverValues Instance;

    private void Awake()
    {
        Instance = this;
    }

    [Button]
    public void ResetValues()
    {
        riverLaneDistance = 7.5f;
        riverLaneCount = 3;
        
        boatSideSpaceDistance = 4f;
        boatSpaceDistance = 2f;
    }
}
*/

/// <summary> Scriptable Object data representing the global values for the river. </summary>
public static class GlobalRiverValues
{
    /// <summary> Distance representing the width of each river lane. </summary>
    public static float RiverLaneDistance = 8.5f;
    /// <summary> Number of lanes in the river. </summary>
    public static int RiverLaneCount = 3;
    
    /// <summary>  Distance representing the Side Space on the boat (Enemies will linger here when following the players boat) </summary>
    public static float BoatSideSpaceDistance = 5f;
    /// <summary>  Distance representing how much distance between each space the boat can move to </summary>
    public static float BoatSpaceDistance = 2f;

    public static void ResetValues()
    {
        RiverLaneDistance = 8.5f;
        RiverLaneCount = 3;
        
        BoatSideSpaceDistance = 6f;
        BoatSpaceDistance = 2f;
    }
}
