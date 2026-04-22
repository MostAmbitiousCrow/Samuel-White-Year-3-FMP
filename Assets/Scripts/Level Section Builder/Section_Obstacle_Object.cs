using System;
using UnityEngine;
using EditorAttributes;
using UnityEngine.Serialization;
using Vector3 = UnityEngine.Vector3;

/// <summary>
/// Section Object derived class that shares overrided stats based on Obstacle objects
/// </summary>
public class Section_Obstacle_Object : SectionBuilderObject
{
    [System.Serializable]
    public class SectionObstacleData
    {
        public ObstacleType obstacleType;

        // Override Stats?
        public bool overrideData;

        [FormerlySerializedAs("overridedData")] 
        [ShowField(nameof(overrideData))] public ObstacleData overriddenData = new();

        // //TODO: Remember to delete these two variables
        // Pipe Data
        [ShowField(nameof(obstacleType), ObstacleType.SewerPipe)]
        public NewPipeObstacleData pipeObstacleData = new();
        
        // Bridge Data
        [ShowField(nameof(obstacleType), ObstacleType.Bridge)]
        public ObstacleBridgeData obstacleBridgeData = new();
    }

    /// <summary> The data of the obstacle shared with the Game_Section_Manager and Section_Builder </summary>
    [Line(GUIColor.Cyan)]
    public SectionObstacleData sectionData;

    public enum ObstacleType
    {
        TrashPile, WideTrashPile, SewerPipe, Bridge
    }

    public override void Register(SectionContentBuilder section)
    {
        section.sectionData.obstacleDatas.Add(this);
        section.sectionData.SectionBuilderDatas.Add(this);
    }

    protected override void AdditionalDebug()
    {
        DrawItem(Color.cyan, Vector3.one, transform.position);
    }

    protected override void AdditionalDebugSelected()
    {
        return;
    }
}
