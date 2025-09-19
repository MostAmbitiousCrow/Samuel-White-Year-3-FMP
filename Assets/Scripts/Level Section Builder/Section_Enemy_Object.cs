/// <summary>
/// Section Object derived class that shares overrided stats based on Enemy objects
/// </summary>
public class Section_Enemy_Object : Section_Builder_Object
{
    [System.Serializable]
    public class EnemyData
    {
        public EnemyType enemyType;
        [UnityEngine.Tooltip("Should the enemy silhouette always be visible, or only when the enemy is emerging?")]
        public bool silhouetteAlwaysVisible = true;

        // Override Stats?
        public bool overrideStats;

        [System.Serializable]
        public class EnemyOverrideStats
        {
            public int Health;
            public float EmergeTime;
        }

        [EditorAttributes.ShowField(nameof(overrideStats))] public EnemyOverrideStats overridedStats;
    }

    [EditorAttributes.Line(EditorAttributes.GUIColor.Red)]
    /// <summary>
    /// The data of the enemy shared with the Game_Section_Manager and Section_Builder
    /// </summary>
    public EnemyData data;

    public enum EnemyType
    {
        Crocodile, Worm
    }

    public override void Register(Section_Content section)
    {
        section.sectionData.EnemyDatas.Add(this);
        section.sectionData.SectionBuilderDatas.Add(this);
    }

    protected override void AdditionalDebug()
    {
        name = new($"{ObjectType} - {data.enemyType}");
    }
}
