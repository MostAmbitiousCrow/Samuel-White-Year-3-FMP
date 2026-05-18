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
        
        [Serializable]
        public class SectionObstacleData
        {
            public Section_Obstacle_Object.SectionObstacleData data;

            public int lane;
            public int distance;
            public int height;
        }
        
        [Serializable]
        public class SectionEnemyData
        {
            public Section_Enemy_Object.SectionEnemyData data;
                
            public int lane;
            public int distance;
            public int height;
        }
        
        [Serializable]
        public class SectionCollectibleData
        {
            public Section_Collectible_Object.SectionCollectibleData data;
                
            public int lane;
            public int distance;
            public int height;
        }
        
        [Serializable]
        public class SectionGemstoneGateData
        {
            public Section_Gemstone_Gate.SectionGemstoneGateData data;
                
            public int lane;
            public int distance;
            public int height;
        }
        
        [Serializable]
        public class SectionSlipStreamData
        {
            public Section_SlipStream_Object.SectionSlipStreamData data;
                
            public int lane;
            public int distance;
            public int height;
        }
        
        /// <summary> What difficulty this section qualifies as. Setting difficulty as none will cause this
        /// section to be ignored. </summary>
        [Flags]
        public enum DifficultyQualification { Easy = 1, Medium = 2, Hard = 4 }
        /// <summary> Determines the difficulty considered for this section </summary>
        public DifficultyQualification difficultyType;
    }
}
