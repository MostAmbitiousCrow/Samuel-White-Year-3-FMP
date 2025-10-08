using UnityEngine;
using System.Collections;
using EditorAttributes;
using System.Collections.Generic;
using System.Linq;


/// <summary>
/// Controls and stores the flow and order of data from level sections
/// </summary>
public class Game_Section_Manager : MonoBehaviour, IAffectedByRiver, ITargetsBoat
{
    [Header("Section Data")]
    public List<Section_Content.SectionData> sectionDatas;

    [Header("Section Info")]
    [Min(0)][SerializeField] int currentSection = 0;
    /// <summary>
    /// Is the current section halted from progressing onto its post delay?
    /// </summary>
    [SerializeField] bool isHalted;

    [Header("Section Objects")] // TODO: rework to use object pooling!
    [Line(GUIColor.Cyan)]
    [FoldoutGroup("Obstacle Objects", nameof(trashObject), nameof(pipeObject))]
    [SerializeField] Void obstacleGroup;
    [SerializeField, HideProperty] GameObject trashObject;
    [SerializeField, HideProperty] GameObject pipeObject;

    [Line(GUIColor.Red)]
    [FoldoutGroup("Enemy Objects", nameof(crocodileObject), nameof(wormObject))]
    [SerializeField] Void enemyGroup;
    [SerializeField, HideProperty] GameObject crocodileObject;
    [SerializeField, HideProperty] GameObject wormObject;

    [Line(GUIColor.Yellow)]
    [FoldoutGroup("Collectible Objects", nameof(gemStoneObject), nameof(gemFragmentObject))]
    [SerializeField] Void collectibleGroup;
    [SerializeField, HideProperty] GameObject gemStoneObject;
    [SerializeField, HideProperty] GameObject gemFragmentObject;

    [Line(GUIColor.White)]
    [SerializeField] GameObject gemStoneGateObject;

    [Button]
    public void GetSectionDatas()
    {
        sectionDatas.Clear();
        for (int i = 0; i < transform.childCount; i++)
        {
            sectionDatas.Add(transform.GetChild(i).GetComponent<Section_Content>().sectionData);
        }
    }

    [Button]
    public void SpawnButton()
    {
        if (Application.isPlaying) StartCoroutine(SpawnSectionObjects());
    }

