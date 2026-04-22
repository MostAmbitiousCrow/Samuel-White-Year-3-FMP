/// <summary> Class representing the global values for the sewer river. </summary>
public static class GlobalRiverValues
{
    #region ReadOnly
    /// <summary> Distance representing the width of each river lane. </summary>
    public static readonly float RiverLaneDistance = 8.5f;
    /// <summary> Number of lanes in the river. </summary>
    public static readonly int RiverLaneCount = 3;
    
    /// <summary>  Distance representing the Side Space on the boat (Enemies will linger here when following the players boat) </summary>
    public static readonly float BoatSideSpaceDistance = 5f;
    /// <summary>  Distance representing how much distance between each space the boat can move to </summary>
    public static readonly float BoatSpaceDistance = 2f;
    
    /// <summary> The max height objects can spawn in the sewer </summary>
    public static readonly float SewerHeight = 5f;
    
    /// <summary> The distance from any lane to the sewer river ceiling. Used for ceiling object placement. </summary>
    public static readonly float CeilingToLaneDistance = 18f;
    /// <summary> The distance from the outer lanes to the sewer river wall. Used for wall object placement. </summary>
    public static readonly float WallToLaneDistance = 9.5f;
    /// <summary> The distance from any lane to the sewer river floor. Used for floor object placement. </summary>
    public static readonly float FloorToLaneDistance = 0.5f;
    #endregion

    /*
    #region Not ReadOnly
    /// <summary> Distance representing the width of each river lane. </summary>
    public static float RiverLaneDistance = 8.5f;
    /// <summary> Number of lanes in the river. </summary>
    public static int RiverLaneCount = 3;
    
    /// <summary>  Distance representing the Side Space on the boat (Enemies will linger here when following the players boat) </summary>
    public static float BoatSideSpaceDistance = 5f;
    /// <summary>  Distance representing how much distance between each space the boat can move to </summary>
    public static float BoatSpaceDistance = 2f;
    
    /// <summary> The max height objects can spawn in the sewer </summary>
    public static float SewerHeight = 5f;
    
    /// <summary> The distance from any lane to the sewer river ceiling. Used for ceiling object placement. </summary>
    public static float CeilingToLaneDistance = 18f;
    /// <summary> The distance from the outer lanes to the sewer river wall. Used for wall object placement. </summary>
    public static float WallToLaneDistance = 9.5f;
    /// <summary> The distance from any lane to the sewer river floor. Used for floor object placement. </summary>
    public static float FloorToLaneDistance = 0.5f;
    
    public static void ResetValues()
    {
        RiverLaneDistance = 8.5f;
        RiverLaneCount = 3;
        
        BoatSideSpaceDistance = 6f;
        BoatSpaceDistance = 2f;
        
        SewerHeight = 5f;

        CeilingToLaneDistance = 18f;
        WallToLaneDistance = 9.5f;
        FloorToLaneDistance = 0.5f;
    }
    #endregion
    */


}
