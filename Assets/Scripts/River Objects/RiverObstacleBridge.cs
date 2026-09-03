using System;
using System.Collections.Generic;
using EditorAttributes;
using sc.modeling.splines.runtime;
using UnityEngine;
using UnityEngine.Splines;
using Random = UnityEngine.Random;

public class RiverObstacleBridge : River_Obstacle
{
    [Header("Bridge Data")]
    [SerializeField] private SplineMesher splineMesher;
    [SerializeField, ReadOnly] private ObstacleBridgeData obstacleBridgeData;
    [SerializeField] private GameObject[] leftArtwork, rightArtwork;
    private readonly List<Vector3> _points = new List<Vector3>();
    private readonly List<Quaternion> _rots = new List<Quaternion>();
    [Header("Detection")]
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private float detectionSize = 1f;

    public void AssignObstacleData(ObstacleBridgeData data)
    {
        obstacleBridgeData = data;
    }

    protected override void FixedTimeUpdate()
    {
        base.FixedTimeUpdate();
        if (isHit && !canTakeMultipleHits || !isMoving) return;
        
        // Do Detection
        for (int i = 0; i < _points.Count-1; i++)
        {
            //TODO: improve performance by storing point values?
            var pointA = transform.TransformPoint(_points[i]);
            var pointB = transform.TransformPoint(_points[i + 1]);

            CastPipeDetection(pointA, pointB);
        }
    }

    protected override void OnObjectPlaced()
    {
        //TODO: Rework
        _points.Clear();
        _rots.Clear();
        var spline = splineMesher.splineContainer.Spline;
        spline.Clear();

        Vector3 point;
        Quaternion rota;
                                    // ==== Set Splines ====
        // --- Left Wall ---
        if (obstacleBridgeData.leftJointLane == 0)
        {
            GetBridgePosition(obstacleBridgeData.leftJointLane, out  point, out  rota);
            point.x -= GlobalRiverValues.WallToLaneDistance;
            _points.Add(point);
            _rots.Add(rota);
        }

        
        // --- Lanes ---
        // Point A
        GetBridgePosition(obstacleBridgeData.leftJointLane, out point, out rota);
        _points.Add(point);
        _rots.Add(rota);
        
        // Point B
        GetBridgePosition(obstacleBridgeData.rightJointLane, out point, out rota);
        _points.Add(point);
        _rots.Add(rota);
        
        // --- Right Wall ---
        // Connect if the joint lane meets the River Lane Count
        if (obstacleBridgeData.rightJointLane == GlobalRiverValues.RiverLaneCount-1)
        {
            GetBridgePosition(obstacleBridgeData.rightJointLane, out point, out rota);
            point.x += GlobalRiverValues.WallToLaneDistance;
            _points.Add(point);
            _rots.Add(rota);
        }
        
        // Add points to the spline
        for (int i = 0; i < _points.Count; i++)
        {
            var knot = new BezierKnot
            {
                // Position = new Vector3 (0f, 0f, _points[i].z),
                Position = _points[i],
                Rotation = _rots[i]
            };
            spline.Add(knot);
        }
        spline.SetTangentMode(TangentMode.AutoSmooth);
        
                                    // ==== Set Artwork ====
        // Choose Left Artwork
        foreach (var art in leftArtwork) art.SetActive(false);
        var rand = Random.Range(0, leftArtwork.Length+1);
        if (rand < leftArtwork.Length && obstacleBridgeData.leftJointLane == 0)
        {
            var artwork = leftArtwork[rand];
            artwork.SetActive(true);
            artwork.transform.localPosition = spline[0].Position;
            artwork.transform.localRotation = spline[0].Rotation;
        }

        // Choose Right Artwork
        foreach (var art in rightArtwork) art.SetActive(false);
        rand = Random.Range(0, rightArtwork.Length+1);
        if (rand < rightArtwork.Length && obstacleBridgeData.rightJointLane == GlobalRiverValues.RiverLaneCount-1)
        {
            var artwork = rightArtwork[rand];
            artwork.SetActive(true);
            artwork.transform.localPosition = spline[^1].Position;
            artwork.transform.localRotation = spline[^1].Rotation;
        }

                                    // === Rebuild the Spline Mesh ===
        splineMesher.Rebuild();
        
        // Remove Unnecessary Caps
        if(obstacleBridgeData.leftJointLane != 0) 
            splineMesher.startCap.DestroyInstances();
        if(obstacleBridgeData.rightJointLane != GlobalRiverValues.RiverLaneCount-1) 
            splineMesher.endCap.DestroyInstances();
    }
    
    private void GetBridgePosition(int lane, out Vector3 posi, out Quaternion rota)
    {
        riverManager.AssignToCurveSection(startDistance, lane, out var pos, out var rot);
        pos += Vector3.up * height;
        pos = transform.InverseTransformPoint(pos);

        posi = pos;
        rota = rot;
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
        var halfExtents = new Vector3(detectionSize + padding, detectionSize + padding, length * 0.5f);

        int hitCount = Physics.OverlapBoxNonAlloc(centre, halfExtents, _results, rotation, layerMask);

        if (hitCount > 0)
        {
            DebugHelpers.DebugDrawBox(centre, halfExtents, rotation, Color.green);

            for (int i = 0; i < hitCount; i++)
            {
                OnHit(_results[i].gameObject);
                Debug.Log($"Hit: {_results[i].gameObject}");
            }
        }
#if Unity
        
#endif
        else DebugHelpers.DebugDrawBox(centre, halfExtents, rotation, Color.red);
    }
}

[Serializable]
public class ObstacleBridgeData
{
    [Range(0, 2)] public int leftJointLane, rightJointLane;

    // private float _width;
    // /// <summary> The length of this bridge based on the maximum river lanes </summary>
    // public float Width
    // {
    //     get => _width;
    //     set
    //     {
    //         if (value <= 0) _width = 0;
    //         else if (value > GlobalRiverValues.RiverLaneCount) _width = GlobalRiverValues.RiverLaneCount;
    //         
    //         var v = Mathf.Round(value * 100f) / 100f;
    //         v = Mathf.Clamp(v, 0f, GlobalRiverValues.RiverLaneCount);
    //         _width = v;
    //     }
    // }
    //
    // private float _height;
    // public float Height
    // {
    //     get => _height;
    //     set
    //     {
    //         if (value <= 0) _height = 0;
    //         var v = Mathf.Round(value * 100f) / 100f;
    //         _height = Mathf.Clamp(v, 0f, GlobalRiverValues.SewerHeight);
    //     }
    // }
    //
    // private float _length;
    // public float Length
    // {
    //     get => _length;
    //     set
    //     {
    //         var v = Mathf.Round(value * 100f) / 100f;
    //         _length = v;
    //     }
    // }
}
