using UnityEngine;
using System.Collections;
using EditorAttributes;
using System.Collections.Generic;

/// <summary>
/// Controls and stores the flow and order of data from level sections
/// </summary>
public class Game_Section_Manager : MonoBehaviour, IAffectedByRiver, ITargetsBoat
{
    [Header("Level Info")]
    [SerializeField] private GameLevels currentLevel = GameLevels.Sewer;
    
    [Header("Section Data")]
    public List<Section_Content.SectionData> sectionDatas;

    [Header("Section Info")]
    [Min(0)][SerializeField] int currentSection = 0;
    /// <summary>
    /// Is the current section halted from progressing onto its post delay?
    /// </summary>
    [SerializeField] bool isHalted;

    #region Section Objects
    [Header("Section Objects")] // TODO: rework to use object pooling!
    [Line(GUIColor.Cyan)]
    [FoldoutGroup("Obstacle Objects", nameof(trashObject), nameof(pipeObject))]
    [SerializeField] Void obstacleGroup;
    [SerializeField, HideProperty] GameObject trashObject;
    [SerializeField, HideProperty] GameObject pipeObject;
    
    [Line(GUIColor.Red)]
    [FoldoutGroup("Enemy Objects", nameof(crocodileObject), nameof(frogObject), nameof(batObject), nameof(tentacleObject))]
    [SerializeField] Void enemyGroup;
    [SerializeField, HideProperty] GameObject crocodileObject, frogObject, batObject, tentacleObject;
    
    [Line(GUIColor.Yellow)]
    [FoldoutGroup("Collectible Objects", nameof(gemStoneObject), nameof(gemFragmentObject))]
    [SerializeField] Void collectibleGroup;
    [SerializeField, HideProperty] GameObject gemStoneObject;
    [SerializeField, HideProperty] GameObject gemFragmentObject;
    
    [Line(GUIColor.White)]
    [SerializeField] GameObject gemStoneGateObject;
    #endregion


    private void OnEnable()
    {
        GameManager.GameLogic.onGameStarted += StartSpawning;
    }

