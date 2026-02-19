using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// </summary>

/*
 * This is the scriptable object data that is inserted into the Game Section Manager in the main game scene.
 * It contains references to a Level Data Prefab that will change the sewer environment and contains Section data for
 * River Object spawning.
 *
 * Additionally, you can choose to change the colours of the game environment.
 * If left null, the colours won't be changed.
 */
[CreateAssetMenu (fileName = "Level Data",  menuName = "ScriptableObjects/Level Data", order = 1)]
public class SO_LevelData : ScriptableObject
{
    [Header("Environment")] 
    public SO_GameColours levelColours;
    
    [Header("Section Data")]
    [Tooltip("The Prefab container the section level data")]
    public GameObject sectionDataObject;
}
