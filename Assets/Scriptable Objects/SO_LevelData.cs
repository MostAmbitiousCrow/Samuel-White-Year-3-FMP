using UnityEngine;
using UnityEngine.Splines;

/// <summary>
/// 
/// </summary>

/*
 * 
 */
[CreateAssetMenu (fileName = "Level Data",  menuName = "ScriptableObjects/Level Data", order = 0)]
public class SO_LevelData : ScriptableObject
{
    //TODO: Write summaries
    [Header("Level Name")]
    public string levelName = "A Level";
    [Header("Environment")] 
    public SO_GameColours levelColours;
    public Spline levelSpline;
    
    [Header("Section Data")]
    public SO_SectionData[] sectionData;
}
