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
        points = BuildPipePoints();

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
    
    private List<Vector3> BuildPipePoints()
    {
        points = new List<Vector3>();
        
        // === Start Surface Connection Pipe ===
        var startPipe = pipeData.connectedPipes[0];
        if (startPipe.pipeConnection != NewPipeObstacleData.PipeConnection.None)
            points.Add(GetPointConnection(startPipe));

        // ==== Start pipe ====
        points.Add(GetPipePosition(startPipe));

        // ==== Joints ====
        foreach (var joint in pipeData.pipeJoints) points.Add(GetPipePosition(joint));
        
        // ==== End pipe ====
        var endPipe = pipeData.connectedPipes[1];
        points.Add(GetPipePosition(endPipe));
        
        // === Start Surface Connection Pipe ===
        if (endPipe.pipeConnection != NewPipeObstacleData.PipeConnection.None)
            points.Add(GetPointConnection(endPipe));

        return points;
    }

    private Vector3 GetPointConnection(NewPipeObstacleData.ConnectedPipeData data)
    {
        var point = GetPipePosition(data);
        point += data.pipeConnection switch
        {
            NewPipeObstacleData.PipeConnection.None => throw new ArgumentOutOfRangeException(),
            NewPipeObstacleData.PipeConnection.Floor => Vector3.down * (GlobalRiverValues.FloorToLaneDistance + point.y),
            NewPipeObstacleData.PipeConnection.LeftWall => Vector3.left * GlobalRiverValues.WallToLaneDistance,
            NewPipeObstacleData.PipeConnection.RightWall => Vector3.right * GlobalRiverValues.WallToLaneDistance,
            NewPipeObstacleData.PipeConnection.Ceiling => Vector3.up * (GlobalRiverValues.CeilingToLaneDistance + point.y),
            _ => throw new ArgumentOutOfRangeException()
        };
        return point;
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

    protected override void FixedTimeUpdate()
    {
        base.FixedTimeUpdate();
        if (IsHit) return;

        for (int i = 0; i < points.Count-1; i++)
        {
            var pointA = transform.TransformPoint(points[i]);
            var pointB = transform.TransformPoint(points[i + 1]);

            CastPipeDetection(pointA, pointB);
        }
    }

    private readonly Collider[] _results = new Collider[4];

    private void CastPipeDetection(Vector3 pointA, Vector3 pointB)
    {
        var segment = pointB - pointA;
        var length = segment.magnitude;

        if (length <= 0.001f) return;

        var direction = segment.normalized;
        var centre = pointA + segment * 0.5f;
        var rotation = Quaternion.LookRotation(direction, Vector3.up);

        const float padding = 0.1f;
        var halfExtents = new Vector3(pipeSize + padding, pipeSize + padding, length * 0.5f);

        int hitCount = Physics.OverlapBoxNonAlloc(centre, halfExtents, _results, rotation, layerMask);

        if (hitCount > 0)
        {
            #if UNITY_EDITOR
            DebugHelpers.DebugDrawBox(centre, halfExtents, rotation, Color.green);
            #endif

            for (int i = 0; i < hitCount; i++)
            {
                OnHit(_results[i].gameObject);
                Debug.Log($"Hit: {_results[i].gameObject}");
            }
        }
        #if UNITY_EDITOR
        else DebugHelpers.DebugDrawBox(centre, halfExtents, rotation, Color.red);
        #endif
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
