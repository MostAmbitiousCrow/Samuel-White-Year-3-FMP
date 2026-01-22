using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SewerEnvironmentScroll : MonoTimeBehaviour
{
    [SerializeField] private Transform[] sewerObjects;
    [Space]
    [SerializeField] private float returnDistance = -10f;

    [SerializeField] private float resetDistance = 100f;

    protected override void TimeUpdate()
    {
        foreach (var item in sewerObjects)
        {
            item.Translate(Vector3.back * (Time.deltaTime * River_Manager.Instance.CurrentRiverSpeed));

            if (item.localPosition.z < returnDistance) item.localPosition = new Vector3(item.localPosition.x, item.localPosition.y, resetDistance);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawSphere(transform.position + (Vector3.back * returnDistance), 2f);
        
        Gizmos.color = Color.white;
        Gizmos.DrawSphere(transform.position + (Vector3.back * resetDistance), 1f);
    }
}
