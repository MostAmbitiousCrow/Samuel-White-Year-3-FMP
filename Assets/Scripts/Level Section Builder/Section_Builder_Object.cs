using EditorAttributes;
using UnityEngine;

public abstract class SectionBuilderObject : MonoBehaviour, ISectionData
{
    [Title("River Object for Building Objects for Level Sections", 11)]
    [Line(GUIColor.White, alpha: 1, lineThickness: 10)]
    [Range(0, 2), SerializeField] int lane;
    [Range(0, 100), SerializeField] int distance;
    [Range(0, 5), SerializeField] int height;

    // ISectionData Variables
    public int Lane { get => lane; set => lane = value; }
    public int Distance { get => distance; set => distance = value; }
    public int Height { get => height; set => height = value; }

    [Header("Debug")]
    [SerializeField] bool enableSnapping = true;

    [Header("Data")]
    // [SerializeField] private GlobalRiverValues globalRiverValues;

    [SerializeField] private River_Manager riverManager;

    public void DrawGizmos()
    {
        SnapToLane();
        AdditionalDebug();
    }

    public void InjectRiverManager(River_Manager rm)
    {
        riverManager = rm;
    }

    protected void DrawItem(Color color, Vector3 scaleVector)
    {
        Gizmos.color = color;
        Gizmos.DrawCube(transform.position, scaleVector);
    }

    private void SnapToLane()
    {
        if (!enableSnapping)
        {
            return;
        }

        riverManager.AssignToCurveSection(Distance, lane, out Vector3 pos, out Quaternion rot);

        pos += (transform.right * (lane - 1)) * GlobalRiverValues.RiverLaneDistance; //TODO: Assign this to AssignToCurveSection
        pos += Vector3.up * height;
        transform.SetLocalPositionAndRotation(pos, rot);
    }

    private void CurveOffset()
    {

    }

    /// <summary>
    /// Called to retrieve or assign object data from the provided Section_Content.
    /// Override this method in derived classes to implement custom data handling logic.
    /// </summary>
    /// <param name = "section" > The Section_Content containing relevant data for this object.</param>

    public abstract void Register(SectionContentBuilder section);

    protected abstract void AdditionalDebug();
    protected abstract void AdditionalDebugSelected();

    private void OnDrawGizmosSelected()
    {
        AdditionalDebugSelected();
    }
}

public enum ObjectType
{
    Obstacle = 0,
    Enemy = 1,
    Collectible = 2
}
