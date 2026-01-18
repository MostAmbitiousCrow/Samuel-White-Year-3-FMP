using UnityEngine;
using EditorAttributes;
using UnityEngine.Serialization;

/// <summary>
/// Scriptable Object data representing the global values for the river.
/// </summary>
[CreateAssetMenu(fileName = "GlobalRiverValues", menuName = "ScriptableObjects/GameSettings/Global River Values")]
public class GlobalRiverValues : ScriptableObject
{
    [Header("River Values")]
    /// <summary> Distance representing the width of each river lane. </summary>
    [Min(3.5f)] public float riverLaneDistance = 7.5f;
    /// <summary> Number of lanes in the river. </summary>
    [Min(1)] public int riverLaneCount = 3;
    
    [Header("Boat Values")]
    /// <summary>  Distance representing the Side Space on the boat (Enemies will linger here when following the players boat) </summary>
    [Min(2.75f)] public float boatSideSpaceDistance = 4f;
    [Min(1.5f)] public float boatSpaceDistance = 2f;

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
        riverLaneDistance = 7.5f;
        riverLaneCount = 3;
        
        boatSideSpaceDistance = 4f;
        boatSpaceDistance = 2f;
    }
}
