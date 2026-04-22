using System;
using sc.modeling.splines.runtime;
using UnityEngine;
using EditorAttributes;
using Unity.Mathematics;
using UnityEngine.Splines;

public class River_PipeObstacle_New : River_Obstacle
{
    [Line]
    [Header("Pipe Data")]
    [SerializeField] private SplineMesher splineMesher;
    public NewPipeObstacleData pipeData;

    /// <summary>
    /// The value used for distancing the spacing between each pipe.
    /// Value is based on the size of the artwork that's being distributed.
    /// </summary>
    public static readonly float PipeLength = 1.6f;

    public void AssignPipeData(NewPipeObstacleData data)
    {
        pipeData = data;
    }

    protected override void OnObjectPlaced()
    {
        base.OnObjectPlaced();
        
        var splineContainer = splineMesher.splineContainer;
        
        // === Set the starting connection ===
        var connectedPipe = pipeData.connectedPipes[0];
        Spline spline = new Spline();
        
        // Add starting Knot to spline
        var knot = new BezierKnot
        {
            Position = new float3()
        };
        spline.Add(knot);
        
        splineContainer.AddSpline();
        
        // Add end knot to spline
        River_Manager.Instance.AssignToCurveSection
            (startDistance + connectedPipe.distance, connectedPipe.lane, out Vector3 pos, out Quaternion rot);
        knot = new BezierKnot
        {
            Position = pos + Vector3.up * connectedPipe.height,
            Rotation = rot
        };
        spline.Add(knot);
        
        // === Build the pipe joints (if available) ===

        if (pipeData.pipeJoints.Length > 0)
        {
            
        }
        
        // === Set the end connection ===
        
        
        // Rebuild the mesh
        splineMesher.Rebuild();
    }
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
