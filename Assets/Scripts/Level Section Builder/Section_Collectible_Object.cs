using UnityEngine;

/// <summary>
/// Section Object derived class that shares overrided stats based on Obstacle objects
/// </summary>
public class Section_Collectible_Object : SectionBuilderObject
{
    
    [System.Serializable]
    public class SectionCollectibleData
    {
        public CollectibleType collectibleType;

        // Override Stats?
        public bool overrideData;
        [EditorAttributes.ShowField(nameof(overrideData))] public CollectibleData overridedData;
    }
        /// <summary>
        /// The data of the collectible shared with the Game_Section_Manager and Section_Builder
        /// </summary>
    [EditorAttributes.Line(EditorAttributes.GUIColor.Yellow)]

    public SectionCollectibleData sectionData;

    public enum CollectibleType
    {
        Gemstone, GemstoneFragment
    }

    public override void Register(SectionContentBuilder section)
    {
        section.sectionData.collectibleDatas.Add(this);
        section.sectionData.SectionBuilderDatas.Add(this);
    }

    protected override void AdditionalDebug()
    {
        DrawItem(Color.yellow, Vector3.one, transform.position);
        name = new($"{ObjectType.Collectible} - {sectionData.collectibleType}");
    }

    protected override void AdditionalDebugSelected()
    {
        return;
    }
}
