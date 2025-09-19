/// <summary>
/// Section Object derived class that shares overrided stats based on Obstacle objects
/// </summary>
public class Section_Collectible_Object : Section_Builder_Object
{
    [System.Serializable]
    public class CollectibleData
    {
        public CollectibleType collectibleType;

        // Override Stats?
        public bool overrideStats;

        [System.Serializable]
        public class CollectibleOverrideStats
        {
            public int BankValue;
            [EditorAttributes.ShowField(nameof(collectibleType), CollectibleType.Gemstone)] public int Amount;
        }

        [EditorAttributes.ShowField(nameof(overrideStats))] public CollectibleOverrideStats overridedStats;
    }
    [EditorAttributes.Line(EditorAttributes.GUIColor.Yellow)]
    /// <summary>
    /// The data of the collectible shared with the Game_Section_Manager and Section_Builder
    /// </summary>
    public CollectibleData data;

    public enum CollectibleType
    {
        Gemstone, GemstoneFragment
    }

    public override void Register(Section_Content section)
    {
        section.sectionData.CollectibleDatas.Add(this);
        section.sectionData.SectionBuilderDatas.Add(this);
    }

    protected override void AdditionalDebug()
    {
        name = new($"{ObjectType} - {data.collectibleType}");
    }
}
