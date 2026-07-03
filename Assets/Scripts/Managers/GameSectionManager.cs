using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EditorAttributes;
using Void = EditorAttributes.Void;

public class LevelSectionManager : MonoBehaviour, IAffectedByRiver, ITargetsBoat
{
    public delegate void SectionLoaded();
    public static SectionLoaded OnSectionLoaded;

    #region Section Objects & Look-up
    [Header("Level Data")]
    [SerializeField] private SO_LevelData currentLevelData;
    private SO_SectionData[] _sectionData;
    
    [Header("Section Info")]
    [Min(0)][SerializeField] private int currentSectionIndex = 0;
    
    [Header("Spacing Settings")]
    [Tooltip("The distance between the end of each section")]
    [SerializeField] private float gapBetweenSections = 20f;
    private float _currentSectionOffset;
    
    [Header("Section Objects")]
    private readonly Dictionary<Enum, int> _prefabLookup = new Dictionary<System.Enum, int>();

    [Line(GUIColor.Cyan)]
    [FoldoutGroup("Obstacle Objects", nameof(trashObjectID), nameof(wideTrashObjectID), nameof(pipeObjectID), nameof(bridgeObjectID))]
    [SerializeField] private Void obstacleGroup;
    [SerializeField, HideProperty] private int trashObjectID, wideTrashObjectID, pipeObjectID, bridgeObjectID;
    
    [Line(GUIColor.Red)]
    [FoldoutGroup("Enemy Objects", nameof(crocodileObjectID), nameof(frogObjectID), nameof(batObjectID), nameof(tentacleObjectID))]
    [SerializeField] private Void enemyGroup;

    [SerializeField, HideProperty] private int crocodileObjectID, frogObjectID, batObjectID, tentacleObjectID;

    [Line(GUIColor.Yellow)]
    [FoldoutGroup("Collectible Objects", nameof(gemStoneObjectID), nameof(gemFragmentObjectID))]
    [SerializeField] private Void collectibleGroup;
    [SerializeField, HideProperty] private int gemStoneObjectID = 7;
    [SerializeField, HideProperty] private int gemFragmentObjectID;
     
    [Line(GUIColor.White)]
    [SerializeField] private int gemStoneGateObjectID= 8;
    [SerializeField] private int slipStreamObjectID = 9;

    // Tracked last object in segment
    [SerializeField, ReadOnly] private River_Object lastSpawnedObject;
    [SerializeField, ReadOnly] private float furthestDistance;
    #endregion

    #region Injection Dependencies
    [Header("Managers")]
    [SerializeField] private River_Manager riverManager;
    [SerializeField] private Boat_Space_Manager boatManager;
    [SerializeField] private Boat_Controller boatController;
    [SerializeField] private GameLevelManager gameLevelManager;

    public void InjectRiverManager(River_Manager manager) => riverManager = manager;
    public void InjectBoatSpaceManager(Boat_Space_Manager bsm) => boatManager = bsm;
    #endregion

    #region  Initialisation

        private void Awake()
        {
            if (!riverManager) riverManager = FindFirstObjectByType<River_Manager>();
            if (!boatManager) boatManager = FindFirstObjectByType<Boat_Space_Manager>();
            if (!boatController) boatController = FindFirstObjectByType<Boat_Controller>();
            if (!gameLevelManager) gameLevelManager =  FindFirstObjectByType<GameLevelManager>();
            
            InitializePrefabLookup();
        }

        private void OnEnable()
        {
            GameLevelManager.OnLevelLoaded += StartSpawning;
            GameManager.MainGameLogic.OnGameOver += StopSpawning;
        }

        private void OnDisable()
        {
            GameLevelManager.OnLevelLoaded -= StartSpawning;
            GameManager.MainGameLogic.OnGameOver -= StopSpawning;
        }
    
        private void InitializePrefabLookup()
        {
            // Obstacles
            _prefabLookup.Add(Section_Obstacle_Object.ObstacleType.TrashPile, trashObjectID);
            _prefabLookup.Add(Section_Obstacle_Object.ObstacleType.WideTrashPile, wideTrashObjectID);
            _prefabLookup.Add(Section_Obstacle_Object.ObstacleType.SewerPipe, pipeObjectID);
    
            // Enemies
            _prefabLookup.Add(Section_Enemy_Object.EnemyType.Crocodile, crocodileObjectID);
            _prefabLookup.Add(Section_Enemy_Object.EnemyType.Frog, frogObjectID);
            _prefabLookup.Add(Section_Enemy_Object.EnemyType.Bat, batObjectID);
            _prefabLookup.Add(Section_Enemy_Object.EnemyType.Tentacle, tentacleObjectID);
    
            // Collectibles
            _prefabLookup.Add(Section_Collectible_Object.CollectibleType.Gemstone, gemStoneObjectID);
            _prefabLookup.Add(Section_Collectible_Object.CollectibleType.GemstoneFragment, gemFragmentObjectID);
            
            // Gemstone gates doesn't have an alt, skipping
        }

