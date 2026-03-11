using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using EditorAttributes;
using UnityEditor;

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

        [Line(GUIColor.Cyan)]
        public List<Section_Obstacle_Object> ObstacleDatas = new();
        [Line(GUIColor.Red)]
        public List<Section_Enemy_Object> EnemyDatas = new();
        [Line(GUIColor.Yellow)]
        public List<Section_Collectible_Object> CollectibleDatas = new();

        [Line(GUIColor.White)]
        public List<Section_Gemstone_Gate> GemstoneGateDatas = new();
    }
    public SectionData sectionData = new();

    // [EditorAttributes.Button]
    public void GetSectionObjects()
    {
        sectionData.SectionBuilderDatas.Clear();

        sectionData.ObstacleDatas.Clear();
        sectionData.EnemyDatas.Clear();
        sectionData.CollectibleDatas.Clear();
        sectionData.GemstoneGateDatas.Clear();

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
        SO_SectionData scriptableObject = ScriptableObject.CreateInstance<SO_SectionData>();

        // Assign default / preconfigured values
        var content = scriptableObject.sectionContent;
        
        // Assign Enemy Data to content
        var enemyData = sectionData.EnemyDatas.Select
            (enemy => new SO_SectionData.SectionContent.SectionEnemyData
            {
                data = enemy.sectionData, 
                distance = enemy.Distance, height = enemy.Height, lane = enemy.Lane
            }).ToList();
        content.enemies = enemyData;
        
        // Assign Obstacle Data to content
        var obstacleData = sectionData.ObstacleDatas.Select(
            obstacle => new SO_SectionData.SectionContent.SectionObstacleData
            {
                data = obstacle.sectionData, 
                distance = obstacle.Distance, height = obstacle.Height, lane = obstacle.Lane
            }).ToList();
        content.obstacles = obstacleData;
        
        // Assign Collectible Data to content
        var collectibleData = sectionData.CollectibleDatas.Select
            (collectible => new SO_SectionData.SectionContent.SectionCollectibleData
            {
                data = collectible.sectionData, 
                distance = collectible.Distance, height = collectible.Height, lane = collectible.Lane
            }).ToList();
        content.collectibles = collectibleData;
        
        // Assign Gemstone Gate Data to content
        var gemstoneGateData = sectionData.GemstoneGateDatas.Select
            (gate => new SO_SectionData.SectionContent.SectionGemstoneGateData
            {
                data = gate.sectionData, 
                distance = gate.Distance, height = gate.Height, lane = gate.Lane
            }).ToList();
        content.gemstoneGates = gemstoneGateData;
        
        // Assign the section content to the Level Data Scriptable Object
        scriptableObject.sectionContent = content;

        var path = $"{pathName}{assetName}.asset";

        AssetDatabase.CreateAsset(scriptableObject, path);
        EditorUtility.SetDirty(scriptableObject);
        AssetDatabase.SaveAssets();

        Debug.Log($"Saved {scriptableObject} to {pathName}");
    }

    [Header("Debug")]
    [SerializeField] private bool enableDebug;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        GetSectionObjects();

        if (!enableDebug) return;

        foreach (var item in sectionData.ObstacleDatas) item.DrawGizmos();
        foreach (var item in sectionData.EnemyDatas) item.DrawGizmos();
        foreach (var item in sectionData.CollectibleDatas) item.DrawGizmos();
        foreach (var item in sectionData.GemstoneGateDatas) item.DrawGizmos();
    }
#endif
}
