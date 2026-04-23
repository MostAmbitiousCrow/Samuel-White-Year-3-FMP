using System;
using System.Collections.Generic;
using UnityEngine;

public class Section_Obstacle_Pipe_Object : Section_Obstacle_Object
{
    protected override void AdditionalDebug()
    {
        name = $"{ObjectType.Obstacle} - {sectionData.obstacleType}";
    
        if (sectionData.pipeObstacleData.connectedPipes.Length < 2) return;

        var points = new List<Vector3>();

        // Start pipe
        var pipe = sectionData.pipeObstacleData.connectedPipes[0];
        
        AssignPosition(pipe, out var pos);
        points.Add(pos);
        DrawItem(Color.cyan, Vector3.one, pos);
        
        // Draw Start Pipe Connection
        if (pipe.pipeConnection != NewPipeObstacleData.PipeConnection.None)
        {
            AssignConnectionPosition(pipe, out var connection);
            Debug.DrawLine(points[0], connection, Color.cyan);
        }

        // Pipe Joints
        foreach (var joint in sectionData.pipeObstacleData.pipeJoints)
        {
            AssignPosition(joint, out pos);
            points.Add(pos);
        }

        // End pipe
        pipe = sectionData.pipeObstacleData.connectedPipes[1];
        AssignPosition(pipe, out pos);
        points.Add(pos);

        // Draw lines
        for (int i = 1; i < points.Count; i++)
        {
            // Draw Lines. Last line must be blue.
            var color = (i == points.Count - 1) ? Color.blue : Color.cyan;
            Debug.DrawLine(points[i - 1], points[i], color);
        }

        // Draw End Pipe
        DrawItem(Color.blue, Vector3.one, points[^1]);

        // Draw End Pipe Connection
        if (pipe.pipeConnection != NewPipeObstacleData.PipeConnection.None)
        {
            AssignConnectionPosition(pipe, out var connection);
            Debug.DrawLine(points[^1], connection, Color.blue);
        }
    }

    /// <summary> Assigns the pipe to the point on the Global River Curve </summary>
    private void AssignPosition(NewPipeObstacleData.PipeData pipe, out Vector3 position)
    {
        riverManager.AssignToCurveSection
            (pipe.distance, pipe.lane, out Vector3 pos, out Quaternion rot);

        pos += transform.right * (pipe.lane - 1) * GlobalRiverValues.RiverLaneDistance / 16f;
        pos += (Vector3.up * pipe.height) + transform.position;
        position = pos;
    }

    private void AssignConnectionPosition(NewPipeObstacleData.ConnectedPipeData pipe, out Vector3 position)
    {
        float distance;
        Vector3 direction;
        switch (pipe.pipeConnection)
        {
            // Pass if no connection
            case NewPipeObstacleData.PipeConnection.None: throw new ArgumentOutOfRangeException();
            
            // Review Connections
            case NewPipeObstacleData.PipeConnection.Floor:
                distance = GlobalRiverValues.FloorToLaneDistance;
                direction = Vector3.down;
                break;
            case NewPipeObstacleData.PipeConnection.LeftWall:
                // If pipe is on the right or centre lane, multiply the distance
                if (pipe.lane >= 1)
                {
                    distance = 
                        GlobalRiverValues.RiverLaneDistance 
                        + 
                        ((GlobalRiverValues.RiverLaneDistance) 
                         * 
                         (pipe.lane +1));
                }
                else distance = GlobalRiverValues.WallToLaneDistance;
                direction = Vector3.left;
                break;
            case NewPipeObstacleData.PipeConnection.RightWall:
                // If pipe is on the left or centre lane, multiply the distance
                if (pipe.lane <= 1)
                {
                    distance = 
                        GlobalRiverValues.RiverLaneDistance 
                        + 
                        ((GlobalRiverValues.RiverLaneDistance) 
                         * 
                         (GlobalRiverValues.RiverLaneCount - pipe.lane));
                }
                else distance = GlobalRiverValues.WallToLaneDistance;
                direction = Vector3.right;
                break;
            case NewPipeObstacleData.PipeConnection.Ceiling:
                distance = GlobalRiverValues.CeilingToLaneDistance;
                direction = Vector3.up;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        riverManager.AssignToCurveSection
            (pipe.distance, pipe.lane, out Vector3 pos, out Quaternion rot);

        pos += transform.right * (pipe.lane - 1) * GlobalRiverValues.RiverLaneDistance / 16f;
        pos += ((direction * distance) + (Vector3.up * pipe.height)) + transform.position;
        position = pos;
    }
}
