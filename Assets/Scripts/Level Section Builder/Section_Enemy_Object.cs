using UnityEngine;

/// <summary>
/// Section Object derived class that shares overrided stats based on Enemy objects
/// </summary>
public class Section_Enemy_Object : Section_Builder_Object
{
    [System.Serializable]
    public class SectionEnemyData
    {
        public EnemyType enemyType;
        [Tooltip("Should the enemy silhouette always be visible, or only when the enemy is emerging?")]
        public bool silhouetteAlwaysVisible = true;

        // Override Stats?
        public bool overrideData;
        [EditorAttributes.ShowField(nameof(overrideData))] public EnemyData overridedData;
    }

    [EditorAttributes.Line(EditorAttributes.GUIColor.Red)]
    /// <summary>
    /// The data of the enemy shared with the Game_Section_Manager and Section_Builder
    /// </summary>
    public SectionEnemyData sectionData;

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
        DrawItem(Color.red, Vector3.one);
        name = new($"{ObjectType.Enemy} - {sectionData.enemyType}");
    }
}
