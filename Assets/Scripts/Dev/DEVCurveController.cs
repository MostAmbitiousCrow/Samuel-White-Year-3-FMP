using System;
using UnityEngine;

public class DEVCurveController : MonoBehaviour
{
    private static readonly int HorizontalCurve = Shader.PropertyToID("_HorizontalCurve");
    private static readonly int VerticalCurve = Shader.PropertyToID("_VerticalCurve");
    [SerializeField, Range(-0.01f, 0.01f)] private float horizontalCurve, verticalCurve;
    [SerializeField] private Material gameMaterial, waterMaterial;

    private void Update()
    {
        gameMaterial.SetFloat(HorizontalCurve, horizontalCurve);
        waterMaterial.SetFloat(HorizontalCurve, horizontalCurve);
        
        gameMaterial.SetFloat(VerticalCurve, verticalCurve);
        waterMaterial.SetFloat(VerticalCurve, verticalCurve);
    }
}