    #endregion

    public void AssignNewLevelData(SO_LevelData data)
    {
        currentLevelData = data;
        _sectionData = data.sectionData;
        
        // Randomise Sections if applicable
        if (currentLevelData.levelType == SO_LevelData.LevelType.Randomised)
            _sectionData = _sectionData.OrderBy(x => new System.Random().Next()).ToArray();
    }

    private Coroutine _spawnRoutine;
    public void StartSpawning()
    {
        // Debug.Log("Starting Spawning Objects");
        if (_spawnRoutine != null) StopCoroutine(_spawnRoutine);
        _spawnRoutine = StartCoroutine(SpawnSectionRoutine());
    }

    public void StopSpawning(GameManager.MainGameLogic.GameOverType gameOverType)
    {
        if (_spawnRoutine != null)
        {
            StopCoroutine(_spawnRoutine);
            Debug.Log("Stopping Spawning Objects");
            return;
        }
        
        Debug.Log("No Routine to Stop");
    }

    private IEnumerator SpawnSectionRoutine()
    {
        int sectionLength = currentLevelData.sectionData.Length;
        _currentSectionOffset = 0f;

        // Wait a frame to allow the boat to reset first
        yield return null;
        
        // Spawn Sections
        while (currentSectionIndex < sectionLength)
        {
            var data = _sectionData[currentSectionIndex];

            if (!data)
            {
                // Debug.LogWarning($"Section Data at index {currentSectionIndex} is null. Skipping.");
                currentSectionIndex++;
                continue;
            }
            
            lastSpawnedObject = null;
            furthestDistance = 0;

            _currentSectionOffset += data.sectionContent.distanceBeforeSection;
            
            // Initial delay
            // if (data.sectionContent.initialDelay > 0) yield return new WaitForSeconds(data.sectionContent.initialDelay);
            
            // Spawn objects
            SpawnObstacles(data.sectionContent.obstacles);
            SpawnEnemies(data.sectionContent.enemies);
            SpawnCollectibles(data.sectionContent.collectibles);
            SpawnGemstoneGates(data.sectionContent.gemstoneGates);
            SpawnSlipStreams(data.sectionContent.slipStreams);
            
            // Debug.Log($"Last Spawned Object is: {lastSpawnedObject}");
            
            // if (lastSpawnedObject) yield return new WaitUntil(() => !lastSpawnedObject.gameObject.activeSelf);
            // else Debug.Log($"Section {currentSectionIndex} had no objects.");

            // Delay
            // if (data.sectionContent.postDelay > 0) yield return new WaitForSeconds(data.sectionContent.postDelay);
            
            // Debug.Log($"Spawned Section {currentSectionIndex}.");
            
            _currentSectionOffset += (data.sectionContent.sectionDistance 
                                      + gapBetweenSections + data.sectionContent.distanceAfterSection);
            yield return currentSectionIndex++;
        }

        // End once the players boat has progressed past the last spawned river object
        if (lastSpawnedObject) yield return new WaitUntil(() 
            => boatController.RiverSplineObject.DistanceOnSpline > lastSpawnedObject.StartDistance + 20f);
        else Debug.Log($"Section {currentSectionIndex} had no objects.");
        
        Debug.Log("All Sections Spawned and Completed. Triggering Next Level Load");
        currentSectionIndex = 0;
        gameLevelManager.LoadNextLevel();
    }

    #region Spawning Logic
    
