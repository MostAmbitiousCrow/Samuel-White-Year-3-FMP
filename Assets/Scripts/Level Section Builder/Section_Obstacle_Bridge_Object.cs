using System.Collections.Generic;
using UnityEngine;

public class Section_Obstacle_Bridge_Object : Section_Obstacle_Object
{
    // [Line]
    // [Header("Bridge Data")]
    // [SerializeField, Range(0, 2)] private float bridgeWidth;
    // [SerializeField, Range(0, 5)] private int bridgeHeight;
    // [SerializeField, Range(0f, 100f)] private float bridgeLength;

    protected override void AdditionalDebug()
    {
        var bridgeData = sectionData.obstacleBridgeData;
        
        //TODO: Remove
        /*// Establish which side the bridge should start projecting from
        if (bridgeData.startRightSide) bridgeData.rightJointLane = GlobalRiverValues.RiverLaneCount - 1;
        else bridgeData.leftJointLane = GlobalRiverValues.RiverLaneCount - 1;*/

        // Always Centre the Bridge to the Centre of the River Lanes
        Lane = 1;
        Height = 2;
        
        var joints = new List<Vector3>();
        
        // Draw Right Joint
        var distance = GlobalRiverValues.RiverLaneDistance * (bridgeData.rightJointLane - 1);
        var pos = transform.position + (Vector3.right * distance);
        DrawItem(Color.cyan, Vector3.one, pos);
        joints.Add(pos);
        
        // Draw Left Joint
        distance = GlobalRiverValues.RiverLaneDistance * (bridgeData.leftJointLane - 1);
        pos = transform.position + (Vector3.right * distance);
        DrawItem(Color.cyan, Vector3.one, pos);
        joints.Add(pos);
        
        // Draw Debug Line
        Debug.DrawLine(joints[0], joints[^1], Color.cyan);
        
        // var wall = joints[0] + (Vector3.right * GlobalRiverValues.WallToLaneDistance);
        // Debug.DrawLine(joints[0], wall, Color.blue);
            
        // Draw Lines to Sewer Walls
        
        // If the left joint is on the furthest lane on the left
        if (bridgeData.leftJointLane == 0)
        {
            var wall = joints[^1] + (Vector3.left * GlobalRiverValues.WallToLaneDistance);
            Debug.DrawLine(joints[^1], wall, Color.blue);
        }
        // If the right joint is on the furthest lane on the right
        if (bridgeData.rightJointLane >= GlobalRiverValues.RiverLaneCount - 1)
        {
            var wall = joints[0] + (Vector3.right * GlobalRiverValues.WallToLaneDistance);
            Debug.DrawLine(joints[0], wall, Color.blue);
        }

        
        /*if (bridgeData.startRightSide)
        {
            var wall = joints[0] + (Vector3.right * GlobalRiverValues.WallToLaneDistance);
            Debug.DrawLine(joints[0], wall, Color.blue);
            
            // If the joint is on the furthest lane on the left
            if (bridgeData.leftJointLane != GlobalRiverValues.RiverLaneCount - 1) return;
            wall = joints[^1] + (Vector3.left * GlobalRiverValues.WallToLaneDistance);
            Debug.DrawLine(joints[^1], wall, Color.blue);
        }
        else
        {
            var wall = joints[^1] + (Vector3.left * GlobalRiverValues.WallToLaneDistance);
            Debug.DrawLine(joints[^1], wall, Color.blue);
            
            // If the joint is on the furthest lane on the right
            if (bridgeData.rightJointLane != GlobalRiverValues.RiverLaneCount - 1) return;
            wall = joints[0] + (Vector3.right * GlobalRiverValues.WallToLaneDistance);
            Debug.DrawLine(joints[0], wall, Color.blue);
        }*/
    }
}
