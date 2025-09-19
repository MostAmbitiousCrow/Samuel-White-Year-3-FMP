using UnityEngine;

[CreateAssetMenu(fileName = "New River Lane Data", menuName = "ScriptableObjects/River Lane Data")]
/// <summary>
/// The data referring to the games lane-based movement feature
/// </summary>
public class SO_RiverLaneData : ScriptableObject
{
    /// <summary>
    /// The amount of lanes river objects and the players boat can move to
    /// </summary>
    [Tooltip("The amount of lanes river objects and the players boat can move to")]
    [Range(0, 10)] public int LaneCount = 3;
    /// <summary>
    /// The distance, in meters, between each lane
    /// </summary>
    [Tooltip("The distance, in meters, between each lane")]
    [Range(0f, 10)] public float LaneDistance = 1f;
}
