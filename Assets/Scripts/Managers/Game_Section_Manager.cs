using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EditorAttributes;
using UnityEngine.Splines;

public class Game_Section_Manager : MonoBehaviour, IAffectedByRiver, ITargetsBoat
{
    [Header("Level Info")]
    [SerializeField] private GameLevels currentLevel = GameLevels.Sewer;
    
    // Temp
    public enum GameLevels
    {
        Sewer, Forest
    }
    
    [Header("Section Data")]
    public List<Section_Content.SectionData> sectionDatas;

    [Header("Section Info")]
    [Min(0)][SerializeField] private int currentSectionIndex = 0;

    #region Section Objects & Look-up
    [Header("Section Objects")]
    private readonly Dictionary<System.Enum, int> _prefabLookup = new Dictionary<System.Enum, int>();

    [Line(GUIColor.Cyan)]
    [FoldoutGroup("Obstacle Objects", nameof(trashObjectID), nameof(wideTrashObjectID),  nameof(pipeObjectID))]
    [SerializeField] private Void obstacleGroup;
    [SerializeField, HideProperty] private int trashObjectID, wideTrashObjectID,  pipeObjectID;
    
    [Line(GUIColor.Red)]
    [FoldoutGroup("Enemy Objects", nameof(crocodileObjectID), nameof(frogObjectID), nameof(batObject), nameof(tentacleObject))]
    [SerializeField] private Void enemyGroup;
    [SerializeField, HideProperty] private int crocodileObjectID, frogObjectID, batObject, tentacleObject;
    
    [Line(GUIColor.Yellow)]
    [FoldoutGroup("Collectible Objects", nameof(gemStoneObject), nameof(gemFragmentObject))]
    [SerializeField] private Void collectibleGroup;
    [SerializeField, HideProperty] private int gemStoneObject;
    [SerializeField, HideProperty] private int gemFragmentObject;
    
    [Line(GUIColor.White)]
    [SerializeField] private int gemStoneGateObjectID;

    // Tracked last object in segment
    [SerializeField, ReadOnly] private River_Object lastSpawnedObject;
    [SerializeField, ReadOnly] private float furthestDistance;
    #endregion

    #region Injection Dependencies
    [Header("Managers")]
    [SerializeField] private River_Manager riverManager;
    [SerializeField] private Boat_Space_Manager boatManager;

    public void InjectRiverManager(River_Manager manager) => riverManager = manager;
    public void InjectBoatSpaceManager(Boat_Space_Manager bsm) => boatManager = bsm;
    #endregion

    private void Awake()
    {
        InitializePrefabLookup();
    }

    private void OnEnable() => GameManager.GameLogic.onGameStarted += StartSpawning;
    private void OnDisable() => GameManager.GameLogic.onGameStarted -= StartSpawning;

    private void InitializePrefabLookup()
    {
        // Obstacles
        _prefabLookup.Add(Section_Obstacle_Object.ObstacleType.TrashPile, trashObjectID);
        _prefabLookup.Add(Section_Obstacle_Object.ObstacleType.WideTrashPile, wideTrashObjectID);
        _prefabLookup.Add(Section_Obstacle_Object.ObstacleType.SewerPipe, pipeObjectID);

        // Enemies
        _prefabLookup.Add(Section_Enemy_Object.EnemyType.Crocodile, crocodileObjectID);
        _prefabLookup.Add(Section_Enemy_Object.EnemyType.Frog, frogObjectID);
        _prefabLookup.Add(Section_Enemy_Object.EnemyType.Bat, batObject);
        _prefabLookup.Add(Section_Enemy_Object.EnemyType.Tentacle, tentacleObject);

        // Collectibles
        _prefabLookup.Add(Section_Collectible_Object.CollectibleType.Gemstone, gemStoneObject);
        _prefabLookup.Add(Section_Collectible_Object.CollectibleType.GemstoneFragment, gemFragmentObject);
        
        // Gemstone gates don't have an alt, skipping
    }

    [Button]
    public void GetSectionDatas()
    {
        sectionDatas.Clear();
        foreach (Transform child in transform)
        {
            if (child.TryGetComponent(out Section_Content content))
            {
                sectionDatas.Add(content.sectionData);
            }
        }
    }

    [Button]
    public void StartSpawning()
    {
        StopAllCoroutines();
        StartCoroutine(SpawnSectionRoutine());
    }

