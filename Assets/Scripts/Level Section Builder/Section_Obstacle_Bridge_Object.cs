using System;
using EditorAttributes;
using UnityEngine;

public class Section_Obstacle_Bridge_Object : Section_Obstacle_Object
{
    [Line]
    [Header("Bridge Data")]
    [SerializeField, Range(0, 2)] private float bridgeWidth;
    [SerializeField, Range(0, 5)] private int bridgeHeight;
    [SerializeField, Range(0f, 100f)] private float bridgeLength;

    protected override void AdditionalDebug()
    {
        base.AdditionalDebug();

        var bridgeData = sectionData.obstacleBridgeData;
        
        bridgeData.Width = bridgeWidth;
        bridgeWidth = bridgeData.Width;
        bridgeData.Height = bridgeHeight;
        bridgeHeight = (int)bridgeData.Height;
        bridgeData.Length = bridgeLength;
        bridgeLength = bridgeData.Length;

        if (Lane == 1)
        {
            Lane = 0;
            return;
        }

        if (bridgeData.Width > 0f)
        {
            Vector3 pos = Lane switch
            {
                // If the bridge is on the Right Lane
                > 1 => -transform.right * (bridgeData.Width * GlobalRiverValues.RiverLaneDistance),
                // If the bridge is on the Left Lane
                < 1 => transform.right * (bridgeData.Width * GlobalRiverValues.RiverLaneDistance),
                _ => throw new ArgumentOutOfRangeException()
            };
            // Add height
            pos += (Vector3.up * bridgeData.Height) + transform.position;
            pos += Vector3.forward * bridgeData.Length;
            
            Debug.DrawLine(transform.position, pos, Color.blue);
        
            // riverManager.AssignToCurveSection
            //     (Distance + bridgeData.Length, bridgeData.Width, out Vector3 pos, out Quaternion rot);
            DrawItem(Color.blue, Vector3.one, pos);
        }
        
        //TODO: Debug Draw the bridge to the sewer wall 
    }
}
