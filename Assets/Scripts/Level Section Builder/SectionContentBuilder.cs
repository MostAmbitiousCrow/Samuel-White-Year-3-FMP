using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
        /// <summary> The overall distance of this section </summary>
        public int sectionDistance = 100;
        /// <summary> The distance added before this lane is spawned </summary>
        public int distanceBeforeSection;
        /// <summary> The distance added after this lane is spawned </summary>
        public int distanceAfterSection;

        public int ObjectCount => SectionBuilderDatas.Count;
        public List<ISectionData> SectionBuilderDatas = new();

        [Line(GUIColor.Cyan)]
        public List<Section_Obstacle_Object> obstacleDatas = new();
        [Line(GUIColor.Red)]
        public List<Section_Enemy_Object> enemyDatas = new();
        [Line(GUIColor.Yellow)]
        public List<Section_Collectible_Object> collectibleDatas = new();

        [Line(GUIColor.Cyan)]
        public List<Section_Gemstone_Gate> gemstoneGateDatas = new();
        
        [Line(GUIColor.White)]
        public List<Section_SlipStream_Object> slipStreamDatas = new();
        
        /// <summary> Determines the difficulty is considered to be for this section </summary>
        [Space]
        
        public DifficultyQualification difficultyType;
        
        public SO_SectionData.SectionContent.AvailableEnvironments applicableEnvironments;
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
        sectionData.slipStreamDatas.Clear();

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
    [SerializeField, TextArea] private string pathName = "Assets/Scriptable Objects/Level Data/";
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
        
        // Assign Slip Stream Data to content
        var slipStreamData = sectionData.slipStreamDatas.Select
        (stream => new SO_SectionData.SectionContent.SectionSlipStreamData()
        {
            data = stream.sectionData, 
            distance = stream.Distance, height = stream.Height, lane = stream.Lane
        }).ToList();
        content.slipStreams = slipStreamData;
        
        // Assign Difficulties and Environments
        content.difficultyType = sectionData.difficultyType;
        content.applicableEnvironments = sectionData.applicableEnvironments;
        
        // Assign Distances
        content.distanceBeforeSection = sectionData.distanceBeforeSection;
        content.distanceAfterSection = sectionData.distanceAfterSection;
        content.sectionDistance = sectionData.sectionDistance;
        
        // Assign the section content to the Level Data Scriptable Object
        scriptableObject.sectionContent = content;

        // Naming
        var environment = (int)content.applicableEnvironments == -1 ?
                new string("Everything") : content.applicableEnvironments.ToString();
        var difficulty = (int)content.difficultyType == -1 ?
            new string("Everything") : content.difficultyType.ToString();
        
        paraAssetName = ($"({environment}) ({difficulty}) {paraAssetName}");

        var path = $"{paraPathName}/{paraAssetName}.asset";

        #if UNITY_EDITOR
        AssetDatabase.CreateAsset(scriptableObject, path);
        EditorUtility.SetDirty(scriptableObject);
        AssetDatabase.SaveAssets();
        #endif

        Debug.Log($"Saved {scriptableObject} to {paraAssetName}");
        return scriptableObject;
    }

    [Header("Debug")]
    [SerializeField] private bool enableDebug;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        GetSectionObjects();

        var environment = (int)sectionData.applicableEnvironments == -1 ?
            new string("All Environments") : sectionData.applicableEnvironments.ToString();
        var difficulty = (int)sectionData.difficultyType == -1 ?
            new string("All Difficulties") : sectionData.difficultyType.ToString();
        
        name = $"- Section Builder | {difficulty} | {environment} | ({assetName})";

        if (!enableDebug) return;

        foreach (var item in sectionData.obstacleDatas) item.DrawGizmos();
        foreach (var item in sectionData.enemyDatas) item.DrawGizmos();
        foreach (var item in sectionData.collectibleDatas) item.DrawGizmos();
        foreach (var item in sectionData.gemstoneGateDatas) item.DrawGizmos();
        foreach (var item in sectionData.slipStreamDatas) item.DrawGizmos();

        // var label = string.Format(assetName);
        Handles.Label(transform.position + (Vector3.up * 5), $"'{assetName}'\n{difficulty} | {environment}");
        
        // Draw Section Distance
        Debug.DrawLine(transform.position + (Vector3.back * sectionData.distanceBeforeSection),
            transform.position + (Vector3.forward * (sectionData.sectionDistance + sectionData.distanceAfterSection)),
            Color.red);
        
        VisualisePlayerProgress();
    }

    [Header("Debug")]
    [SerializeField] private Mesh playerVisual;
    public float currentDistance;
    private Vector3 _playerPosition;
    private const int CurrentLane = 1;
    public float speed;
    public float defaultSpeed = 10;

    private void VisualisePlayerProgress()
    {
        if (!playerVisual) return;

        if (sectionData.slipStreamDatas.Count > 0)
        {
            Section_SlipStream_Object slipStream = null;
            
            foreach (var stream in sectionData.slipStreamDatas)
            {
                if (stream.Distance > currentDistance) continue;
                slipStream = stream;
            }

            // Set the boat speed to the speed of the slipstream
            if (slipStream) speed = slipStream.sectionData.overridedData.speedIncreaseAmount;
        }
        else speed = defaultSpeed;
        
        // Draw player mesh down the lane
        currentDistance += Time.deltaTime * (speed / 2f);

        if (currentDistance > sectionData.sectionDistance + sectionData.distanceAfterSection)
        {
            currentDistance = -sectionData.distanceBeforeSection;
            speed = defaultSpeed;
        }
        
        // riverManager.AssignToCurveSection(currentDistance, CurrentLane, out Vector3 pos, out Quaternion rot);

        // pos += transform.position + transform.right * (CurrentLane - 1) * GlobalRiverValues.RiverLaneDistance / 16f; //TODO?: Assign this to AssignToCurveSection
        _playerPosition = (Vector3.forward * currentDistance) + transform.position; //transform.position + transform.right * (CurrentLane - 1) * GlobalRiverValues.RiverLaneDistance / 16f;
        // transform.localPosition = _playerPosition + transform.parent.position;
        
        Gizmos.color = Color.green;
        // Gizmos.DrawMesh(playerVisual, pos, rot, Vector3.one * 4f);
        Gizmos.DrawMesh(playerVisual, _playerPosition, Quaternion.identity, Vector3.one * 4f);
    }
#endif
}
