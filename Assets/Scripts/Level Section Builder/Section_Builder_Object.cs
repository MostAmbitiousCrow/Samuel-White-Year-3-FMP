using System;
using EditorAttributes;
using UnityEngine;

public abstract class Section_Builder_Object : MonoBehaviour, ISection_Data
{
    [Line(GUIColor.White, alpha: 1, lineThickness: 10)]
    [Range(0, 2), SerializeField] int lane;
    [Range(0, 100), SerializeField] int distance;
    [Range(0, 5), SerializeField] int height;

    // ISection_Object Variables
    public int Lane { get => lane; set => lane = value; }
    public int Distance { get => distance; set => distance = value; }
    public int Height { get => height; set => height = value; }

    [Header("Debug")]
    [SerializeField] bool enableSnapping = true;

#if UNITY_EDITOR
    public void DrawGizmos()
    {
        SnapToLane();
        AdditionalDebug();
    }

    protected void DrawItem(Color color, Vector3 scaleVector)
    {
        Gizmos.color = color;
        Gizmos.DrawCube(transform.position, scaleVector);
    }

    void SnapToLane()
    {
        if (enableSnapping) transform.position = new((Lane - 1) * 5f, Height, Distance);
    }
#endif

    /// <summary>
    /// Called to retrieve or assign object data from the provided Section_Content.
    /// Override this method in derived classes to implement custom data handling logic.
    /// </summary>
    /// <param name="sc">The Section_Content containing relevant data for this object.</param>

    public abstract void Register(Section_Content section);

    protected abstract void AdditionalDebug();
}

public enum ObjectType
{
    Obstacle = 0,
    Enemy = 1,
    Collectible = 2
}