    /// <summary> Method for spawning Obstacles and their respective types </summary>
    private void SpawnObstacles(List<SO_SectionData.SectionContent.SectionObstacleData> data)
    {
        if (data == null) return;
        // Override data (if applicable) and place the collectible object!
        foreach (var item in data)
        {
            int id = item.data.obstacleType switch
            {
                Section_Obstacle_Object.ObstacleType.TrashPile => trashObjectID,
                Section_Obstacle_Object.ObstacleType.WideTrashPile => wideTrashObjectID,
                Section_Obstacle_Object.ObstacleType.SewerPipe => pipeObjectID,
                Section_Obstacle_Object.ObstacleType.Bridge => bridgeObjectID,
                _ => throw new ArgumentOutOfRangeException()
            };
            var obs = ObjectPoolManager.Instance.Spawn<River_Obstacle>(id);

            switch (obs)
            {
                // Check if the obstacle is a pipe
                case River_PipeObstacle_New pipe:
                    pipe.AssignPipeData(item.data.pipeObstacleData);
                    break;
                case RiverObstacleBridge bridge:
                    bridge.AssignObstacleData(item.data.obstacleBridgeData);
                    break;
                default:
                    obs.OverrideData(item.data.overriddenData);
                    break;
            }
            
            PlaceSectionObject(obs, item.lane, item.distance, item.height);
        }
    }

    /// <summary> Method for spawning Enemies and their respective types </summary>
    private void SpawnEnemies(List<SO_SectionData.SectionContent.SectionEnemyData> data)
    {
        if (data == null) return;
        // Override data (if applicable) and place the collectible object!
        foreach (var item in data)
        {
            int id = item.data.enemyType switch
            {
                Section_Enemy_Object.EnemyType.Crocodile => crocodileObjectID,
                Section_Enemy_Object.EnemyType.Frog => frogObjectID,
                Section_Enemy_Object.EnemyType.Bat => batObjectID,
                Section_Enemy_Object.EnemyType.Tentacle => tentacleObjectID,
                _ => throw new ArgumentOutOfRangeException()
            };
            var enm = ObjectPoolManager.Instance.Spawn<River_Enemy>(id);
            // Override Data
            enm.OverrideData(item.data.overridedData);
            PlaceSectionObject(enm, item.lane, item.distance, item.height);
        }
    }
    
    /// <summary> Method for spawning Gemstone Gates </summary>
    private void SpawnCollectibles(List<SO_SectionData.SectionContent.SectionCollectibleData> data)
    {
        if (data == null) return;
        // Override data (if applicable) and place the collectible object!
        foreach (var item in data)
        {
            var col = ObjectPoolManager.Instance.Spawn<Gemstone>(gemStoneObjectID);
            // Override Data
            if (item.data.overrideData) col.OverrideData(item.data.overridedData);
            PlaceSectionObject(col, item.lane, item.distance, item.height);
        }
    }

    /// <summary> Method for spawning Gemstone Gates </summary>
    private void SpawnGemstoneGates(List<SO_SectionData.SectionContent.SectionGemstoneGateData> data)
    {
        if (data == null) return;
        foreach (var item in data)
        {
            var gsg = ObjectPoolManager.Instance.Spawn<Gemstone_Gate>(gemStoneGateObjectID);
            // Override Data
            if (item.data.overrideData) gsg.OverrideData(item.data.overridedData);
            PlaceSectionObject(gsg, item.lane, item.distance, item.height);
        }
    }
    
    /// <summary> Method for spawning River Slipstreams </summary>
    private void SpawnSlipStreams(List<SO_SectionData.SectionContent.SectionSlipStreamData> data)
    {
        if (data == null) return;
        foreach (var item in data)
        {
            var gsg = ObjectPoolManager.Instance.Spawn<River_SlipStream>(slipStreamObjectID);
            // Override Data
            if (item.data.overrideData) gsg.OverrideData(item.data.overridedData);
            PlaceSectionObject(gsg, item.lane, item.distance, item.height);
        }
    }

    private void PlaceSectionObject(River_Object ro, int lane, int distance, int height)
    {
        // Debug.Log($"Placing Object: {ro.name}. Lane = {lane}, Distance = {distance}, Height = {height}");
        ro.InjectRiverManager(riverManager);
        ro.canMove = true;

        float spawnDist = distance;
        
        // Store the distance and object, as to detect the furthest object in this section
        if (spawnDist > furthestDistance)
        {
            lastSpawnedObject = ro;
            furthestDistance = spawnDist;
        }

        // TODO: Add a set distance between each section
        
        float finalDistance = riverManager.riverObjectSpawnDistance + _currentSectionOffset + distance;
        
        // Initial Spawn Distance from Boat * the current section displacement + Space Between Each Section
        // distance = (distance + riverManager.riverObjectSpawnDistance) * (currentSectionIndex + 1) + 10; //* currentSectionIndex + 1;
        distance = (int)finalDistance;
        
        ro.StartOnLane(lane, distance, height);
    }
    #endregion
}