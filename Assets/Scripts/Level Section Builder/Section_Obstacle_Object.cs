using System;
using System.Numerics;
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

        // Pipe Data
        [ShowField(nameof(obstacleType), ObstacleType.SewerPipe)] public Pipe_Obstacle_Data pipeObstacleData = new();
    }

    /// <summary> The data of the obstacle shared with the Game_Section_Manager and Section_Builder </summary>
    [Line(GUIColor.Cyan)]
    public SectionObstacleData sectionData;

    public enum ObstacleType
    {
        TrashPile, WideTrashPile, SewerPipe
    }

    public override void Register(SectionContentBuilder section)
    {
        section.sectionData.obstacleDatas.Add(this);
        section.sectionData.SectionBuilderDatas.Add(this);
    }

    protected override void AdditionalDebug()
    {
        DrawItem(Color.cyan, Vector3.one, transform.position);
        name = new($"{ObjectType.Obstacle} - {sectionData.obstacleType}");
        
        if (sectionData.obstacleType != ObstacleType.SewerPipe) return;
        
        // Do Sewer Pipe specific Visual Debugging
        
        // TODO: Draw boxes displaying the connection of pipes
        var data = sectionData.pipeObstacleData;
        for (int i = 0; i < data.amount; i++)
        {
            var direction = data.pipeConnection switch
            {
                Pipe_Obstacle_Data.PipeConnection.Left => Vector3.left,
                Pipe_Obstacle_Data.PipeConnection.Top => Vector3.up,
                Pipe_Obstacle_Data.PipeConnection.Bottom => Vector3.down,
                Pipe_Obstacle_Data.PipeConnection.Right => Vector3.right,
                _ => Vector3.zero
            };
            var position = transform.position + i * (direction * data.distancePerPipe);
            DrawItem(Color.cyan, Vector3.one, position);
        }
    }

    protected override void AdditionalDebugSelected()
    {
        return;
    }
}
