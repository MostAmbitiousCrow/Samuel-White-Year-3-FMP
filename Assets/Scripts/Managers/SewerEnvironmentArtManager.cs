using System;
using System.Collections;
using sc.modeling.splines.runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

public class SewerEnvironmentArtManager : MonoBehaviour
{
    [Header("Spline Meshes")]
    [SerializeField] private SplineMesher sewerRiverSplineMesher;
    [SerializeField] private SplineMesher[] sewerWallSplineMeshers, sewerCeilingSplineMeshers;

    [Header("Sewer Art")]
    public SewerArtContent[] sewerArtContents = new SewerArtContent[4];
    [Serializable]
    public class SewerArtContent
    {
        public Environments levelType;
        public Mesh wallMesh;
        public Material wallMaterial;
        
        public Mesh ceilingMesh;
        public Material ceilingMaterial;
    }
    
    [Header("Dependencies")]
    private GameLevelManager _gameLevelManager;
    private SplineContainer _splineContainer;

    public delegate void EnvironmentUpdated();
    public static EnvironmentUpdated OnEnvironmentUpdated;
    

    private void Awake()
    {
        _gameLevelManager = FindFirstObjectByType<GameLevelManager>();
        _splineContainer = GetComponentInChildren<SplineContainer>();
    }

    private void OnEnable()
    {
        GameLevelManager.OnLevelLoaded += UpdateSewerEnvironment;
    }

    private void OnDisable()
    {
        GameLevelManager.OnLevelLoaded -= UpdateSewerEnvironment;
    }

    public void UpdateSewerEnvironment() //TODO Find a way to ease the massive frame drop when regenerating the environment
    {
        if (_gameLevelManager.GameCompleted) return; // TODO: TEMP Until I find a solution to levels loading after levels have already all been completed
        
        var data = _gameLevelManager.Levels[_gameLevelManager.CurrentLevel];
        var content = sewerArtContents[(int)data.environmentType];
        
        // Update the World Spline Container
        if (data.levelSpline.Count > 1) _splineContainer.Spline = data.levelSpline;

        // Update Sewer Wall Artwork
        foreach (var splineMesher in sewerWallSplineMeshers)
        {
            splineMesher.sourceMesh = content.wallMesh;
            splineMesher.GetComponent<MeshRenderer>().material = content.wallMaterial;
            splineMesher.Rebuild();
        }
        
        // Update Sewer Ceiling Artwork
        foreach (var splineMesher in sewerCeilingSplineMeshers)
        {
            splineMesher.sourceMesh = content.ceilingMesh;
            splineMesher.GetComponent<MeshRenderer>().material = content.ceilingMaterial;
            splineMesher.Rebuild();
        }
        
        // Rebuild Sewer River Artwork
        sewerRiverSplineMesher.Rebuild();
        
        // Update subscribers (River Manager updates its spline length when the environment is updated)
        OnEnvironmentUpdated?.Invoke();
    }

    /*
    private IEnumerator UpdateSewerEnvironmentRoutine()
    {
        var data = _gameLevelManager.Levels[_gameLevelManager.CurrentLevel];
        var content = sewerArtContents[(int)data.levelType];

        var time = new WaitForEndOfFrame();
        
        // Update the World Spline Container
        if (data.levelSpline.Count > 1) _splineContainer.Spline = data.levelSpline;

        // Update Sewer Wall Artwork
        foreach (var splineMesher in sewerWallSplineMeshers)
        {
            splineMesher.sourceMesh = content.wallMesh;
            splineMesher.GetComponent<MeshRenderer>().material = content.wallMaterial;
            splineMesher.Rebuild();
            yield return time;
        }
        
        // Update Sewer Ceiling Artwork
        foreach (var splineMesher in sewerCeilingSplineMeshers)
        {
            splineMesher.sourceMesh = content.ceilingMesh;
            splineMesher.GetComponent<MeshRenderer>().material = content.ceilingMaterial;
            splineMesher.Rebuild();
            yield return time;
        }
        
        // Rebuild Sewer River Artwork
        yield return time;
        sewerRiverSplineMesher.Rebuild();
    }
    */
}
