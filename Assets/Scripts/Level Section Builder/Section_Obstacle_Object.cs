using UnityEngine;
using EditorAttributes;

/// <summary>
/// Section Object derived class that shares overrided stats based on Obstacle objects
/// </summary>
public class Section_Obstacle_Object : Section_Builder_Object
{
    [System.Serializable]
    public class SectionObstacleData
    {
        public ObstacleType obstacleType;

        // Override Stats?
        public bool overrideData;

        [ShowField(nameof(overrideData))] public ObstacleData overridedData = new();

        // Pipe Data
        [ShowField(nameof(obstacleType), ObstacleType.SewerPipe)] public Pipe_Obstacle_Data pipeObstacleData = new();
    }

    [Line(GUIColor.Cyan)]
    /// <summary>
    /// The data of the obstacle shared with the Game_Section_Manager and Section_Builder
    /// </summary>
    public SectionObstacleData sectionData;

    public enum ObstacleType
    {
        TrashPile, SewerPipe
    }

    public override void Register(Section_Content section)
    {
        section.sectionData.ObstacleDatas.Add(this);
        section.sectionData.SectionBuilderDatas.Add(this);
    }

    protected override void AdditionalDebug()
    {
        DrawItem(Color.cyan, Vector3.one);
        name = new($"{ObjectType.Obstacle} - {sectionData.obstacleType}");
    }

    protected override void AdditionalDebugSelected()
    {
        return;
    }
}
