using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using EditorAttributes;
using UnityEditor;
using UnityEngine.Serialization;
using UnityEngine.Splines;

/// <summary> Class used for the level builder tool to generate Sections stored in the Level Data Scriptable Objects </summary>
public class SectionContentBuilder : MonoBehaviour, IAffectedByRiver
{
    [Header("Data")]
    [SerializeField] private River_Manager riverManager;

    [Serializable]
    public class SectionData
    {
        public float initialDelay = 0f;
        public float postDelay = 0f;

        public int ObjectCount => SectionBuilderDatas.Count;
        public List<ISectionData> SectionBuilderDatas = new();

        [FormerlySerializedAs("ObstacleDatas")] [Line(GUIColor.Cyan)]
        public List<Section_Obstacle_Object> obstacleDatas = new();
        [FormerlySerializedAs("EnemyDatas")] [Line(GUIColor.Red)]
        public List<Section_Enemy_Object> enemyDatas = new();
        [FormerlySerializedAs("CollectibleDatas")] [Line(GUIColor.Yellow)]
        public List<Section_Collectible_Object> collectibleDatas = new();

        [FormerlySerializedAs("GemstoneGateDatas")] [Line(GUIColor.White)]
        public List<Section_Gemstone_Gate> gemstoneGateDatas = new();
    }
    public SectionData sectionData = new();
    // public SplineContainer splineContainer;

    // [EditorAttributes.Button]
    public void GetSectionObjects()
    {
        sectionData.SectionBuilderDatas.Clear();

        sectionData.obstacleDatas.Clear();
        sectionData.enemyDatas.Clear();
        sectionData.collectibleDatas.Clear();
        sectionData.gemstoneGateDatas.Clear();

        foreach (var sbo in GetComponentsInChildren<SectionBuilderObject>())
        {
            sbo.Register(this);
            if (!riverManager)
            {
                Debug.LogWarning("No River Manager found, attempting to find reference...");
                riverManager = FindFirstObjectByType<River_Manager>();
                if (riverManager == null)
                {
                    Debug.LogWarning("No River Manager found!");
                    return;
                }
                Debug.Log("River Manager found!");
            }
            sbo.InjectRiverManager(riverManager);
        }

        // var spline = GetComponentInChildren<SplineContainer>();
        // splineContainer = spline;
    }

    public void InjectRiverManager(River_Manager manager)
    {
        riverManager = manager;
    }

    [Header("Data Asset Creation")]
    [SerializeField] private string pathName = "Assets/Scriptable Objects/Level Data/";
    [SerializeField] private string assetName = "Level Section Data";

    [Button]
    public void CreateSectionDataAsset()
    {
        CreateSectionDataAsset(pathName, assetName);
    }

    public SO_SectionData CreateSectionDataAsset(string paraPathName, string paraAssetName)
    {
        SO_SectionData scriptableObject = ScriptableObject.CreateInstance<SO_SectionData>();

        // Assign default / preconfigured values
        var content = scriptableObject.sectionContent;
        
        // Assign Enemy Data to content
        var enemyData = sectionData.enemyDatas.Select
            (enemy => new SO_SectionData.SectionContent.SectionEnemyData
            {
                data = enemy.sectionData, 
                distance = enemy.Distance, height = enemy.Height, lane = enemy.Lane
            }).ToList();
        content.enemies = enemyData;
        
        // Assign Obstacle Data to content
        var obstacleData = sectionData.obstacleDatas.Select(
            obstacle => new SO_SectionData.SectionContent.SectionObstacleData
            {
                data = obstacle.sectionData, 
                distance = obstacle.Distance, height = obstacle.Height, lane = obstacle.Lane
            }).ToList();
        content.obstacles = obstacleData;
        
        // Assign Collectible Data to content
        var collectibleData = sectionData.collectibleDatas.Select
            (collectible => new SO_SectionData.SectionContent.SectionCollectibleData
            {
                data = collectible.sectionData, 
                distance = collectible.Distance, height = collectible.Height, lane = collectible.Lane
            }).ToList();
        content.collectibles = collectibleData;
        
        // Assign Gemstone Gate Data to content
        var gemstoneGateData = sectionData.gemstoneGateDatas.Select
            (gate => new SO_SectionData.SectionContent.SectionGemstoneGateData
            {
                data = gate.sectionData, 
                distance = gate.Distance, height = gate.Height, lane = gate.Lane
            }).ToList();
        content.gemstoneGates = gemstoneGateData;
        
        // Assign the section content to the Level Data Scriptable Object
        scriptableObject.sectionContent = content;

        var path = $"{paraPathName}{paraAssetName}.asset";

        AssetDatabase.CreateAsset(scriptableObject, path);
        EditorUtility.SetDirty(scriptableObject);
        AssetDatabase.SaveAssets();

        Debug.Log($"Saved {scriptableObject} to {paraAssetName}");
        return scriptableObject;
    }

    [Header("Debug")]
    [SerializeField] private bool enableDebug;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        GetSectionObjects();

        if (!enableDebug) return;

        foreach (var item in sectionData.obstacleDatas) item.DrawGizmos();
        foreach (var item in sectionData.enemyDatas) item.DrawGizmos();
        foreach (var item in sectionData.collectibleDatas) item.DrawGizmos();
        foreach (var item in sectionData.gemstoneGateDatas) item.DrawGizmos();
    }
#endif
}
