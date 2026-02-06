using System;
using UnityEngine;
using UnityEngine.Splines;

public class SewerEnvironmentManager : MonoBehaviour
{
    [SerializeField] private SplineAnimate[] sewerObjects;

    private void Start()
    {
        UpdateSpeed();
    }

    private void OnEnable()
    {
        River_Manager.Instance.OnRiverSpeedUpdate += UpdateSpeed;
    }

    private void OnDestroy()
    {
        River_Manager.Instance.OnRiverSpeedUpdate -= UpdateSpeed;
    }

    private void UpdateSpeed()
    {
        foreach (var item in sewerObjects)
        {
            item.MaxSpeed = River_Manager.Instance.RiverFlowSpeed;
        }
    }

    private void GenerateChoicePath()
    {
        
    }
}
