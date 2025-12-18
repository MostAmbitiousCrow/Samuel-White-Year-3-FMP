using UnityEngine;

/// <summary>
/// Section Object derived class that shares overrided stats based on Obstacle objects
/// </summary>
public class Section_Collectible_Object : Section_Builder_Object
{
    
    [System.Serializable]
    public class SectionCollectibleData
    {
        public CollectibleType collectibleType;

        // Override Stats?
        public bool overrideData;
        [EditorAttributes.ShowField(nameof(overrideData))] public CollectibleData overridedData;
    }
    [EditorAttributes.Line(EditorAttributes.GUIColor.Yellow)]
    /// <summary>
    /// The data of the collectible shared with the Game_Section_Manager and Section_Builder
    /// </summary>
    public SectionCollectibleData sectionData;

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
        DrawItem(Color.yellow, Vector3.one);
        name = new($"{ObjectType.Collectible} - {sectionData.collectibleType}");
    }

    protected override void AdditionalDebugSelected()
    {
        return;
    }
}
