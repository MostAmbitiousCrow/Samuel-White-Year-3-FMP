using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SO_LevelsContainer", menuName = "ScriptableObjects/SO_LevelsContainer")]
public class SO_LevelsContainer : ScriptableObject
{
    public SO_LevelData[] levels;
    
    public Dictionary<int, SO_LevelData[]> SortedLevels = new Dictionary<int, SO_LevelData[]>();
    
    private void OnValidate()
    {
        SortLevels();
    }

    private void SortLevels()
    {
        var environmentLevels = new Dictionary<int, List<SO_LevelData>>();
        SortedLevels.Clear();
        
        var count = Enum.GetNames(typeof(Environments)).Length;

        // Create Sorted Levels Dictionary using available level types
        for (int i = 0; i < count; i++) environmentLevels.Add(i, new List<SO_LevelData>());
        
        foreach (var level in levels)
        {
            environmentLevels.TryGetValue((int)level.environmentType, out var data);
            data?.Add(level);
        }

        var logDetails = new string("");
        foreach (var level in environmentLevels)
        {
            var valueArray = level.Value.ToArray();
            SortedLevels.Add(level.Key, valueArray);

            logDetails += new string($" {(Environments)level.Key}: {valueArray.Length} levels");
        }
        
        Debug.Log("Levels Sorted. Details: " + logDetails);
    }
}
