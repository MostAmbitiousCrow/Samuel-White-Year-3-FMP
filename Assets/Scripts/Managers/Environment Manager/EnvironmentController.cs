using System.Collections.Generic;
using EditorAttributes;
using UnityEngine;
using UnityEngine.Pool;

public class EnvironmentController : MonoBehaviour
{
    private void Start()
    {
        // Create Pools
        CreatePools();
        
        GenerateInitialEnvironment();
    }

    private void Update()
    {
        PerlinCurveEnvironment();

        if (currentPool == null) return;
        // Place Blocks
        SpawnBlocks();
        MoveEnvironment();
    }
    
    private void MoveEnvironment()
    {
        var offset = Vector3.back * (River_Manager.Instance.currentRiverSpeed * Time.deltaTime);

        foreach (var block in activeBlocks)
        {
            block.transform.position += offset;
        }
    }

    #region Shader Environment Curving

    [Header("Environment Curving")]
    private static readonly int HorizontalCurve = Shader.PropertyToID("_HorizontalCurve");

    private static readonly int VerticalCurve = Shader.PropertyToID("_VerticalCurve");

    [SerializeField, Range(0f, 0.1f)] private float horizontalCurveScale = 0.1f;
    [SerializeField, Range(0f, 0.1f)] private float verticalCurveScale = 0.1f;

    [SerializeField] private float speedMultiplier = 1f;
    [SerializeField] private float noiseScale = 1f;
    [SerializeField, ReadOnly] private float horizontalValue, verticalValue;
    private float time;
    
    private void PerlinCurveEnvironment()
    {
        time += Time.deltaTime * River_Manager.Instance.currentRiverSpeed * speedMultiplier;
        
        var horizontalNoise = Mathf.PerlinNoise(time * noiseScale * horizontalCurveScale, 0f) * 2f - 1f;
        
        var verticalNoise = Mathf.PerlinNoise(0f, time * noiseScale * verticalCurveScale) * 2f - 1f;
        
        horizontalValue = horizontalNoise * horizontalCurveScale;
        verticalValue = verticalNoise * verticalCurveScale;
        
        Shader.SetGlobalFloat(HorizontalCurve, horizontalValue);
        Shader.SetGlobalFloat(VerticalCurve, verticalValue);
    }
    
    #endregion
    
    #region Pooling and Block Placing

    [Header("Environment Pooling")]
    [SerializeField] private Transform folder;
    [SerializeField] private SO_Environment[] environmentDatas;

    /// <summary> The distance of which a block will despawn after the players boat has surpassed its end point </summary>
    [SerializeField] private float despawnDistance = -10f;

    /// <summary> The maximum blocks the controller will allow to exist in the scene </summary>
    [SerializeField] private int maxBlocks = 10;
    
    private readonly Dictionary<Environments, EnvironmentPool> environmentPools = new();
    private readonly List<EnvironmentBlock> activeBlocks = new();
    private EnvironmentPool currentPool = new();
    [SerializeField, ReadOnly] private EnvironmentBlock lastBlock;
    [SerializeField, ReadOnly] private EnvironmentBlock closestBlock;
    
    private void CreatePools()
    {
        foreach (var data in environmentDatas)
        {
            if (!data || data.blocks.Length < 1) continue;
            if (data.blocks.Length < 1) continue;
            if (environmentPools.ContainsKey(data.environmentType)) continue;
            
            // Add environment scriptable object data to the pool dictionary with the reference to its pool class
            var pool = new EnvironmentPool { controller = this, environmentData = data };
            pool.CreatePool();
            environmentPools.Add(data.environmentType, pool);
        }
        Debug.Log($"Created {environmentPools.Count} environment pools.");
    }
    
    private void GenerateInitialEnvironment()
    {
        // Select the current environment pool based on the current environment
        currentPool = environmentPools[GameLevelManager.CurrentEnvironment];
        
        // Spawn blocks ahead of the world position
        for (int i = 0; i < maxBlocks; i++) SpawnNextBlockAhead();
        closestBlock = activeBlocks[0];
    }
    
    private void SpawnBlocks()
    {
        // Get the furthest block
        closestBlock = activeBlocks[0];
        
        var distanceToEnd = closestBlock.EndAnchor.transform.position.z;
        
        // If the closest blocks end anchor has reached past the despawn distance check:
        // Despawn the closest block and spawn the next block ahead
        if (distanceToEnd < despawnDistance)
        {
            DeSpawnBlocksBehind();
            SpawnNextBlockAhead();
        }
    }
    
    private void DeSpawnBlocksBehind()
    {
        if (activeBlocks.Count == 0) return;
        
        var firstBlock = activeBlocks[0];
        currentPool.objectPool.Release(firstBlock);
        
        activeBlocks.Remove(firstBlock);
    }
    
    private void SpawnNextBlockAhead()
    {
        if (currentPool == null)
        {
            Debug.LogWarning("No pool currently selected to spawn a block.");
            return;
        }
        
        EnvironmentBlock block = currentPool.objectPool.Get();
        
        PositionBlock(block);
        activeBlocks.Add(block);
        
        lastBlock = block;
    }
    
    private void PositionBlock(EnvironmentBlock block)
    {
        Vector3 targetPosition;

        if (!lastBlock)
        {
            // Backwards offset so you don't see the skybox during the first few frames
            targetPosition = Vector3.back * 20f;
        }
        else
        {
            targetPosition = lastBlock.EndAnchor.position;
        }

        block.transform.position = targetPosition - block.StartAnchor.localPosition;
    }
    
    private class EnvironmentPool
    {
        public SO_Environment environmentData;
        public EnvironmentController controller;
        public IObjectPool<EnvironmentBlock> objectPool;
        
        public void CreatePool()
        {
            objectPool = new ObjectPool<EnvironmentBlock>
                (OnBlockCreated, OnBlockTaken, OnBlockReturned, OnDestroyBlock,
                    true, controller.maxBlocks, controller.maxBlocks);
        }
        
        private EnvironmentBlock OnBlockCreated()
        {
            //TODO: figure out how to diversify
            var block = Instantiate(environmentData.blocks[0], controller.folder, true);
            var data = block.GetComponent<EnvironmentBlock>();
            return data;
        }
        
        private void OnBlockTaken(EnvironmentBlock block)
        {
            block.gameObject.SetActive(true);
            block.OnSpawned();
        }
        
        private void OnBlockReturned(EnvironmentBlock block)
        {
            block.OnReturned();
            block.gameObject.SetActive(false);
        }
        
        private void OnDestroyBlock(EnvironmentBlock block)
        {
            Destroy(block.gameObject);
        }
    }
    
    #endregion
}