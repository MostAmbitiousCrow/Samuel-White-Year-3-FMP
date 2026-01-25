using System;
using UnityEngine;

public class Pipe_Obstacle : River_Obstacle
{
    [Header("Pipe Content")]
    /// <summary>
    /// What direction does the pipe connect to on the sewer walls
    /// </summary>
    public Pipe_Obstacle_Data PipeData = new();
    [SerializeField] BoxCollider _hitBox;
    [SerializeField] Transform[] _pipes;

    private void OnEnable()
    {
        foreach (var item in _pipes)
        {
            item.gameObject.SetActive(true);
        }
    }

    void OnDisable()
    {
        foreach (var item in _pipes)
        {
            item.gameObject.SetActive(false);
        }  
    }

    protected override void OnSpawn()
    {
        base.OnSpawn();
        ConnectPipeToSurface();
    }
    void ConnectPipeToSurface()
    {
        //// Set rotation based on pipe connection direction
        transform.localRotation = Quaternion.Euler(0, 0, (int)PipeData.pipeConnection * 90);

        // Calculate hitbox center and size
        float pipesLength = _pipes.Length;
        float totalDistance = PipeData.distancePerPipe * pipesLength;
        _hitBox.center = pipesLength * 0.5f * PipeData.distancePerPipe * Vector3.right;
        _hitBox.size = new Vector3(totalDistance + 1, 1, 1);

        // Enable and set the position of additional art pipes
        for (int i = 0; i < pipesLength; i++)
        {
            Transform pipe = _pipes[i];
            pipe.gameObject.SetActive(true);
            pipe.localPosition = (i + 1) * PipeData.distancePerPipe * Vector3.right;
        }
    }
}

[Serializable]
public class Pipe_Obstacle_Data
{
    /// <summary>
    /// Enum represetning the direction the pipe will repeat across each lane
    /// </summary>
    public enum PipeConnection
    { Left = 2, Top = 1, Bottom = 3, Right = 0 }
    /// <summary>
    /// What direction will the pipe
    /// </summary>
    public PipeConnection pipeConnection = PipeConnection.Top;
    /// <summary>
    /// Distance between each pipe. Modify only if the art requires it.
    /// </summary>
    public float distancePerPipe = 1.6f;
    /// <summary>
    /// How many Pipes will be arranged
    /// </summary>
    public int amount = 5;
}
