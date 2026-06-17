using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "SO_LevelsContainer", menuName = "ScriptableObjects/SO_LevelsContainer")]
public class SO_LevelsContainer : ScriptableObject
{
    public SO_LevelData[] levels;
    public SO_SectionData[] sections;
    
    public Dictionary<Environments, SO_LevelData[]> SortedLevels = new();
    public Dictionary<Environments, Dictionary<GameDifficulty, List<SO_SectionData>>> SortedSections = new();
    
    #if UNITY_EDITOR
    private void OnValidate()
    {
        if (levels == null) return;
        if (sections == null) return;
        
        SortLevels();
        SortSections();
    }
    #endif

    public void SortLevels()
    {
        var environmentLevels = new Dictionary<Environments, List<SO_LevelData>>();
        SortedLevels.Clear();
        
        var count = Enum.GetNames(typeof(Environments)).Length;

        // Create Sorted Levels Dictionary using available level types
        foreach (Environments environment in Enum.GetValues(typeof(Environments))) 
            environmentLevels.Add(environment, new List<SO_LevelData>());
        
        foreach (var level in levels)
        {
            environmentLevels.TryGetValue(level.environmentType, out var data);
            data?.Add(level);
        }

        var sb = new StringBuilder();

        foreach (var level in environmentLevels)
        {
            sb.Append($"{level.Key}: {level.Value.Count} levels ");
            SortedLevels.Add(level.Key, level.Value.ToArray());
        }

        Debug.Log($"Levels Sorted. Details: {sb}");
    }

    public void SortSections()
    {
        var sortedSections = new Dictionary<Environments, Dictionary<GameDifficulty, List<SO_SectionData>>>();
        SortedSections.Clear();
        
        var count = Enum.GetNames(typeof(Environments)).Length;
        
        // Create sorted sections categorised by Environment and difficulty
        foreach (Environments environment in Enum.GetValues(typeof(Environments))) 
            sortedSections.Add(environment, new Dictionary<GameDifficulty, List<SO_SectionData>>
            {
                { GameDifficulty.Easy, new List<SO_SectionData>() },
                { GameDifficulty.Medium, new List<SO_SectionData>() },
                { GameDifficulty.Hard, new List<SO_SectionData>() }
            });
        
        var environmentMap = new Dictionary<
            SO_SectionData.SectionContent.AvailableEnvironments,
            Environments>
        {
            { SO_SectionData.SectionContent.AvailableEnvironments.Sewer, Environments.Sewer },
            { SO_SectionData.SectionContent.AvailableEnvironments.Pyramid, Environments.Pyramid },
            { SO_SectionData.SectionContent.AvailableEnvironments.Cave, Environments.Cave },
            { SO_SectionData.SectionContent.AvailableEnvironments.Forest, Environments.Forest },
            { SO_SectionData.SectionContent.AvailableEnvironments.Dungeon, Environments.Dungeon }
        };

        // Sort Sections into Environment and Difficulty
        foreach (var section in sections)
        {
            foreach (var pair in environmentMap)
            {
                if (section.sectionContent.applicableEnvironments.HasFlag(pair.Key))
                {
                    AddBasedOnDifficulty(sortedSections[pair.Value]);
                }
            }
            continue;

            void AddBasedOnDifficulty(Dictionary<GameDifficulty, List<SO_SectionData>> sort)
            {
                if (section.sectionContent.difficultyType == DifficultyQualification.None)
                {
                    Debug.LogWarning("Difficulty was None, skipped");
                    return;
                }
                
                // Difficulty Sorting
                if (section.sectionContent.difficultyType.HasFlag(DifficultyQualification.Easy))
                {
                    sort[GameDifficulty.Easy].Add(section);
                }
                if (section.sectionContent.difficultyType.HasFlag(DifficultyQualification.Medium))
                {
                    sort[GameDifficulty.Medium].Add(section);
                }
                if (section.sectionContent.difficultyType.HasFlag(DifficultyQualification.Hard))
                {
                    sort[GameDifficulty.Hard].Add(section);
                }
            }
        }

        SortedSections = sortedSections;
        #if UNITY_EDITOR
        var sb = new StringBuilder();

        sb.AppendLine("Sections Sorted:");
        sb.AppendLine("--------------------");

        foreach (var environment in SortedSections)
        {
            sb.AppendLine();
            sb.AppendLine($"Environment: {environment.Key}");

            foreach (var difficulty in environment.Value)
            {
                sb.AppendLine(
                    $"  {difficulty.Key,-6} : {difficulty.Value.Count} sections");
            }
        }

        Debug.Log(sb.ToString());
        #endif
    }
    
    public SO_SectionData GetRandomSection(Environments environment, GameDifficulty difficulty)
    {
        if (!SortedSections.TryGetValue(environment, out var env))
            return null;

        if (!env.TryGetValue(difficulty, out var sections))
            return null;

        if (sections.Count == 0) return null;
        
        return sections[Random.Range(0, sections.Count)];
    }
    
    public SO_LevelData GetRandomLevel(Environments environment)
    {
        if (!SortedLevels.TryGetValue(environment, out var env))
            return null;

        if (env.Length == 0)
            return null;
        
        return levels[Random.Range(0, levels.Length)];
    }

    
    // public List<SO_SectionData> GetSections(Environments environment, SO_GameDifficultyValues difficultyValues, float difficulty)
    // {
    //     // Select Sections based on the given environment
    //     SortedSections.TryGetValue(environment, out var data);
    //     
    //     
    //     // Curate the section selection based on the difficulty
    //     // store the current difficulty to make sorting quicker
    //
    //     var values = difficultyValues.GameDifficultyValues[0]; // TODO
    //     var sections = new List<SO_SectionData>(data);
    //     
    //     // Randomly select sections
    //     for (int i = 0; i < difficultyValues.GetSectionCount(values, difficulty); i++)
    //     {
    //         
    //     }
    //     
    //     data.TryGetValue()
    //     
    //     // Return the curated sections
    //     
    // }
}
