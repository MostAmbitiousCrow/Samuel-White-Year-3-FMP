using System;
using System.Collections.Generic;
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
    [Space]
    [SerializeField] private List<Vector3> points;

    /// <summary>
    /// The value used for distancing the spacing between each pipe.
    /// Value is based on the size of the artwork that's being distributed.
    /// </summary>
    public static readonly float PipeLength = 1.6f;

    public void AssignPipeData(NewPipeObstacleData data)
    {
        pipeData = data;
    }

    // TODO: Currently doesn't work :(
    private void OnCollisionEnter(Collision other)
    {
        if (IsHit) return;
        // print($"{name} hit: {other.name}");

        if (other.collider.TryGetComponent<IDamageable>(out var character))
            character.TakeDamage(DamageType.Standard, obstacleData.ImpactDamage);
        if (other.collider.CompareTag("Boat"))
            other.collider.GetComponent<Boat_Controller>().TakeDamage();
        IsHit = true;

        if (explodesOnHit) artExploder.ExplodeArt();
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

        foreach (var spline in splines)
        {
            splineContainer.AddSpline(spline);
        }

        splineMesher.Rebuild();
        Debug.Log("Pipe Obstacle Built!");
    }
    
    private List<Vector3> BuildPipePoints(NewPipeObstacleData data, River_Manager rm, Transform t)
    {
        var points = new List<Vector3>();

        // ==== Start pipe ====
        var start = data.connectedPipes[0];
        points.Add(GetPipePosition(start, rm, t));

        // ==== Joints ====
        foreach (var joint in data.pipeJoints) points.Add(GetPipePosition(joint, rm, t));

        // ==== End pipe ====
        var end = data.connectedPipes[1];
        points.Add(GetPipePosition(end, rm, t));

        return points;
    }

    private Vector3 GetPipePosition(NewPipeObstacleData.PipeData pipe, River_Manager rm, Transform t)
    {
        // rm.AssignToCurveSection(pipe.distance, pipe.lane, out var pos, out var rot);
        var pos = new Vector3();
        pos += t.right * (pipe.lane - 1) * GlobalRiverValues.RiverLaneDistance; //* 1.6f;
        pos += Vector3.up * pipe.height;
        pos += transform.forward * pipe.distance;
        
        return pos;
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
