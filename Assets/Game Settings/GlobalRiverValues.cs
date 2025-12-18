using UnityEngine;
using EditorAttributes;

/// <summary>
/// Scriptable Object data representing the global values for the river.
/// </summary>
[CreateAssetMenu(fileName = "GlobalRiverValues", menuName = "ScriptableObjects/GameSettings/Global River Values")]
public class GlobalRiverValues : ScriptableObject
{
    /// <summary> Distance representing the width of each river lane. </summary>
    [Min(3.5f)] public float RiverLaneDistance = 7.5f;
    /// <summary> Number of lanes in the river. </summary>
    [Min(1)] public int RiverLaneCount = 3;
    /// <summary>  Distance representing the Side Space on the boat (Enemies will linger here when following the players boat) </summary>
    [Min(2.75f)] public float RiverBoatSideSpaceDistance = 4f;

    /// <summary>
    /// The instance of the Global River Values
    /// </summary>
    //public static GlobalRiverValues Instance;

    //private void Awake()
    //{
    //    Instance = this;
    //}

    //private void OnEnable()
    //{
    //    Instance = this;
    //}

    [Button]
    public void ResetValues()
    {
        RiverLaneDistance = 7.5f;
        RiverLaneCount = 3;
        RiverBoatSideSpaceDistance = 4f;
    }
}
