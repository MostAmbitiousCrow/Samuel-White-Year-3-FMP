using System;
using System.Collections.Generic;
using EditorAttributes;
using UnityEngine;

/// <summary>
/// 
/// </summary>

/*
 * This is the scriptable object data that is inserted into the Game Section Manager in the main game scene.
 * It contains the data obtained from the Section_Content from the level editor.
 *
 * Additionally, you can choose to change the colours of the game environment.
 * If left null, the colours won't be changed.
 */
[CreateAssetMenu (fileName = "Level Section Data",  menuName = "ScriptableObjects/Level Section Data", order = 1)]
public class SO_SectionData : ScriptableObject
{
    /// <summary> The section content data storing data for what objects will appear in this section. </summary>
    [Header("Section Data")]
    public SectionContent sectionContent;

    /// <summary> </summary>
    [Serializable]
    public struct SectionContent
    {
        public float initialDelay;
        public float postDelay;

        public int ObjectCount => obstacles.Count + enemies.Count + collectibles.Count + gemstoneGates.Count;

        [Line(GUIColor.Orange)] public List<SectionObstacleData> obstacles;
        [Line(GUIColor.Red)] public List<SectionEnemyData> enemies;
        [Line(GUIColor.Cyan)] public List<SectionCollectibleData> collectibles;

        [Line(GUIColor.White)] public List<SectionGemstoneGateData> gemstoneGates;

        [Line(GUIColor.Gray)] public List<SectionSlipStreamData> slipStreams;
        
        public abstract class SectionData
        {
            public int lane;
            public int distance;
            public int height;
        }
        [Serializable]
        public class SectionObstacleData : SectionData
        {
            public Section_Obstacle_Object.SectionObstacleData data;
        }
        
        [Serializable]
        public class SectionEnemyData : SectionData
        {
            public Section_Enemy_Object.SectionEnemyData data;
        }
        
        [Serializable]
        public class SectionCollectibleData : SectionData
        {
            public Section_Collectible_Object.SectionCollectibleData data;
        }
        
        [Serializable]
        public class SectionGemstoneGateData : SectionData
        {
            public Section_Gemstone_Gate.SectionGemstoneGateData data;
        }
        
        [Serializable]
        public class SectionSlipStreamData : SectionData
        {
            public Section_SlipStream_Object.SectionSlipStreamData data;
        }
        /// <summary> Determines the difficulty is considered to be for this section </summary>
        public DifficultyQualification difficultyType;

        [Flags]
        public enum AvailableEnvironments
        {
            None = 0, Sewer = 1, Pyramid = 2, Cave = 4, Forest = 8, Dungeon = 16
        }
        public AvailableEnvironments applicableEnvironments;
    }
}
