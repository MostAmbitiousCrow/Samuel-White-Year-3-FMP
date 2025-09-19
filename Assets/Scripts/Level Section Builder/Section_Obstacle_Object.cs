/// <summary>
/// Section Object derived class that shares overrided stats based on Obstacle objects
/// </summary>
public class Section_Obstacle_Object : Section_Builder_Object
{

    [System.Serializable]
    public class ObstacleData
    {
        public ObstacleType obstacleType;

        // Override Stats?
        public bool overrideStats;

        [System.Serializable]
        public class ObstacleOverrideStats
        {
            public float ImpactDamage;
        }
        [EditorAttributes.ShowField(nameof(overrideStats))] public ObstacleOverrideStats overridedStats;
    }

    [EditorAttributes.Line(EditorAttributes.GUIColor.Cyan)]
    /// <summary>
    /// The data of the obstacle shared with the Game_Section_Manager and Section_Builder
    /// </summary>
    public ObstacleData data;

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
        name = new($"{ObjectType} - {data.obstacleType}");
    }
}
