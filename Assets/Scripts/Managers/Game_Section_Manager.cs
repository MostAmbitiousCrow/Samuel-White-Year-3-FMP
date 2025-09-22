using System.Collections;
using System.Collections.Generic;
using EditorAttributes;
using UnityEngine;

/// <summary>
/// Controls and stores the flow and order of data from level sections
/// </summary>
public class Game_Section_Manager : MonoBehaviour, IAffectedByRiver
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
    [SerializeField] GameObject obstacleObject;
    [Line(GUIColor.Red)]
    [SerializeField] GameObject enemyObject;

    [Line(GUIColor.Yellow)]
    [FoldoutGroup("Collectible Objects", nameof(gemStoneObject), nameof(gemFragmentObject))]
    [SerializeField] private Void groupHolder;
    [SerializeField, HideProperty] GameObject gemStoneObject;
    [SerializeField, HideProperty] GameObject gemFragmentObject;

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
        StartCoroutine(SpawnSectionObjects());
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
            AsyncInstantiateOperation<GameObject> op;

            // Initial Delay
            yield return new WaitForSeconds(sectionDatas[currentSection].initialDelay);

            int c; // Count
            int a = 0; // Amount Remaining

            // Spawn Obstacles
            c = s.ObstacleDatas.Count;
            while (a < c)
            {
                op = InstantiateAsync(obstacleObject, c);

                yield return new WaitUntil(() => op.isDone);
                print($"Instantiating {op.Result.Length} Obstacles");

                foreach (GameObject item in op.Result)
                {
                    // Obtain the items River Component and obtain its alligned obstacle data
                    if (!item.TryGetComponent<River_Obstacle>(out var itemData))
                    {
                        Debug.LogError("River Component is missing!");
                        continue;
                    }
                    Section_Obstacle_Object.ObstacleData obstacleData = sectionDatas?[currentSection].ObstacleDatas[a].data;

                    // Inject River_Manager
                    itemData.InjectRiverManager(riverManager);

                    // Place the obstacle in the world
                    PlaceSectionObject(itemData, sectionDatas[currentSection].ObstacleDatas[a]);

                    // Override Stats (if true)
                    if (obstacleData.overrideStats)
                    {
                        itemData.OverrideStats(obstacleData.overridedStats);
                    }

                    // Update amount
                    a++;

                    yield return null;
                }
            }

            a = 0;

            // Spawn Enemies
            c = s.EnemyDatas.Count;
            while (a < c)
            {
                op = InstantiateAsync(enemyObject, c);

                yield return new WaitUntil(() => op.isDone);

                foreach (GameObject item in op.Result)
                {
                    // Obtain the items River Component and obtain its alligned obstacle data
                    if (!item.TryGetComponent<River_Enemy>(out var itemData))
                    {
                        Debug.LogError("River Component is missing!");
                        continue;
                    }
                    Section_Enemy_Object.EnemyData enemyData = sectionDatas?[currentSection].EnemyDatas[a].data;

                    // Inject River_Manager
                    itemData.InjectRiverManager(riverManager);

                    // Place the obstacle in the world
                    PlaceSectionObject(itemData, sectionDatas[currentSection].EnemyDatas[a]);

                    // Override Stats (if true)
                    if (enemyData.overrideStats)
                    {
                        itemData.OverrideStats(enemyData.overridedStats);
                    }

                    // Update amount
                    a++;

                    yield return null;
                }
            }

            a = 0;

            // Spawn Collectibles
            c = s.CollectibleDatas.Count;
            while (a < c)
            {
                op = InstantiateAsync(gemStoneObject, c);

                yield return new WaitUntil(() => op.isDone);

                foreach (GameObject item in op.Result)
                {
                    Section_Collectible_Object.CollectibleData collectibleData = sectionDatas?[currentSection].CollectibleDatas[a].data;

                    // Obtain the items River Component and obtain its alligned obstacle data
                    if (!item.TryGetComponent<River_Collectible>(out var itemData))
                    {
                        Debug.LogError("River Component is missing!");
                        continue;
                    }

                    // Inject River_Manager
                    itemData.InjectRiverManager(riverManager);

                    // Place the obstacle in the world
                    PlaceSectionObject(itemData, sectionDatas[currentSection].CollectibleDatas[a]);

                    // Override Stats (if true)
                    if (collectibleData.overrideStats)
                    {
                        itemData.OverrideStats(collectibleData.overridedStats);
                    }

                    // Update amount
                    a++;

                    yield return null;
                }
            }

            // Halt section
            yield return new WaitUntil(() => !isHalted);

            // Post Delay
            yield return new WaitForSeconds(sectionDatas[currentSection].postDelay);
            currentSection++;
        }

        yield break;
    }

    /// <summary>
    /// Places the River Object based on the positioning data provided by the Section Builder Object
    /// </summary>
    void PlaceSectionObject(River_Object ro, Section_Builder_Object sbo)
    {
        ro.CanMove = true;
        ro.StartOnLane(sbo.Lane, sbo.Distance, sbo.Height);
    }

    #region Injection
    private River_Manager riverManager;

    public void InjectRiverManager(River_Manager manager)
    {
        riverManager = manager;
    }
    #endregion

    // https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Object.InstantiateAsync.html
}
