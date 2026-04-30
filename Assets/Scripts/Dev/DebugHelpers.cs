using UnityEngine;

public class DebugHelpers
{
    public static void DebugDrawBox(Vector3 center, Vector3 halfExtents, Quaternion rotation, Color color)
    {
        var right = rotation * Vector3.right * halfExtents.x;
        var up = rotation * Vector3.up * halfExtents.y;
        var forward = rotation * Vector3.forward * halfExtents.z;

        Vector3[] corners =
        {
            center + right + up + forward,
            center + right + up - forward,
            center + right - up + forward,
            center + right - up - forward,
            center - right + up + forward,
            center - right + up - forward,
            center - right - up + forward,
            center - right - up - forward
        };

        DrawLine(0,1); DrawLine(0,2); DrawLine(0,4);
        DrawLine(7,5); DrawLine(7,6); DrawLine(7,3);
        DrawLine(1,5); DrawLine(1,3);
        DrawLine(2,3); DrawLine(2,6);
        DrawLine(4,5); DrawLine(4,6);
        return;

        // Edges
        void DrawLine(int a, int b) => Debug.DrawLine(corners[a], corners[b], color);
    }
}
