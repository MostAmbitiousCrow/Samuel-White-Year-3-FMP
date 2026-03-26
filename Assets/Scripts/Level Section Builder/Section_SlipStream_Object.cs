using UnityEngine;

public class Section_SlipStream_Object : SectionBuilderObject
{
    [System.Serializable]
    public class SectionSlipStreamData
    {
        // Override Stats?
        public bool overrideData;
        [EditorAttributes.ShowField(nameof(overrideData))] public River_SlipStream.SlipStreamData overridedData;
    }
    /// <summary>
    /// The data of the collectible shared with the Game_Section_Manager and Section_Builder
    /// </summary>
    [EditorAttributes.Line(EditorAttributes.GUIColor.White)]

    public SectionSlipStreamData sectionData;

    public override void Register(SectionContentBuilder section)
    {
        section.sectionData.slipStreamDatas.Add(this);
        section.sectionData.SectionBuilderDatas.Add(this);
    }

    protected override void AdditionalDebug()
    {
        DrawItem(Color.gray, Vector3.one, transform.position);
        name = new($"Slip Stream - ({sectionData.overridedData.speedIncreaseAmount})");
    }

    protected override void AdditionalDebugSelected()
    {
        return;
    }
}
