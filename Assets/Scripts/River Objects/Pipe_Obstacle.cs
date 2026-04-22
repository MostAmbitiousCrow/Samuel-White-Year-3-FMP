using System;
using UnityEngine;

public class Pipe_Obstacle : River_Obstacle
{
    /// <summary> What direction does the pipe connect to on the sewer walls </summary>
    [Header("Pipe Content")]
    public Pipe_Obstacle_Data pipeData = new();
    [SerializeField] private Transform[] pipes;

    private void OnEnable()
    {
        foreach (var item in pipes)
        {
            item.gameObject.SetActive(false);
        }
        ConnectPipeToSurface();
    }

    // private void OnDisable()
    // {
    //     foreach (var item in pipes)
    //     {
    //         item.gameObject.SetActive(false);
    //     }  
    // }

    public void OverridePipeData(Pipe_Obstacle_Data overridedData)
    {
        pipeData = overridedData;
        ConnectPipeToSurface();
    }
    
    private void ConnectPipeToSurface()
    {
        // Calculate hitbox centre and size
        float pipesLength = pipeData.amount;
        float totalDistance = pipeData.distancePerPipe * pipesLength;
        boxCollider.center = pipesLength * 0.5f * pipeData.distancePerPipe * Vector3.right;
        boxCollider.size = new Vector3(totalDistance + 1, 1, 1);

        // Enable and set the position of additional art pipes
        for (int i = 0; i < pipesLength; i++)
        {
            Transform pipe = pipes[i];
            pipe.gameObject.SetActive(true);
            pipe.localPosition = (i + 1) * pipeData.distancePerPipe * Vector3.right;
        }
        
        // Debug.Log($"Pipe Obstacle Constructed. Pipes = {pipesLength}. Direction = {pipeData.pipeConnection}");
    }

    protected override void OnObjectPlaced()
    {
        // Set rotation based on pipe connection direction (Doing this after the object has been set on the river)
        transform.localRotation *= Quaternion.Euler(0, 0, (int)pipeData.pipeConnection * 90f);
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
