using System;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "GameDifficultyValues", menuName = "ScriptableObjects/GameSettings/GameDifficultyValues")]
public class SO_GameDifficultyValues : ScriptableObject
{
    [Header("Difficulty Curve")]
    public float maxDifficulty = 100f;

    [Header("Section Counts")]
    public DifficultyValues[] GameDifficultyValues = new []
    {
        // Easy
        new DifficultyValues()
        {
            difficulty = GameDifficulty.Easy,
            sectionsRange = new Vector2Int(6, 8),
            threshold = new Vector2(90f, 5f),
            levels = 2
        },
        // Medium
        new DifficultyValues()
        {
            difficulty = GameDifficulty.Medium,
            sectionsRange = new Vector2Int(8, 10),
            threshold = new Vector2(10f, 25f),
            levels = 3
        },
        // Hard
        new DifficultyValues()
        {
            difficulty = GameDifficulty.Hard,
            sectionsRange = new Vector2Int(10, 12),
            threshold = new Vector2(0f, 70f),
            levels = 4
        }
    };

    [Serializable]
    public struct DifficultyValues
    {
        [FormerlySerializedAs("Difficulty")] public GameDifficulty difficulty;
        [FormerlySerializedAs("SectionsRange")] [Tooltip("The random range of sections that the amount of sections can appear in a level")]
        public Vector2Int sectionsRange;
        [FormerlySerializedAs("Threshold")] [Tooltip("The target threshold that this difficulty will be registered as the new difficulty. Values cross from min difficulty to max difficulty")]
        public Vector2 threshold;
        [FormerlySerializedAs("Levels")] [Tooltip("The amount of levels expected for the player to complete under this difficulty")]
        public int levels;
    }
    
    /// <summary>
    /// Get sections from this difficulties section range count based on the difficulty
    /// </summary>
    /// <param name="difficulty">The current difficulty of the game</param>
    /// <returns></returns>
    public int GetSectionCount(DifficultyValues values, float difficulty)
    {
        return Mathf.RoundToInt
            (Mathf.Lerp(values.sectionsRange.x, values.sectionsRange.y, difficulty / maxDifficulty));
    }
}