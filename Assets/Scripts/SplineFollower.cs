using System;
using UnityEngine;
using UnityEngine.Splines;

public class SplineFollower : MonoBehaviour
{
    [SerializeField] private Transform followObject;
    [SerializeField] private SplineAnimate splineAnimate;

    private void Update()
    {
        splineAnimate.ElapsedTime = 1f;
    }
}
