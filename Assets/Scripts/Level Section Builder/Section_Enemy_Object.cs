using UnityEngine;
using EditorAttributes;

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
        [ShowField(nameof(overrideData))] public BoatEnemy_Data overridedData;
    }

    [Line(GUIColor.Red)]
    /// <summary>
    /// The data of the enemy shared with the Game_Section_Manager and Section_Builder
    /// </summary>
    public SectionEnemyData sectionData;

    public enum EnemyType
    {
        Crocodile, Frog, Bat, Tentacle
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

    protected override void AdditionalDebugSelected()
    {
        // 
        Gizmos.color = Color.white;
        BoatEnemy_Data data = sectionData.overridedData;
        Gizmos.DrawSphere(Boat_Space_Manager.Instance.GetSideSpace(data.targetSideSpace, data.targetLeftSide).t.position, .5f);

        // Draw targeted boat space
        Gizmos.color = Color.black;
        data = sectionData.overridedData;
        Gizmos.DrawSphere(Boat_Space_Manager.Instance.GetSpace(data.targetBoatSide, data.targetSpace).t.position, .25f);
    }
}
