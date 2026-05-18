using UnityEngine;
using UnityEngine.Serialization;
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
    [Tooltip("The name of the level")]
    public string levelName = "A Level";
    [Header("Environment")] 
    public SO_GameColours levelColours;
    [Tooltip("The spline that will be applied to the World Spline of which objects travel on")]
    public Spline levelSpline;
    [Tooltip("The level type that will affect the environment of the game")]
    public Environments environmentType;
    
    [Header("Section Data")]
    [Tooltip("The array of sections that will appear during this level. Sections load from first to last.")]
    public SO_SectionData[] sectionData;
    
    /// <summary>
    /// <para>The type this level will approach loading sections.</para>
    /// <para>Defined will spawn the sections provided in order.</para>
    /// <para>Randomised will randomise the sections provided.</para>
    /// </summary>
    public enum LevelType
    {
        Defined, Randomised
    }
    public LevelType levelType;
}
