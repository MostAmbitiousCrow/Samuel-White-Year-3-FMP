using UnityEngine;
using EditorAttributes;

/// <summary>
/// Section Object derived class that shares overrided stats based on the Gemstone Gates
/// </summary>
public class Section_Gemstone_Gate : Section_Builder_Object
{
    [System.Serializable]
    public class SectionGemstoneGateData
    {
        // Override Stats?
        public bool overrideData;
        [ShowField(nameof(overrideData))] public GemstoneGateData overridedData;
    }
    [Line(GUIColor.White)]
    /// <summary>
    /// The data of the obstacle shared with the Game_Section_Manager and Section_Builder
    /// </summary>
    public SectionGemstoneGateData sectionData;


    public override void Register(Section_Content section)
    {
        section.sectionData.GemstoneGateDatas.Add(this);
        section.sectionData.SectionBuilderDatas.Add(this);
    }

    protected override void AdditionalDebug()
    {
        DrawItem(Color.white, new(4f, 6f, 1f));
        name = new($"---Gemstone Gate---({sectionData.overridedData.GemRequirement})");
    }
}