    IEnumerator SpawnSectionObjects()
    {
        if (currentSection < 0 || currentSection >= sectionDatas.Count)
        {
            Debug.LogError($"currentSection index {currentSection} is out of bounds for sectionDatas (Count: {sectionDatas.Count}).");
            yield break;
        }

        for (int i = 0; i < sectionDatas.Count; i++)
        {
            Section_Content.SectionData s = sectionDatas[currentSection];

            // Initial Delay
            yield return new WaitForSeconds(sectionDatas[currentSection].initialDelay);

            int c; // Count

            #region Spawn Obstacles
            // Spawn Obstacles
            c = s.ObstacleDatas.Count;
            for (int j = 0; j < c; j++)
            {
                Section_Obstacle_Object.SectionObstacleData obstacleData = sectionDatas?[currentSection].ObstacleDatas[j].sectionData;

                // Instantiate the selected object type
                AsyncInstantiateOperation<GameObject> op = default;

                if (obstacleData.obstacleType == Section_Obstacle_Object.ObstacleType.TrashPile)
                    op = InstantiateAsync(trashObject, new Vector3(100f, 100f, 100f), Quaternion.identity);
                else if (obstacleData.obstacleType == Section_Obstacle_Object.ObstacleType.SewerPipe)
                    op = InstantiateAsync(pipeObject, new Vector3(100f, 100f, 100f), Quaternion.identity);

                // Wait until the object has been Instantiated
                yield return new WaitUntil(() => op.isDone);

                GameObject result = op.Result.First();

                // Obtain the items River Component and obtain its alligned obstacle data
                if (!result.TryGetComponent<River_Obstacle>(out var itemData))
                {
                    Debug.LogError("River Component is missing!");
                    yield break;
                }

                // Override Data (if true)
                if (obstacleData.overrideData)
                {
                    itemData.OverrideData(obstacleData.overridedData);
                }

                // Provide Pipe Data if obstacle is a pipe
                if (obstacleData.obstacleType == Section_Obstacle_Object.ObstacleType.SewerPipe)
                {
                    itemData.GetComponent<Pipe_Obstacle>().PipeData = obstacleData.pipeObstacleData;
                }

                // Inject River_Manager
                itemData.InjectRiverManager(riverManager);

                // Place the obstacle in the world
                PlaceSectionObject(itemData, sectionDatas[currentSection].ObstacleDatas[j]);

                // Trigger Spawned Method
                itemData.Spawned();

                yield return null;
            }
            #endregion

            #region Spawn Enemies
            // Spawn Enemies
            c = s.EnemyDatas.Count;
            for (int j = 0; j < c; j++)
            {
                Section_Enemy_Object.SectionEnemyData enemyData = sectionDatas?[currentSection].EnemyDatas[j].sectionData;

                // Instantiate the selected object type
                AsyncInstantiateOperation<GameObject> op = default;

                if (enemyData.enemyType == Section_Enemy_Object.EnemyType.Crocodile)
                    op = InstantiateAsync(crocodileObject, new Vector3(100f, 100f, 100f), Quaternion.identity);
                else if (enemyData.enemyType == Section_Enemy_Object.EnemyType.Worm)
                    op = InstantiateAsync(wormObject, new Vector3(100f, 100f, 100f), Quaternion.identity);

                // Wait until the object has been Instantiated
                yield return new WaitUntil(() => op.isDone);

                GameObject result = op.Result.First();

                // Obtain the items River Component and obtain its alligned enemy data
                if (!result.TryGetComponent<River_Enemy>(out var itemData))
                {
                    Debug.LogError($"{result.name}s River Component is missing!");
                    continue;
                }

                // Override Data (if true)
                if (enemyData.overrideData)
                {
                    itemData.OverrideStats(enemyData.overridedData);
                }

                // Inject River_Manager
                itemData.InjectRiverManager(riverManager);

                // Place the enemy in the world
                PlaceSectionObject(itemData, sectionDatas[currentSection].EnemyDatas[j]);

                // Trigger Spawned Method
                itemData.Spawned();

                yield return null;
            }
            #endregion

            #region Spawn Collectibles
            // Spawn Collectibles
            c = s.CollectibleDatas.Count;
            for (int j = 0; j < c; j++)
            {
                Section_Collectible_Object.SectionCollectibleData collectibleData = sectionDatas?[currentSection].CollectibleDatas[j].sectionData;

                // Instantiate the selected object type
                AsyncInstantiateOperation<GameObject> op = default;

                if (collectibleData.collectibleType == Section_Collectible_Object.CollectibleType.Gemstone)
                    op = InstantiateAsync(gemStoneObject, new Vector3(100f, 100f, 100f), Quaternion.identity);
                else if (collectibleData.collectibleType == Section_Collectible_Object.CollectibleType.GemstoneFragment)
                    op = InstantiateAsync(gemFragmentObject, new Vector3(100f, 100f, 100f), Quaternion.identity);

                // Wait until the object has been Instantiated
                yield return new WaitUntil(() => op.isDone);

                GameObject result = op.Result.First();

                // Obtain the items River Component and obtain its alligned collectible data
                if (!result.TryGetComponent<River_Collectible>(out var itemData))
                {
                    Debug.LogError("River Component is missing!");
                    continue;
                }

                // Override Data (if true)
                if (collectibleData.overrideData)
                {
                    itemData.OverrideData(collectibleData.overridedData);
                }

                // Inject River_Manager
                itemData.InjectRiverManager(riverManager);

                // Place the collectible in the world
                PlaceSectionObject(itemData, sectionDatas[currentSection].CollectibleDatas[j]);

                // Trigger Spawned Method
                itemData.Spawned();

                yield return null;
            }
            #endregion

            #region Spawn Gemstone Gates
            c = s.GemstoneGateDatas.Count;
            for (int j = 0; j < c; j++)
            {
                Section_Gemstone_Gate.SectionGemstoneGateData gateData = sectionDatas?[currentSection].GemstoneGateDatas[j].sectionData;

                AsyncInstantiateOperation<GameObject> op = default;
                op = InstantiateAsync(gemStoneGateObject, new Vector3(100f, 100f, 100f), Quaternion.identity);

                // Wait until the object has been Instantiated
                yield return new WaitUntil(() => op.isDone);

                GameObject result = op.Result.First();

                // Obtain the items River Component and obtain its alligned collectible data
                if (!result.TryGetComponent<Gemstone_Gate>(out var itemData))
                {
                    Debug.LogError("River Component is missing!");
                    continue;
                }

                // Override Data (if true)
                if (gateData.overrideData)
                {
                    itemData.OverrideData(gateData.overridedData);
                }

                // Inject River_Manager and Boat_Space_Manager
                itemData.InjectRiverManager(riverManager);
                itemData.InjectBoatSpaceManager(boatManager);

                // Place the collectible in the world
                PlaceSectionObject(itemData, sectionDatas[currentSection].CollectibleDatas[j]);

                // Trigger Spawned Method
                itemData.Spawned();
            }
            #endregion

            // Halt section
            yield return new WaitUntil(() => !isHalted);

            // Post Delay
            yield return new WaitForSeconds(sectionDatas[currentSection].postDelay);
            currentSection++;
        }

        yield break;
    }

    #region Placement
    /// <summary>
    /// Places the River Object based on the positioning data provided by the Section Builder Object
    /// </summary>
    void PlaceSectionObject(River_Object ro, Section_Builder_Object sbo)
    {
        ro.StartOnLane(sbo.Lane, sbo.Distance + riverManager.RiverObjectSpawnDistance, sbo.Height);
        ro.CanMove = true;
    }
    #endregion

    #region Injection
    [Header("Managers")]
    [SerializeField] River_Manager riverManager;

    public void InjectRiverManager(River_Manager manager)
    {
        riverManager = manager;
    }

    [SerializeField] Boat_Space_Manager boatManager;

    public void InjectBoatSpaceManager(Boat_Space_Manager bsm)
    {
        boatManager = bsm;
    }
    #endregion
}
