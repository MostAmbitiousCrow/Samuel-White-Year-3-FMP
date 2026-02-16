using UnityEngine;

public class SpiralTransitionController : MonoBehaviour
{
    // Cached Property Indexes
    private static readonly int S = Shader.PropertyToID(MaskString);
    private static readonly int Alpha = Shader.PropertyToID(AlphaString);
    private static readonly int Scale = Shader.PropertyToID(ScaleString);
    private static readonly int Rotation = Shader.PropertyToID(RotationString);
    private static readonly int Offset = Shader.PropertyToID(OffsetString);
    
    [SerializeField] private Material spiralMaterial;
    [Space] 
    private const string MaskString = "_MaskString";
    [SerializeField, Range(0f, 1f)] private float maskAlpha = 1f;
    private const string AlphaString = "_Alpha";
    [SerializeField, Range(0f, 1f)] private float alpha = 1f;
    [Space] private const string ScaleString = "_Scale";

    [SerializeField, Range(0f, 10f)] private float scale;
    private const string RotationString = "_Rotation";
    [SerializeField, Range(0f, 6.28f)] private float rotation;
    [Space]
    private const string OffsetString = "_Offset";
    [SerializeField] private Vector2 offset;
    
    private void Update()
    {
        spiralMaterial.SetFloat(S, maskAlpha);
        spiralMaterial.SetFloat(Alpha, alpha);
        
        spiralMaterial.SetFloat(Scale, scale);
        spiralMaterial.SetFloat(Rotation, rotation);
        
        spiralMaterial.SetVector(Offset, offset);
    }
}
