using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using EditorAttributes;
using sc.modeling.splines.runtime;
using Unity.Mathematics;
using UnityEngine.Splines;

public class River_PipeObstacle_New : River_Obstacle
{
    [Line]
    [Header("Pipe Data")]
    [SerializeField] private SplineMesher splineMesher;
    public NewPipeObstacleData pipeData;
    [Space]
    [SerializeField] private List<Vector3> points;
    
    [Header("Pipe Detection")]
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private float pipeSize = 1f;

    /// <summary>
    /// The value used for distancing the spacing between each pipe.
    /// Value is based on the size of the artwork that's being distributed.
    /// </summary>
    public static readonly float PipeLength = 1.6f;

    #region Pipe Generation
    public void AssignPipeData(NewPipeObstacleData data)
    {
        pipeData = data;
    }

    protected override void OnObjectPlaced()
    {
        base.OnObjectPlaced();

        var splineContainer = splineMesher.splineContainer;
        splineContainer.Splines = new List<Spline>();

        // Build the pipe path using shared logic
        points = BuildPipePoints(pipeData, River_Manager.Instance, transform);

        // Create splines
        
        var splines = new List<Spline>();
        for (int i = 0; i < points.Count - 1; i++)
        {
            var spline = new Spline
            {
                new BezierKnot
                {
                    Position = points[i],
                    Rotation = quaternion.identity
                },
                new BezierKnot
                {
                    Position = points[i + 1],
                    Rotation = quaternion.identity
                }
            };
            spline.SetTangentMode(TangentMode.AutoSmooth);
            splines.Add(spline);
        }

        foreach (var spline in splines) splineContainer.AddSpline(spline);

        splineMesher.Rebuild();
    }
    
    private List<Vector3> BuildPipePoints(NewPipeObstacleData data, River_Manager rm, Transform t)
    {
        points = new List<Vector3>();

        // ==== Start pipe ====
        var start = data.connectedPipes[0];
        points.Add(GetPipePosition(start));

        // ==== Joints ====
        foreach (var joint in data.pipeJoints) points.Add(GetPipePosition(joint));
        
        // ==== End pipe ====
        var end = data.connectedPipes[1];
        points.Add(GetPipePosition(end));
        
        return points;
    }

    private Vector3 GetPipePosition(NewPipeObstacleData.PipeData pipe)
    {
        float baseDistance = startDistance;
        riverManager.AssignToCurveSection(baseDistance + pipe.distance, pipe.lane, out var pos, out var rot);
        pos += Vector3.up * pipe.height;
        pos = transform.InverseTransformPoint(pos);

        return pos;
    }
    #endregion

    #region Detection

    private void FixedUpdate()
    {
        if (IsHit) return;

        for (int i = 0; i < points.Count-1; i++)
        {
            var pointA = transform.TransformPoint(points[i]);
            var pointB = transform.TransformPoint(points[i + 1]);

            CastPipeDetection(pointA, pointB);
        }
    }

    private void CastPipeDetection(Vector3 pointA, Vector3 pointB)
    {
        var segment = pointB - pointA;
        var length = segment.magnitude;

        // Prevent Mesh Infinity Error
        if (length <= 0.001f) return;

        var direction = segment.normalized;
        var centre = pointA + segment * 0.5f;
        var rotation = Quaternion.LookRotation(direction);
        
        const float padding = 0.1f;

        var halfExtents = new Vector3(pipeSize + padding, pipeSize + padding, length * 0.5f);

        var origin = centre - direction * 0.01f;
        const float maxDistance = 0.02f;

        // TODO: Overlap Boxes aren't at the correct position...
        var results = Array.Empty<Collider>();
        Physics.OverlapBoxNonAlloc(centre, halfExtents, results, rotation, layerMask);

        if (results.Length > 0)
        {
            DebugDrawBox(centre, halfExtents, rotation, Color.green);
            OnHit(results.First().gameObject);
            Debug.Log($"Drawing Boxcast at centre: {centre}. Hit = {results.First().gameObject}");
        }
        else
        {
            DebugDrawBox(centre, halfExtents, rotation, Color.red);
        }
        Debug.Log($"Drawing Boxcast at centre: {centre}");
    }

    
    private void DebugDrawBox(Vector3 center, Vector3 halfExtents, Quaternion rotation, Color color)
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

        L(0,1); L(0,2); L(0,4);
        L(7,5); L(7,6); L(7,3);
        L(1,5); L(1,3);
        L(2,3); L(2,6);
        L(4,5); L(4,6);
        return;

        // Edges
        void L(int a, int b) => Debug.DrawLine(corners[a], corners[b], color);
    }
    #endregion
}

[Serializable]
public class NewPipeObstacleData
{
    /// <summary>Represents which side of the sewer the pipe will connect to, or none at all</summary>
    public enum PipeConnection { None, Floor, LeftWall, RightWall, Ceiling }
    
    [Serializable]
    public class ConnectedPipeData : PipeData
    {
        public PipeConnection pipeConnection;
    }
    // Each pipes start-point needs to be set on a given river lane
    // Each pipes end-point can then be set to connect to a given (different) lane, distance and height.
    
    /// <summary>
    /// The base class for pipes that determines what lane the pipe will target,
    /// the height and distance it will project forward
    /// </summary>
    [Serializable]
    public class PipeData
    {
        [Range(0, 2)] public int lane;
        [Range(0, 5)] public int height;
        [Range(0, 100)] public int distance;
    }

    public ConnectedPipeData[] connectedPipes = new ConnectedPipeData[2];
    public PipeData[] pipeJoints;
}