    private IEnumerator SpawnSectionRoutine()
    {
        // Spawn Sections
        while (currentSectionIndex < sectionDatas.Count)
        {
            Section_Content.SectionData data = sectionDatas[currentSectionIndex];

            if (data == null)
            {
                Debug.LogWarning($"Section Data at index {currentSectionIndex} is null. Skipping.");
                currentSectionIndex++;
                continue;
            }
            
            lastSpawnedObject = null;
            furthestDistance = 0;
            
            // Initial delay
            if (data.initialDelay > 0) yield return new WaitForSeconds(data.initialDelay);
            
            // Spawn objects
            SpawnObjects(data.ObstacleDatas);
            SpawnObjects(data.EnemyDatas);
            SpawnObjects(data.CollectibleDatas);
            SpawnGates(data.GemstoneGateDatas);
            
            // Wait until the specific object that was last spawned is disabled (returned to pool) //TODO: Currently broken???
            if (lastSpawnedObject != null) 
                yield return new WaitUntil(() => !lastSpawnedObject.gameObject.activeSelf);
            else Debug.LogWarning($"Section {currentSectionIndex} had no objects.");

            // Delay
            if (data.postDelay > 0) yield return new WaitForSeconds(data.postDelay);
            
            currentSectionIndex++;
        }

        Debug.Log("All Sections Spawned and Completed.");
    }

    #region Spawning Logic

    // Spawn Objects into the River Lane
    private void SpawnObjects<T>(List<T> list) where T : Section_Builder_Object
    {
        if (list == null) return;

        foreach (var item in list)
        {
            if (!item) continue;
            
            // Determine the Enum type dynamically to find the prefab
            System.Enum typeKey = GetTypeKey(item);
            if (typeKey != null && _prefabLookup.TryGetValue(typeKey, out int id)) 
                SpawnAndInitialize(id, item);
        }
    }

    // Method for spawning gates, since they don't have an enum check...
    private void SpawnGates(List<Section_Gemstone_Gate> list)
    {
        if (list == null) return;
        foreach (var item in list.Where(item => item)) // Thanks Rider. Again.
            SpawnAndInitialize(gemStoneGateObjectID, item);
    }

    private void SpawnAndInitialize(int id, Section_Builder_Object data)
    {
        // Get the object from the object pool, with a given ID
        var pooledObject = ObjectPoolManager.Instance.Spawn<River_Object>(id);
        
        if (!pooledObject) return;

        // Assign Pipe Data
        if (pooledObject is Pipe_Obstacle pipe && data is Section_Obstacle_Object obstacleData)
        {
            pipe.OverridePipeData(obstacleData.sectionData.pipeObstacleData);
        }

        // Override object data
        if (data)
        {
            Debug.Log($"Spawning: {pooledObject}, {data}");
            switch (pooledObject)
            {
                case River_Obstacle ro:
                    ro.OverrideData(data.GetComponent<Section_Obstacle_Object>().sectionData.overridedData); break;
                case River_Enemy re:
                    re.OverrideStats(data.GetComponent<Section_Enemy_Object>().sectionData.overridedData); break;
                case River_Collectible rc:
                    rc.OverrideData(data.GetComponent<Section_Collectible_Object>().sectionData.overridedData); break;
                case Gemstone_Gate gg:
                    gg.OverrideData(data.GetComponent<Section_Gemstone_Gate>().sectionData.overridedData); break;
            }
        }

        // Place the section object in the river lane!
        PlaceSectionObject(pooledObject, data);
    }

    // Get the specific type from this object type
    private System.Enum GetTypeKey(Section_Builder_Object item)
    {
        return item switch
        {
            Section_Obstacle_Object ob => ob.sectionData.obstacleType,
            Section_Enemy_Object en => en.sectionData.enemyType,
            Section_Collectible_Object co => co.sectionData.collectibleType,
            _ => null
        };
    }

    private void PlaceSectionObject(River_Object ro, Section_Builder_Object sbo)
    {
        ro.canMove = true;
        ro.isMoving = true;

        float spawnDist = sbo.Distance;
        
        // Store the distance and object, as to detect the furthest object in this section
        if (spawnDist > furthestDistance)
        {
            lastSpawnedObject = ro;
            furthestDistance = spawnDist;
        }
        
        ro.StartOnLane(sbo.Lane, sbo.Distance + riverManager.RiverObjectSpawnDistance, sbo.Height);
    }

    #endregion
}