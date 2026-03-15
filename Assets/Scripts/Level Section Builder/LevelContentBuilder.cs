using System.Collections.Generic;
using System.Linq;
using EditorAttributes;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

public class LevelContentBuilder : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private SectionContentBuilder[] sections;
    [SerializeField] private SplineContainer splineContainer;
    [SerializeField] private SO_GameColours levelColours;
    
    [Header("Data Asset Creation")]
    [SerializeField] private string pathName = "Assets/Scriptable Objects/Level Data/";
    [SerializeField] private string assetName = "Level Data";

    private void OnValidate()
    {
        sections = GetComponentsInChildren<SectionContentBuilder>(false);

        if (TryGetComponent(out SplineContainer container)) splineContainer = container;
        
        var sc = GetComponentInChildren<SplineContainer>();
        if (sc) splineContainer = sc;
    }

    [Button]
    public void CreateSectionDataAsset()
    {
        SO_LevelData scriptableObject = ScriptableObject.CreateInstance<SO_LevelData>();
        
        // Assign the level content to the Level Data Scriptable Object

        scriptableObject.levelColours = levelColours;
        scriptableObject.levelSpline = splineContainer.Spline;

        // LINQ to sort all section data into an array
        scriptableObject.sectionData = sections.Select(section => 
            section.CreateSectionDataAsset(pathName, assetName + " Section")).ToArray();

        // Create the asset
        scriptableObject.name = assetName;
        var path = $"{pathName}{assetName}.asset";

        AssetDatabase.CreateAsset(scriptableObject, path);
        EditorUtility.SetDirty(scriptableObject);
        AssetDatabase.SaveAssets();

        Debug.Log($"Saved {scriptableObject} to {pathName}");
    }
}