    private void OnDisable()
    {
        GameManager.GameLogic.onGameStarted -= StartSpawning;
    }

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
        StartSpawning();
    }

    public void StartSpawning()
    {
        StartCoroutine(SpawnSectionObjects());
    }

    IEnumerator SpawnSectionObjects()
    {
        if (currentSection < 0 || currentSection >= sectionDatas.Count)
        {
            Debug.LogError($"currentSection index {currentSection} is out of bounds for sectionDatas (Count: {sectionDatas.Count}).");
            yield break;
        }

        // Spawn Provided Section Data
        for (int i = 0; i < sectionDatas.Count; i++)
        {
            Section_Content.SectionData s = sectionDatas[currentSection];
            
            if(s == null) continue;
            
            // Initial Delay
            yield return new WaitForSeconds(sectionDatas[currentSection].initialDelay);

            #region Spawn Obstacles
            // TODO: Rework object spawning so that they all start moving after the async is completed
            
            // Spawn Obstacles
            int c = s.ObstacleDatas.Count; // Count Obstacles
            for (int j = 0; j < c; j++)
            {
                // Get the obstacle data and the pooled obstacle
                var obstacleData = sectionDatas?[currentSection].ObstacleDatas[j];
                River_Obstacle obstacle;

                if (!obstacleData) continue;

                obstacle = obstacleData.sectionData.obstacleType switch
                {
                    Section_Obstacle_Object.ObstacleType.TrashPile => ObjectPoolManager.Instance.Spawn<River_Obstacle>(
                        trashObject),
                    Section_Obstacle_Object.ObstacleType.SewerPipe => ObjectPoolManager.Instance.Spawn<Pipe_Obstacle>(
                        pipeObject),
                };

                if (!obstacle) continue;
                
                // Update pipe data if the obstacle is a pipe
                if (obstacleData.sectionData.obstacleType == Section_Obstacle_Object.ObstacleType.SewerPipe)
                {
                    var pipeObstacle = (Pipe_Obstacle)obstacle;
                    pipeObstacle.PipeData = obstacleData.sectionData.pipeObstacleData;
                }
                
                // Check if this obstacles data be overridden
                if (obstacleData.sectionData.overrideData)
                {
                    obstacle.OverrideData(obstacleData.sectionData.overridedData);
                }
                
                // Place the obstacle in the world!
                PlaceSectionObject(obstacle, obstacleData);

                yield return null;
            }
            #endregion

            #region Spawn Enemies
            // Spawn Enemies
            c = s.EnemyDatas.Count;
            for (int j = 0; j < c; j++)
            {
                // Get the obstacle data and the pooled obstacle
                var enemyData = sectionDatas?[currentSection].EnemyDatas[j];
                River_Enemy enemy;

                if (!enemyData) continue;

                enemy = enemyData.sectionData.enemyType switch
                {
                    Section_Enemy_Object.EnemyType.Crocodile => ObjectPoolManager.Instance.Spawn<River_Enemy>(crocodileObject),
                    Section_Enemy_Object.EnemyType.Frog => ObjectPoolManager.Instance.Spawn<River_Enemy>(frogObject),
                    Section_Enemy_Object.EnemyType.Bat => ObjectPoolManager.Instance.Spawn<River_Enemy>(batObject),
                    Section_Enemy_Object.EnemyType.Tentacle => ObjectPoolManager.Instance.Spawn<River_Enemy>(tentacleObject),
                };

                if (!enemy) continue;
                
                // Check if this enemies data will be overridden
                if (enemyData.sectionData.overrideData)
                {
                    enemy.OverrideStats(enemyData.sectionData.overridedData);
                }
                
                // Place the obstacle in the world!
                PlaceSectionObject(enemy, enemyData);

                yield return null;
            }
            #endregion

            #region Spawn Collectibles
            // Spawn Collectibles
            c = s.CollectibleDatas.Count;
            for (int j = 0; j < c; j++)
            {
                // Get the Collectible data and the pooled Collectibles
                var collectibleData = sectionDatas?[currentSection].CollectibleDatas[j];
                River_Collectible collectible;

                if (!collectibleData) continue;

                collectible = collectibleData.sectionData.collectibleType switch
                {
                    Section_Collectible_Object.CollectibleType.Gemstone => ObjectPoolManager.Instance.Spawn<River_Collectible>(gemStoneObject),
                    Section_Collectible_Object.CollectibleType.GemstoneFragment => ObjectPoolManager.Instance.Spawn<River_Collectible>(gemFragmentObject),
                };

                if (!collectible) continue;
                
                // Check if this enemies data will be overridden
                if (collectibleData.sectionData.overrideData)
                {
                    collectible.OverrideData(collectibleData.sectionData.overridedData);
                }
                
                // Place the obstacle in the world!
                PlaceSectionObject(collectible, collectibleData);

                yield return null;
            }
            #endregion

            //TODO: Maybe somehow involve this with the obstacles?
            #region Spawn Gemstone Gates
            c = s.GemstoneGateDatas.Count;
            for (int j = 0; j < c; j++)
            {
                // Get the Gate data and the pooled Gemstone Gates
                var gateData = sectionDatas?[currentSection].GemstoneGateDatas[j];
                Gemstone_Gate gemstoneGate;

                if (!gateData) continue;

                gemstoneGate = ObjectPoolManager.Instance.Spawn<Gemstone_Gate>(gemStoneGateObject);

                if (!gemstoneGate) continue;
                
                // Check if this enemies data will be overridden
                if (gateData.sectionData.overrideData)
                {
                    gemstoneGate.OverrideData(gateData.sectionData.overridedData);
                }
                
                // Place the obstacle in the world!
                PlaceSectionObject(gemstoneGate, gateData);

                yield return null;
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
    private void PlaceSectionObject(River_Object ro, Section_Builder_Object sbo)
    {
        ro.canMove = true;
        ro.isMoving = true;
        ro.StartOnLane(sbo.Lane, sbo.Distance + riverManager.RiverObjectSpawnDistance, sbo.Height);

        Debug.Log($"{ro.name} was placed");
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

public enum GameLevels
{
    Sewer, Forest, GemstoneCavern
}