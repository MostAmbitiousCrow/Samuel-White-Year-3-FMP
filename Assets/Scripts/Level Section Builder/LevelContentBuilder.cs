using System;
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
    [SerializeField] private Environments environmentType;
    
    [Header("Data Asset Creation")]
    [SerializeField, TextArea] private string pathName = "Assets/Scriptable Objects/Level Data/";
    [SerializeField] private string assetName = "Level Data";

    private void OnValidate()
    {
        sections = GetComponentsInChildren<SectionContentBuilder>(false);

        if (TryGetComponent(out SplineContainer container)) splineContainer = container;
        
        var sc = GetComponentInChildren<SplineContainer>();
        if (sc) splineContainer = sc;
        
        // Naming:
        string splined = sc ? "Splined" : "";
        name = new string($"{environmentType} Level Content Builder ({sections.Length}) {splined}");

        foreach (var section in sections)
        {
            section.currentDistance = 0f;
        }
    }

    [Button]
    public void CreateSectionDataAsset()
    {
        SO_LevelData scriptableObject = ScriptableObject.CreateInstance<SO_LevelData>();
        
        // Assign the level content to the Level Data Scriptable Object

        scriptableObject.levelColours = levelColours;
        scriptableObject.levelSpline = splineContainer.Spline;
        scriptableObject.environmentType = environmentType;

        // LINQ to sort all section data into an array
        var id = 0;
        scriptableObject.sectionData = sections.Select(section =>
        {
            id++;
            return section.CreateSectionDataAsset(pathName, $"Level_{environmentType}_Section_{assetName}({id})");
        }).ToArray();

        // Create the asset
        scriptableObject.name = assetName;
        var path = $"{pathName}/Level_{environmentType}_{assetName}.asset";

        # if UNITY_EDITOR
        AssetDatabase.CreateAsset(scriptableObject, path);
        EditorUtility.SetDirty(scriptableObject);
        AssetDatabase.SaveAssets();
        #endif

        Debug.Log($"Saved {scriptableObject} to {pathName}");
    }
    
    # if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (sections.Length <= 0) return;
        
        var previousSpeed = 10;
        foreach (var section in sections)
        {
            if (section.sectionData.slipStreamDatas.Count > 0)
            {
                // Get the last slip stream speed value
                var increaseAmount = section.sectionData.slipStreamDatas[^1].sectionData.overridedData.speedIncreaseAmount;
                
                if (increaseAmount >= previousSpeed)
                {
                    section.speed = increaseAmount;
                    previousSpeed = increaseAmount;
                }
            }
            else
            {
                section.defaultSpeed = previousSpeed;
            }
        }
    }
    #endif
}