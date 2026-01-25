using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using EditorAttributes;

public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager Instance { get; private set; }
    
    [SerializeField] private Transform objectFolder;
    [Tooltip("Initialises the pool upon loading. No trigger required.")]
    [SerializeField] private bool autoStart = true;

    
    // Poolable items
    [Serializable]
    public struct PoolItem
    {
        public string name; // Organisation purposes
        [AssetPreview(64, 64)] public GameObject prefab;
        public int amount;
    }

    [Header("Pooling")]
    [SerializeField] private List<PoolItem> itemsToPool;
    
    // Note: Uses a Prefab (Key) to obtain its specific Pool (Value)
    private Dictionary<GameObject, ObjectPool<GameObject>> _poolDictionary = new Dictionary<GameObject, ObjectPool<GameObject>>();
    
    public bool IsPoolReady { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
    }

    private void Start()
    {
        if (autoStart) InitializePools();
    }

    public void InitializePools()
    {
        if (IsPoolReady) return;
        StartCoroutine(CreatePoolsRoutine());
    }

    private IEnumerator CreatePoolsRoutine()
    {
        IsPoolReady = false;

        // List through each pool items
        foreach (var item in itemsToPool)
        {
            if (item.prefab == null) continue; // Skip if the prefab isn't assigned

            // Create Object Pool // https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Pool.ObjectPool_1.html
            
            // Sets up events to do the following:
            // Instantiate the prefab upon the pool being created
            // Once taken, enable the object
            // Once returned, disable the object
            // If object can't access the pool, destroy
            // Set initial capacity and max capacity based on the object pool size
            var newPool = new ObjectPool<GameObject>
            ( // Thanks Rider for simplifying this lol
                createFunc: () => CreateFunction(item.prefab),
                actionOnGet: (obj) => obj.SetActive(true),
                actionOnRelease: (obj) => obj.SetActive(false),
                actionOnDestroy: (obj) => Destroy(obj), 
                defaultCapacity: item.amount,
                maxSize: item.amount
            );

            _poolDictionary.Add(item.prefab, newPool);
            
            if (item.amount < 1) yield break;
            
            // Create the objects, release to its registered pool
            var op = InstantiateAsync(item.prefab, item.amount, objectFolder, Vector3.one * 1000, Quaternion.identity);
            yield return new WaitUntil(() => op.isDone);

            foreach (var instance in op.Result)
            {
                // Release (Adding) the created objects to the pool
                newPool.Release(instance); 
            }
            
            Debug.Log($"{item.name} Pool Created.");
        }

        IsPoolReady = true;
    }

    // Used by the pool when it runs out of objects and needs to create a new one synchronously
    // Technically don't need this but the Manual insists...
    private GameObject CreateFunction(GameObject prefab)
    {
        return Instantiate(prefab, objectFolder);
    }

    #region Pooling Methods

    // https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/generics/constraints-on-type-parameters
    /// <summary> Spawns an object from the pool and returns the specific component requested. </summary>
    public T Spawn<T>(GameObject type, Vector3 position, Quaternion rotation) where T : IPooledObject // T stands for 'type', so it can be basically anything :D
    {
        if (!IsPoolReady || !_poolDictionary.ContainsKey(type))
        {
            Debug.LogError($"Pool for {type} not found or not ready!");
        }

        // Get object type from the Unity Pool
        GameObject obj = _poolDictionary[type].Get();
        
        // Set the specified position
        obj.transform.SetPositionAndRotation(position, rotation);

        // Return the specific component
        return obj.GetComponent<T>();
    }
    
    public T Spawn<T>(GameObject type) where T : IPooledObject
    {
        if (!IsPoolReady || !_poolDictionary.ContainsKey(type))
        {
            Debug.LogError($"Pool for {type} not found or not ready!");
        }

        // Get object type from the Unity Pool
        GameObject obj = _poolDictionary[type].Get();

        // Return the specific component
        return obj.GetComponent<T>();
    }

    /// <summary> Returns an object back to its specific pool. </summary>
    public void ReturnToPool(GameObject type, GameObject instance)
    {
        if (_poolDictionary.TryGetValue(type, out var pool))
        {
            pool.Release(instance);
        }
        else
        {
            Debug.LogWarning("Pool does not exist. Destroying instead.");
            Destroy(instance);
        }
    }

    #endregion

    [ContextMenu("Clear Pools")]
    public void DestroyPools()
    {
        if(_poolDictionary == null) return;

        foreach (var pool in _poolDictionary.Values)
        {
            pool.Clear(); // Destroys all objects inside
        }
        _poolDictionary.Clear();
        IsPoolReady = false;
    }
}