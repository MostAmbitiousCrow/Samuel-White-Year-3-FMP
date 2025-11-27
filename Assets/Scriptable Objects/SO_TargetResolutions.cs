using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ReferenceResolutions", menuName = "ScriptableObjects/GameSettings/ReferenceResolutions", order = 0)]
public class SO_TargetResolutions : ScriptableObject
{
    public AspectResolution[] asepectResolutions =
    {
        new (new (1920, 1080), new ("16:9")), new (new (2560, 1440), new ("16:9")),
        new (new (3840, 2160), new ("16:9")),
        
        new (new (1680, 1050), new ("16:10")), new (new (1920, 1200), new ("16:10")),
        
        new (new (1024, 768), new ("4:3")), new (new (1280, 960), new ("4:3")),
        
        new (new (2560, 1080), new ("21:9")), new (new (3440, 1440), new ("21:9")),
        
        new (new (1280, 720), new ("16:9")), new (new (1600, 900), new ("16:9")),
        new (new (1366, 768), new ("16:9"))
    };
}

[Serializable]
public class AspectResolution
{
    public string aspect;
    public Vector2Int resolution;

    public AspectResolution(Vector2Int vector2Int, string v)
    {
        resolution = vector2Int;
        aspect = v;
    }
}