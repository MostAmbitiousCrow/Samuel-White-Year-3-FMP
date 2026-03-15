using UnityEngine;
using EditorAttributes;
using UnityEngine.Serialization;

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
        DrawItem(Color.cyan, Vector3.one);
        name = new($"{ObjectType.Obstacle} - {sectionData.obstacleType}");

        if (sectionData.obstacleType == ObstacleType.SewerPipe)
        {
            // TODO: Draw boxes displaying the connection of pipes
        }
    }

    protected override void AdditionalDebugSelected()
    {
        return;
    }
}
