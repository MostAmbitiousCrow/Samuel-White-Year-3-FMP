using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PooledBlockObject : MonoBehaviour
{
    public EnvironmentController EnvironmentController { get; set; }
    [SerializeField] private Transform startPoint, endPoint;
    public Transform StartPoint => startPoint;
    public Transform EndPoint => endPoint;

    public  void OnSpawned()
    {

    }

    public  void OnRecycled()
    {
        
    }

    private void Update()
    {
        
    }
}
