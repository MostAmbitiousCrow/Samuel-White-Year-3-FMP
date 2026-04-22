using System;
using sc.modeling.splines.runtime;
using UnityEngine;
using Random = UnityEngine.Random;

public class RiverObstacleBridge : River_Obstacle
{
    [Header("Bridge Data")]
    [SerializeField] private SplineMesher splineMesher;
    private ObstacleBridgeData _obstacleBridgeData;
    [SerializeField] private GameObject[] leftArtwork, rightArtwork;

    public void AssignObstacleData(ObstacleBridgeData data)
    {
        _obstacleBridgeData = data;
    }

    protected override void OnObjectPlaced()
    {
        base.OnObjectPlaced();

        // Position first knot to the wall
        var spline = splineMesher.splineContainer.Spline;
        Vector3 pos = new Vector3(0f, 0f, CalculateLaneToWallDistance());

        // Get the first knot by reference and set its position
        var knot0 = spline[0];      // Get the knot by value
        knot0.Position = pos;    // Set the position
        spline[0] = knot0;          // Set the updated knot back
        
        // Choose Left Artwork
        foreach (var art in leftArtwork) art.SetActive(false);
        var rand = Random.Range(0, leftArtwork.Length+1);
        if (rand < leftArtwork.Length)
        {
            var artwork = leftArtwork[rand];
            artwork.SetActive(true);
            artwork.transform.localPosition = pos;
        }

        // Position second knot
        pos = new Vector3(_obstacleBridgeData.Length, _obstacleBridgeData.Height, _obstacleBridgeData.Width);

        // Get the second knot by reference and set its position
        var knot1 = spline[1];      // Get the knot by value
        knot1.Position = pos;    // Set the position
        spline[1] = knot1;          // Set the updated knot back
        
        // Choose Right Artwork
        foreach (var art in rightArtwork) art.SetActive(false);
        rand = Random.Range(0, rightArtwork.Length+1);
        if (rand < rightArtwork.Length)
        {
            var artwork = rightArtwork[rand];
            artwork.SetActive(true);
            artwork.transform.localPosition = pos;
        }
        
        splineMesher.splineContainer.Spline.Clear();
        splineMesher.splineContainer.Spline.Add(spline);

        // === Rebuild the Spline Mesh ===
        splineMesher.Rebuild();

        // === Update the collider ===
        boxCollider.size = new Vector3(_obstacleBridgeData.Width, 1f, 2f);
        boxCollider.center = new Vector3(_obstacleBridgeData.Length / 2f, 0f, 0f);  // Example centre adjustment
    }

    private float CalculateLaneToWallDistance()
    {
        return currentLane switch
        {
            // If the object is on the left lane
            < 1 => GlobalRiverValues.WallToLaneDistance * -1,
            // If the object is on the right lane
            > 1 => GlobalRiverValues.WallToLaneDistance,
            // Don't allow centre lane
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}

[Serializable]
public class ObstacleBridgeData
{
    private float _width;
    /// <summary> The length of this bridge based on the maximum river lanes </summary>
    public float Width
    {
        get => _width;
        set
        {
            if (value <= 0) _width = 0;
            else if (value > GlobalRiverValues.RiverLaneCount) _width = GlobalRiverValues.RiverLaneCount;
            
            var v = Mathf.Round(value * 100f) / 100f;
            v = Mathf.Clamp(v, 0f, GlobalRiverValues.RiverLaneCount);
            _width = v;
        }
    }
    
    private float _height;
    public float Height
    {
        get => _height;
        set
        {
            if (value <= 0) _height = 0;
            var v = Mathf.Round(value * 100f) / 100f;
            _height = Mathf.Clamp(v, 0f, GlobalRiverValues.SewerHeight);
        }
    }
    
    private float _length;
    public float Length
    {
        get => _length;
        set
        {
            var v = Mathf.Round(value * 100f) / 100f;
            _length = v;
        }
    }
}
